using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.RateLimiting;

namespace StreamVault.API.Middleware
{
    public static class SecurityMiddleware
    {
        public static IServiceCollection AddSecurityServices(this IServiceCollection services)
        {
            // Rate limiting
            services.AddRateLimiter(options =>
            {
                // Global rate limit
                options.AddGlobalPolicy("Global", policy =>
                {
                    policy.PermitLimit = 100;
                    policy.Window = TimeSpan.FromSeconds(60);
                    policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    policy.QueueLimit = 10;
                });

                // Authentication endpoints - stricter limits
                options.AddPolicy("Auth", policy =>
                {
                    policy.PermitLimit = 5;
                    policy.Window = TimeSpan.FromMinutes(1);
                    policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    policy.QueueLimit = 0;
                });

                // Upload endpoints - moderate limits
                options.AddPolicy("Upload", policy =>
                {
                    policy.PermitLimit = 10;
                    policy.Window = TimeSpan.FromMinutes(1);
                    policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    policy.QueueLimit = 5;
                });

                // API endpoints per user
                options.AddPolicy("PerUser", policy =>
                {
                    policy.PermitLimit = 1000;
                    policy.Window = TimeSpan.FromHours(1);
                    policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    policy.QueueLimit = 100;
                    policy.User = new RateLimitUserPartitionBuilder<string>(context =>
                    {
                        var userId = context.User?.Identity?.Name ?? "anonymous";
                        return userId;
                    });
                });
            });

            // CORS configuration
            services.AddCors(options =>
            {
                options.AddPolicy("Default", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "https://streamvault.app")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // 24 hours
                });

                options.AddPolicy("Admin", policy =>
                {
                    policy.WithOrigins("http://localhost:3001", "https://admin.streamvault.app")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromSeconds(86400));
                });

                options.AddPolicy("Public", policy =>
                {
                    policy.AllowAnyOrigin()
                          .WithMethods("GET", "OPTIONS")
                          .WithHeaders("Content-Type", "Authorization")
                          .SetPreflightMaxAge(TimeSpan.FromSeconds(3600)); // 1 hour
                });
            });

            // Security headers
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "XSRF-TOKEN";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.HeaderName = "X-XSRF-TOKEN";
            });

            return services;
        }

        public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder app)
        {
            // Security headers middleware
            app.Use(async (context, next) =>
            {
                // Add security headers
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Add("Permissions-Policy", 
                    "camera=(), microphone=(), geolocation=(), payment=()");
                
                // CSP header
                context.Response.Headers.Add("Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                    "font-src 'self' https://fonts.gstatic.com; " +
                    "img-src 'self' data: https:; " +
                    "media-src 'self' https:; " +
                    "connect-src 'self' https://api.stripe.com https://js.stripe.com; " +
                    "frame-src 'self' https://js.stripe.com; " +
                    "object-src 'none'; " +
                    "base-uri 'self'; " +
                    "form-action 'self'");

                await next();
            });

            // Rate limiting
            app.UseRateLimiter();

            // CORS
            app.UseCors("Default");

            // HTTPS redirection in production
            if (!app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            return app;
        }
    }

    // Custom rate limiting attributes
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RateLimitAuthAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RateLimitUploadAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RateLimitPerUserAttribute : Attribute { }

    // Input validation middleware
    public class InputValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public InputValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check for common attack patterns
            var suspiciousPatterns = new[]
            {
                "<script", "</script>", "javascript:", "vbscript:", "onload=", "onerror=",
                "eval(", "expression(", "url(", "import(", "@import", "behavior:",
                "binding(", "include(", "charset=", "base64", "unescape", "escape",
                "alert(", "confirm(", "prompt(", "document.cookie", "document.write"
            };

            // Check URL parameters
            foreach (var param in context.Request.Query)
            {
                if (ContainsSuspiciousContent(param.Value, suspiciousPatterns))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid input detected");
                    return;
                }
            }

            // Check request body for POST/PUT requests
            if (context.Request.Method == "POST" || context.Request.Method == "PUT")
            {
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (ContainsSuspiciousContent(body, suspiciousPatterns))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid input detected");
                    return;
                }
            }

            await _next(context);
        }

        private bool ContainsSuspiciousContent(string content, string[] patterns)
        {
            if (string.IsNullOrEmpty(content))
                return false;

            var lowerContent = content.ToLowerInvariant();
            return patterns.Any(pattern => lowerContent.Contains(pattern));
        }
    }

    // Request size limiting middleware
    public class RequestSizeLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly long _maxRequestSize;

        public RequestSizeLimitMiddleware(RequestDelegate next, long maxRequestSize = 100 * 1024 * 1024) // 100MB default
        {
            _next = next;
            _maxRequestSize = maxRequestSize;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var contentLength = context.Request.ContentLength;
            
            if (contentLength.HasValue && contentLength.Value > _maxRequestSize)
            {
                context.Response.StatusCode = 413;
                await context.Response.WriteAsync($"Request size exceeds limit of {_maxRequestSize / (1024 * 1024)}MB");
                return;
            }

            // For chunked encoding, we need to read the stream
            if (!contentLength.HasValue)
            {
                context.Request.EnableBuffering();
                var memoryStream = new MemoryStream();
                await context.Request.Body.CopyToAsync(memoryStream);
                
                if (memoryStream.Length > _maxRequestSize)
                {
                    context.Response.StatusCode = 413;
                    await context.Response.WriteAsync($"Request size exceeds limit of {_maxRequestSize / (1024 * 1024)}MB");
                    return;
                }

                memoryStream.Position = 0;
                context.Request.Body = memoryStream;
            }

            await _next(context);
        }
    }

    // IP blocking middleware for DDoS protection
    public class IPBlockingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Dictionary<string, DateTime> _blockedIPs;
        private readonly Dictionary<string, List<DateTime>> _requestCounts;
        private readonly object _lock = new object();

        public IPBlockingMiddleware(RequestDelegate next)
        {
            _next = next;
            _blockedIPs = new Dictionary<string, DateTime>();
            _requestCounts = new Dictionary<string, List<DateTime>>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIP = GetClientIP(context);
            
            // Clean up old entries
            CleanupOldEntries();

            // Check if IP is blocked
            lock (_lock)
            {
                if (_blockedIPs.ContainsKey(clientIP))
                {
                    var blockExpiry = _blockedIPs[clientIP];
                    if (DateTime.UtcNow < blockExpiry)
                    {
                        context.Response.StatusCode = 429;
                        await context.Response.WriteAsync("IP address temporarily blocked");
                        return;
                    }
                    else
                    {
                        _blockedIPs.Remove(clientIP);
                    }
                }
            }

            // Track request count
            lock (_lock)
            {
                if (!_requestCounts.ContainsKey(clientIP))
                {
                    _requestCounts[clientIP] = new List<DateTime>();
                }

                var now = DateTime.UtcNow;
                _requestCounts[clientIP].Add(now);

                // Remove requests older than 1 minute
                _requestCounts[clientIP].RemoveAll(dt => dt < now.AddMinutes(-1));

                // Block if too many requests
                if (_requestCounts[clientIP].Count > 1000) // 1000 requests per minute
                {
                    _blockedIPs[clientIP] = DateTime.UtcNow.AddMinutes(30); // Block for 30 minutes
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Too many requests - IP temporarily blocked");
                    return;
                }
            }

            await _next(context);
        }

        private string GetClientIP(HttpContext context)
        {
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            var xRealIP = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIP))
            {
                return xRealIP;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private void CleanupOldEntries()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                
                // Remove expired blocks
                var expiredBlocks = _blockedIPs.Where(kvp => kvp.Value < now).Select(kvp => kvp.Key).ToList();
                foreach (var ip in expiredBlocks)
                {
                    _blockedIPs.Remove(ip);
                }

                // Clean old request counts (older than 5 minutes)
                foreach (var kvp in _requestCounts.ToList())
                {
                    kvp.Value.RemoveAll(dt => dt < now.AddMinutes(-5));
                    if (kvp.Value.Count == 0)
                    {
                        _requestCounts.Remove(kvp.Key);
                    }
                }
            }
        }
    }

    // SQL Injection protection middleware
    public class SQLInjectionProtectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _sqlPatterns;

        public SQLInjectionProtectionMiddleware(RequestDelegate next)
        {
            _next = next;
            _sqlPatterns = new[]
            {
                "drop table", "truncate table", "delete from", "insert into", "update set",
                "union select", "exec(", "execute(", "sp_", "xp_", "0x", "char(", "ascii(",
                "substring(", "concat(", "cast(", "convert(", "declare @", "set @",
                "begin transaction", "commit transaction", "rollback transaction",
                "--", "/*", "*/", "';", "'--"
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check URL parameters
            foreach (var param in context.Request.Query)
            {
                if (ContainsSQLInjection(param.Value))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid input detected");
                    return;
                }
            }

            // Check request body
            if (context.Request.Method == "POST" || context.Request.Method == "PUT")
            {
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (ContainsSQLInjection(body))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid input detected");
                    return;
                }
            }

            await _next(context);
        }

        private bool ContainsSQLInjection(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var lowerInput = input.ToLowerInvariant();
            return _sqlPatterns.Any(pattern => lowerInput.Contains(pattern));
        }
    }
}

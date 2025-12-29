using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Services
{
    public interface ISupportService
    {
        Task<SupportTicket> CreateTicketAsync(CreateTicketRequest request);
        Task<SupportTicket> UpdateTicketAsync(Guid ticketId, UpdateTicketRequest request);
        Task<SupportTicket?> GetTicketAsync(Guid ticketId);
        Task<IEnumerable<SupportTicket>> GetTicketsAsync(Guid? tenantId = null, TicketStatus? status = null, TicketPriority? priority = null, int page = 1, int pageSize = 20);
        Task AssignTicketAsync(Guid ticketId, Guid assignedToUserId);
        Task CloseTicketAsync(Guid ticketId, string resolution);
        Task ReopenTicketAsync(Guid ticketId, string reason);
        Task AddTicketReplyAsync(Guid ticketId, string content, Guid userId, bool isInternal = false);
        Task<IEnumerable<SupportTicketReply>> GetTicketRepliesAsync(Guid ticketId);
        Task<SupportTicket> EscalateTicketAsync(Guid ticketId, string reason);
        Task<KnowledgeBaseArticle> CreateKnowledgeBaseArticleAsync(CreateKBArticleRequest request);
        Task<KnowledgeBaseArticle> UpdateKnowledgeBaseArticleAsync(Guid articleId, UpdateKBArticleRequest request);
        Task<KnowledgeBaseArticle?> GetKnowledgeBaseArticleAsync(Guid articleId);
        Task<IEnumerable<KnowledgeBaseArticle>> SearchKnowledgeBaseAsync(string query, Guid? tenantId = null);
        Task<IEnumerable<KnowledgeBaseArticle>> GetPopularArticlesAsync(int limit = 10);
        Task<CannedResponse> CreateCannedResponseAsync(CreateCannedResponseRequest request);
        Task<CannedResponse> UpdateCannedResponseAsync(Guid responseId, UpdateCannedResponseRequest request);
        Task<CannedResponse?> GetCannedResponseAsync(Guid responseId);
        Task<IEnumerable<CannedResponse>> GetCannedResponsesAsync(Guid? tenantId = null);
        Task DeleteCannedResponseAsync(Guid responseId);
        Task<EmailTemplate> CreateEmailTemplateAsync(CreateEmailTemplateRequest request);
        Task<EmailTemplate> UpdateEmailTemplateAsync(Guid templateId, UpdateEmailTemplateRequest request);
        Task<EmailTemplate?> GetEmailTemplateAsync(Guid templateId);
        Task<IEnumerable<EmailTemplate>> GetEmailTemplatesAsync();
        Task SendEmailFromTemplateAsync(Guid templateId, string toEmail, Dictionary<string, object> variables);
        Task<SupportMetrics> GetSupportMetricsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    public class SupportService : ISupportService
    {
        private readonly StreamVaultDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ITenantContext _tenantContext;

        public SupportService(StreamVaultDbContext context, IEmailService emailService, ITenantContext tenantContext)
        {
            _context = context;
            _emailService = emailService;
            _tenantContext = tenantContext;
        }

        public async Task<SupportTicket> CreateTicketAsync(CreateTicketRequest request)
        {
            var ticket = new SupportTicket
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                UserId = request.UserId,
                Subject = request.Subject,
                Description = request.Description,
                Category = request.Category,
                Priority = request.Priority,
                Status = TicketStatus.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Send confirmation email
            await SendTicketNotificationAsync(ticket, "created");

            // Auto-assign if possible
            await AutoAssignTicketAsync(ticket);

            return ticket;
        }

        public async Task<SupportTicket> UpdateTicketAsync(Guid ticketId, UpdateTicketRequest request)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                throw new ArgumentException("Ticket not found", nameof(ticketId));

            if (request.Subject != null) ticket.Subject = request.Subject;
            if (request.Description != null) ticket.Description = request.Description;
            if (request.Category != null) ticket.Category = request.Category;
            if (request.Priority != null) ticket.Priority = request.Priority;
            if (request.Status != null) ticket.Status = request.Status.Value;

            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await SendTicketNotificationAsync(ticket, "updated");

            return ticket;
        }

        public async Task<SupportTicket?> GetTicketAsync(Guid ticketId)
        {
            return await _context.SupportTickets
                .Include(t => t.User)
                .Include(t => t.AssignedTo)
                .Include(t => t.Tenant)
                .Include(t => t.Replies)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<IEnumerable<SupportTicket>> GetTicketsAsync(Guid? tenantId = null, TicketStatus? status = null, TicketPriority? priority = null, int page = 1, int pageSize = 20)
        {
            var query = _context.SupportTickets.AsQueryable();

            if (tenantId.HasValue)
                query = query.Where(t => t.TenantId == tenantId.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            return await query
                .Include(t => t.User)
                .Include(t => t.AssignedTo)
                .Include(t => t.Tenant)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AssignTicketAsync(Guid ticketId, Guid assignedToUserId)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                throw new ArgumentException("Ticket not found", nameof(ticketId));

            var user = await _context.Users.FindAsync(assignedToUserId);
            if (user == null)
                throw new ArgumentException("User not found", nameof(assignedToUserId));

            ticket.AssignedToId = assignedToUserId;
            ticket.Status = TicketStatus.InProgress;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await SendTicketNotificationAsync(ticket, "assigned");
        }

        public async Task CloseTicketAsync(Guid ticketId, string resolution)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                throw new ArgumentException("Ticket not found", nameof(ticketId));

            ticket.Status = TicketStatus.Closed;
            ticket.Resolution = resolution;
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await SendTicketNotificationAsync(ticket, "closed");
        }

        public async Task ReopenTicketAsync(Guid ticketId, string reason)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                throw new ArgumentException("Ticket not found", nameof(ticketId));

            ticket.Status = TicketStatus.Reopened;
            ticket.Resolution = null;
            ticket.ClosedAt = null;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await SendTicketNotificationAsync(ticket, "reopened");
        }

        public async Task AddTicketReplyAsync(Guid ticketId, string content, Guid userId, bool isInternal = false)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                throw new ArgumentException("Ticket not found", nameof(ticketId));

            var reply = new SupportTicketReply
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UserId = userId,
                Content = content,
                IsInternal = isInternal,
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportTicketReplies.Add(reply);
            
            // Update ticket status if customer replied
            if (!isInternal && ticket.Status == TicketStatus.Closed)
            {
                ticket.Status = TicketStatus.Reopened;
            }
            else if (!isInternal && ticket.Status == TicketStatus.Open)
            {
                ticket.Status = TicketStatus.InProgress;
            }

            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await SendReplyNotificationAsync(ticket, reply);
        }

        public async Task<IEnumerable<SupportTicketReply>> GetTicketRepliesAsync(Guid ticketId)
        {
            return await _context.SupportTicketReplies
                .Where(r => r.TicketId == ticketId)
                .Include(r => r.User)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<SupportTicket> EscalateTicketAsync(Guid ticketId, string reason)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                throw new ArgumentException("Ticket not found", nameof(ticketId));

            ticket.Status = TicketStatus.Escalated;
            ticket.Priority = TicketPriority.High;
            ticket.EscalationReason = reason;
            ticket.EscalatedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await SendTicketNotificationAsync(ticket, "escalated");

            return ticket;
        }

        public async Task<KnowledgeBaseArticle> CreateKnowledgeBaseArticleAsync(CreateKBArticleRequest request)
        {
            var article = new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                Title = request.Title,
                Content = request.Content,
                Summary = request.Summary,
                Category = request.Category,
                Tags = request.Tags ?? new List<string>(),
                IsPublished = request.IsPublished,
                Views = 0,
                HelpfulVotes = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.KnowledgeBaseArticles.Add(article);
            await _context.SaveChangesAsync();

            return article;
        }

        public async Task<KnowledgeBaseArticle> UpdateKnowledgeBaseArticleAsync(Guid articleId, UpdateKBArticleRequest request)
        {
            var article = await _context.KnowledgeBaseArticles.FindAsync(articleId);
            if (article == null)
                throw new ArgumentException("Article not found", nameof(articleId));

            if (request.Title != null) article.Title = request.Title;
            if (request.Content != null) article.Content = request.Content;
            if (request.Summary != null) article.Summary = request.Summary;
            if (request.Category != null) article.Category = request.Category;
            if (request.Tags != null) article.Tags = request.Tags;
            if (request.IsPublished.HasValue) article.IsPublished = request.IsPublished.Value;

            article.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return article;
        }

        public async Task<KnowledgeBaseArticle?> GetKnowledgeBaseArticleAsync(Guid articleId)
        {
            var article = await _context.KnowledgeBaseArticles.FindAsync(articleId);
            if (article != null)
            {
                article.Views++;
                await _context.SaveChangesAsync();
            }
            return article;
        }

        public async Task<IEnumerable<KnowledgeBaseArticle>> SearchKnowledgeBaseAsync(string query, Guid? tenantId = null)
        {
            var dbQuery = _context.KnowledgeBaseArticles
                .Where(a => a.IsPublished);

            if (tenantId.HasValue)
            {
                dbQuery = dbQuery.Where(a => a.TenantId == null || a.TenantId == tenantId.Value);
            }

            return await dbQuery
                .Where(a => a.Title.Contains(query) || 
                           a.Content.Contains(query) || 
                           a.Summary.Contains(query) ||
                           a.Tags.Any(t => t.Contains(query)))
                .OrderByDescending(a => a.HelpfulVotes)
                .ThenByDescending(a => a.Views)
                .Take(20)
                .ToListAsync();
        }

        public async Task<IEnumerable<KnowledgeBaseArticle>> GetPopularArticlesAsync(int limit = 10)
        {
            return await _context.KnowledgeBaseArticles
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.Views)
                .ThenByDescending(a => a.HelpfulVotes)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<CannedResponse> CreateCannedResponseAsync(CreateCannedResponseRequest request)
        {
            var response = new CannedResponse
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                Name = request.Name,
                Content = request.Content,
                Category = request.Category,
                Shortcuts = request.Shortcuts ?? new List<string>(),
                IsActive = true,
                UsageCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CannedResponses.Add(response);
            await _context.SaveChangesAsync();

            return response;
        }

        public async Task<CannedResponse> UpdateCannedResponseAsync(Guid responseId, UpdateCannedResponseRequest request)
        {
            var response = await _context.CannedResponses.FindAsync(responseId);
            if (response == null)
                throw new ArgumentException("Canned response not found", nameof(responseId));

            if (request.Name != null) response.Name = request.Name;
            if (request.Content != null) response.Content = request.Content;
            if (request.Category != null) response.Category = request.Category;
            if (request.Shortcuts != null) response.Shortcuts = request.Shortcuts;
            if (request.IsActive.HasValue) response.IsActive = request.IsActive.Value;

            response.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return response;
        }

        public async Task<CannedResponse?> GetCannedResponseAsync(Guid responseId)
        {
            var response = await _context.CannedResponses.FindAsync(responseId);
            if (response != null && response.IsActive)
            {
                response.UsageCount++;
                await _context.SaveChangesAsync();
            }
            return response;
        }

        public async Task<IEnumerable<CannedResponse>> GetCannedResponsesAsync(Guid? tenantId = null)
        {
            var query = _context.CannedResponses.Where(r => r.IsActive);

            if (tenantId.HasValue)
            {
                query = query.Where(r => r.TenantId == null || r.TenantId == tenantId.Value);
            }

            return await query
                .OrderBy(r => r.Category)
                .ThenBy(r => r.Name)
                .ToListAsync();
        }

        public async Task DeleteCannedResponseAsync(Guid responseId)
        {
            var response = await _context.CannedResponses.FindAsync(responseId);
            if (response == null)
                throw new ArgumentException("Canned response not found", nameof(responseId));

            response.IsActive = false;
            response.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<EmailTemplate> CreateEmailTemplateAsync(CreateEmailTemplateRequest request)
        {
            var template = new EmailTemplate
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Subject = request.Subject,
                HtmlContent = request.HtmlContent,
                TextContent = request.TextContent,
                Category = request.Category,
                Variables = request.Variables ?? new List<string>(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.EmailTemplates.Add(template);
            await _context.SaveChangesAsync();

            return template;
        }

        public async Task<EmailTemplate> UpdateEmailTemplateAsync(Guid templateId, UpdateEmailTemplateRequest request)
        {
            var template = await _context.EmailTemplates.FindAsync(templateId);
            if (template == null)
                throw new ArgumentException("Email template not found", nameof(templateId));

            if (request.Name != null) template.Name = request.Name;
            if (request.Subject != null) template.Subject = request.Subject;
            if (request.HtmlContent != null) template.HtmlContent = request.HtmlContent;
            if (request.TextContent != null) template.TextContent = request.TextContent;
            if (request.Category != null) template.Category = request.Category;
            if (request.Variables != null) template.Variables = request.Variables;
            if (request.IsActive.HasValue) template.IsActive = request.IsActive.Value;

            template.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return template;
        }

        public async Task<EmailTemplate?> GetEmailTemplateAsync(Guid templateId)
        {
            return await _context.EmailTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive);
        }

        public async Task<IEnumerable<EmailTemplate>> GetEmailTemplatesAsync()
        {
            return await _context.EmailTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task SendEmailFromTemplateAsync(Guid templateId, string toEmail, Dictionary<string, object> variables)
        {
            var template = await GetEmailTemplateAsync(templateId);
            if (template == null)
                throw new ArgumentException("Email template not found", nameof(templateId));

            // Replace variables in subject and content
            var subject = ReplaceVariables(template.Subject, variables);
            var htmlContent = ReplaceVariables(template.HtmlContent, variables);
            var textContent = ReplaceVariables(template.TextContent, variables);

            await _emailService.SendEmailAsync(toEmail, subject, htmlContent, textContent);
        }

        public async Task<SupportMetrics> GetSupportMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.SupportTickets.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(t => t.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(t => t.CreatedAt <= endDate.Value);

            var tickets = await query.ToListAsync();

            return new SupportMetrics
            {
                TotalTickets = tickets.Count,
                OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open),
                InProgressTickets = tickets.Count(t => t.Status == TicketStatus.InProgress),
                ClosedTickets = tickets.Count(t => t.Status == TicketStatus.Closed),
                AverageResponseTime = await CalculateAverageResponseTime(tickets),
                AverageResolutionTime = await CalculateAverageResolutionTime(tickets),
                CustomerSatisfactionScore = await CalculateSatisfactionScore(tickets),
                TicketsByCategory = tickets
                    .GroupBy(t => t.Category)
                    .Select(g => new TicketCategoryStat
                    {
                        Category = g.Key,
                        Count = g.Count()
                    })
                    .ToList(),
                TicketsByPriority = tickets
                    .GroupBy(t => t.Priority)
                    .Select(g => new TicketPriorityStat
                    {
                        Priority = g.Key,
                        Count = g.Count()
                    })
                    .ToList()
            };
        }

        private async Task AutoAssignTicketAsync(SupportTicket ticket)
        {
            // Find available support agents based on category and workload
            var availableAgent = await _context.Users
                .Where(u => u.Roles.Contains("Support") && u.IsActive)
                .OrderBy(u => _context.SupportTickets.Count(t => t.AssignedToId == u.Id && t.Status != TicketStatus.Closed))
                .FirstOrDefaultAsync();

            if (availableAgent != null)
            {
                await AssignTicketAsync(ticket.Id, availableAgent.Id);
            }
        }

        private async Task SendTicketNotificationAsync(SupportTicket ticket, string action)
        {
            // Send notification to user about ticket status change
            var template = await _context.EmailTemplates
                .FirstOrDefaultAsync(t => t.Category == "support" && t.Name == $"ticket_{action}");

            if (template != null)
            {
                var variables = new Dictionary<string, object>
                {
                    ["ticketId"] = ticket.Id,
                    ["subject"] = ticket.Subject,
                    ["status"] = ticket.Status.ToString(),
                    ["priority"] = ticket.Priority.ToString()
                };

                await SendEmailFromTemplateAsync(template.Id, ticket.User.Email, variables);
            }
        }

        private async Task SendReplyNotificationAsync(SupportTicket ticket, SupportTicketReply reply)
        {
            // Notify the other party about the reply
            var recipientId = reply.IsInternal ? ticket.UserId : ticket.AssignedToId;
            if (recipientId.HasValue)
            {
                var recipient = await _context.Users.FindAsync(recipientId.Value);
                if (recipient != null)
                {
                    var template = await _context.EmailTemplates
                        .FirstOrDefaultAsync(t => t.Category == "support" && t.Name == "ticket_reply");

                    if (template != null)
                    {
                        var variables = new Dictionary<string, object>
                        {
                            ["ticketId"] = ticket.Id,
                            ["subject"] = ticket.Subject,
                            ["replyAuthor"] = reply.User.FirstName + " " + reply.User.LastName
                        };

                        await SendEmailFromTemplateAsync(template.Id, recipient.Email, variables);
                    }
                }
            }
        }

        private string ReplaceVariables(string template, Dictionary<string, object> variables)
        {
            foreach (var variable in variables)
            {
                template = template.Replace($"{{{{{variable.Key}}}}}", variable.Value?.ToString() ?? "");
            }
            return template;
        }

        private async Task<TimeSpan> CalculateAverageResponseTime(List<SupportTicket> tickets)
        {
            var responseTimes = new List<TimeSpan>();
            
            foreach (var ticket in tickets)
            {
                var firstReply = await _context.SupportTicketReplies
                    .Where(r => r.TicketId == ticket.Id && !r.IsInternal)
                    .OrderBy(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (firstReply != null)
                {
                    responseTimes.Add(firstReply.CreatedAt - ticket.CreatedAt);
                }
            }

            return responseTimes.Any() ? TimeSpan.FromTicks((long)responseTimes.Average(rt => rt.Ticks)) : TimeSpan.Zero;
        }

        private async Task<TimeSpan> CalculateAverageResolutionTime(List<SupportTicket> tickets)
        {
            var closedTickets = tickets.Where(t => t.ClosedAt.HasValue).ToList();
            
            if (!closedTickets.Any())
                return TimeSpan.Zero;

            var resolutionTimes = closedTickets
                .Select(t => t.ClosedAt!.Value - t.CreatedAt)
                .ToList();

            return TimeSpan.FromTicks((long)resolutionTimes.Average(rt => rt.Ticks));
        }

        private async Task<double> CalculateSatisfactionScore(List<SupportTicket> tickets)
        {
            // This would typically be calculated from customer feedback
            return 4.2; // Placeholder out of 5
        }
    }

    // DTOs
    public class CreateTicketRequest
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public string Subject { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TicketCategory Category { get; set; }
        public TicketPriority Priority { get; set; }
    }

    public class UpdateTicketRequest
    {
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public TicketCategory? Category { get; set; }
        public TicketPriority? Priority { get; set; }
        public TicketStatus? Status { get; set; }
    }

    public class CreateKBArticleRequest
    {
        public Guid? TenantId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Summary { get; set; }
        public string Category { get; set; } = null!;
        public List<string>? Tags { get; set; }
        public bool IsPublished { get; set; } = false;
    }

    public class UpdateKBArticleRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Summary { get; set; }
        public string? Category { get; set; }
        public List<string>? Tags { get; set; }
        public bool? IsPublished { get; set; }
    }

    public class CreateCannedResponseRequest
    {
        public Guid? TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Category { get; set; } = null!;
        public List<string>? Shortcuts { get; set; }
    }

    public class UpdateCannedResponseRequest
    {
        public string? Name { get; set; }
        public string? Content { get; set; }
        public string? Category { get; set; }
        public List<string>? Shortcuts { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CreateEmailTemplateRequest
    {
        public string Name { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string HtmlContent { get; set; } = null!;
        public string TextContent { get; set; } = null!;
        public string Category { get; set; } = null!;
        public List<string>? Variables { get; set; }
    }

    public class UpdateEmailTemplateRequest
    {
        public string? Name { get; set; }
        public string? Subject { get; set; }
        public string? HtmlContent { get; set; }
        public string? TextContent { get; set; }
        public string? Category { get; set; }
        public List<string>? Variables { get; set; }
        public bool? IsActive { get; set; }
    }

    public class SupportMetrics
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ClosedTickets { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public TimeSpan AverageResolutionTime { get; set; }
        public double CustomerSatisfactionScore { get; set; }
        public List<TicketCategoryStat> TicketsByCategory { get; set; } = new();
        public List<TicketPriorityStat> TicketsByPriority { get; set; } = new();
    }

    public class TicketCategoryStat
    {
        public TicketCategory Category { get; set; }
        public int Count { get; set; }
    }

    public class TicketPriorityStat
    {
        public TicketPriority Priority { get; set; }
        public int Count { get; set; }
    }
}

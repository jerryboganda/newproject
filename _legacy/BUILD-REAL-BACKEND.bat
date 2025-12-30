@echo off
echo ========================================
echo Building REAL StreamVault Backend
echo ========================================
echo.

cd /d "%~dp0"

echo Creating clean solution...
dotnet new sln -n StreamVault --force

echo Creating projects...
dotnet new webapi -n StreamVault.Api --force
dotnet new classlib -n StreamVault.Application --force
dotnet new classlib -n StreamVault.Domain --force
dotnet new classlib -n StreamVault.Infrastructure --force
dotnet new classlib -n StreamVault.Shared --force

echo Adding projects to solution...
dotnet sln add StreamVault.Api/StreamVault.Api.csproj
dotnet sln add StreamVault.Application/StreamVault.Application.csproj
dotnet sln add StreamVault.Domain/StreamVault.Domain.csproj
dotnet sln add StreamVault.Infrastructure/StreamVault.Infrastructure.csproj
dotnet sln add StreamVault.Shared/StreamVault.Shared.csproj

echo Adding project references...
cd StreamVault.Api
dotnet add reference ../StreamVault.Application/StreamVault.Application.csproj
dotnet add reference ../StreamVault.Infrastructure/StreamVault.Infrastructure.csproj
dotnet add reference ../StreamVault.Shared/StreamVault.Shared.csproj

cd ../StreamVault.Application
dotnet add reference ../StreamVault.Domain/StreamVault.Domain.csproj
dotnet add reference ../StreamVault.Shared/StreamVault.Shared.csproj

cd ../StreamVault.Infrastructure
dotnet add reference ../StreamVault.Application/StreamVault.Application.csproj
dotnet add reference ../StreamVault.Domain/StreamVault.Domain.csproj
dotnet add reference ../StreamVault.Shared/StreamVault.Shared.csproj

cd ..

echo.
echo Installing NuGet packages...
cd StreamVault.Api
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package StackExchange.Redis
dotnet add package RabbitMQ.Client
dotnet add package Stripe.net
dotnet add package SendGrid
dotnet add package AWSSDK.S3

cd ../StreamVault.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package StackExchange.Redis
dotnet add package RabbitMQ.Client
dotnet add package Stripe.net
dotnet add package SendGrid

cd ..

echo.
echo Building solution...
dotnet build

echo.
echo ========================================
echo REAL StreamVault Backend Created!
echo ========================================
echo.
echo Next steps:
echo 1. Set up PostgreSQL database
echo 2. Configure appsettings.json
echo 3. Run migrations
echo 4. Start implementing features
echo.
pause

using DocuMind.Application.HttpClients;
using DocuMind.Application.Interfaces;
using DocuMind.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuMind.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        // Typed HTTP Client for Python AI Microservice
        var aiBaseUrl = configuration["AIService:BaseUrl"] ?? "http://localhost:8001";
        var aiInternalKey = configuration["AIService:InternalApiKey"] ?? "default_internal_secret";

        services.AddHttpClient<IAIServiceClient, AIServiceClient>(client =>
        {
            client.BaseAddress = new Uri(aiBaseUrl);
            client.DefaultRequestHeaders.Add("X-Internal-Api-Key", aiInternalKey);
            client.Timeout = TimeSpan.FromMinutes(3);
        });

        return services;
    }
}


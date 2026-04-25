using BankingApi.Repositories;
using BankingApi.Services;
using Microsoft.OpenApi.Models;

namespace BankingApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionService, TransactionService>();
        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Banking Transactions API",
                Version = "v1",
                Description = "A simple REST API for banking transactions"
            });
        });
        return services;
    }
}

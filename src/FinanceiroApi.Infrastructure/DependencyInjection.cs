using Amazon;
using Amazon.S3;
using FinanceiroApi.Application.Interfaces;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Caching;
using FinanceiroApi.Infrastructure.Data;
using FinanceiroApi.Infrastructure.ExternalServices.Email;
using FinanceiroApi.Infrastructure.ExternalServices.Payment;
using FinanceiroApi.Infrastructure.ExternalServices.Storage;
using FinanceiroApi.Infrastructure.Reports;
using FinanceiroApi.Infrastructure.Repositories;
using FinanceiroApi.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FinanceiroApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddRepositories()
            .AddCaching(configuration)
            .AddMessaging(configuration)
            .AddExternalServices(configuration)
            .AddReports();

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IAccountPayableRepository, AccountPayableRepository>();
        services.AddScoped<IAccountReceivableRepository, AccountReceivableRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<ICostCenterRepository, CostCenterRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IChartOfAccountRepository, ChartOfAccountRepository>();
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<IAccountingPeriodRepository, AccountingPeriodRepository>();
        services.AddScoped<ITaxEntryRepository, TaxEntryRepository>();
        services.AddScoped<ITaxPaymentRepository, TaxPaymentRepository>();
        services.AddScoped<IBankStatementRepository, BankStatementRepository>();
        services.AddScoped<IBankReconciliationRepository, BankReconciliationRepository>();

        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(connectionString, sql =>
                {
                    sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    sql.CommandTimeout(60);
                }));

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IPayrollRepository, PayrollRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IUnitOfWork, FinanceiroApi.Infrastructure.Persistence.UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddCaching(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' not found.");

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnection + ",abortConnect=false"));

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }

    private static IServiceCollection AddMessaging(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.AddScoped<IEventBusPublisher, EventBusPublisher>();
        services.AddHostedService<PayrollProcessedConsumer>();
        services.AddHostedService<PayrollCancelledConsumer>();
        services.AddHostedService<EmployeeCreatedConsumer>();
        return services;
    }

    private static IServiceCollection AddExternalServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        var emailSettings = configuration
            .GetSection(EmailSettings.SectionName)
            .Get<EmailSettings>() ?? new EmailSettings();

        if (!string.IsNullOrWhiteSpace(emailSettings.ApiKey))
            services.AddScoped<IEmailSender, SendGridEmailSender>();
        else
            services.AddScoped<IEmailSender, NullEmailSender>();

        services.Configure<StorageSettings>(configuration.GetSection(StorageSettings.SectionName));

        var storageSettings = configuration
            .GetSection(StorageSettings.SectionName)
            .Get<StorageSettings>() ?? new StorageSettings();

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(storageSettings.Region) };
            return new AmazonS3Client(storageSettings.AccessKey, storageSettings.SecretKey, config);
        });

        services.AddScoped<IStorageService, S3StorageService>();

        services.Configure<PaymentSettings>(configuration.GetSection(PaymentSettings.SectionName));

        services.AddHttpClient<IPaymentGateway, PaymentGatewayService>((sp, client) =>
        {
            var settings = configuration
                .GetSection(PaymentSettings.SectionName)
                .Get<PaymentSettings>() ?? new PaymentSettings();

            client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + '/');
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("X-Api-Key", settings.ApiKey);
        });

        return services;
    }

    private static IServiceCollection AddReports(this IServiceCollection services)
    {
        services.AddScoped<IFinancialReportService, FinancialReportService>();
        services.AddScoped<IPayslipGeneratorService, PayslipGeneratorService>();

        return services;
    }
}

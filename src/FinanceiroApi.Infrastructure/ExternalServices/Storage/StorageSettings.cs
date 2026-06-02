namespace FinanceiroApi.Infrastructure.ExternalServices.Storage;

public sealed class StorageSettings
{
    public const string SectionName = "Storage";

    public string BucketName { get; init; } = string.Empty;
    public string Region { get; init; } = "us-east-1";
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
}

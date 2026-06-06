namespace FinanceiroApi.CrossCutting.Services;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}

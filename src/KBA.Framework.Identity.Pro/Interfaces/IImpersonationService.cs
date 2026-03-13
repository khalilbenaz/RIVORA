namespace KBA.Framework.Identity.Pro.Interfaces;
public interface IImpersonationService {
    Task<string> ImpersonateUserAsync(string adminUserId, string targetTenantId, string targetUserId);
    Task<string> StopImpersonationAsync(string currentToken);
}

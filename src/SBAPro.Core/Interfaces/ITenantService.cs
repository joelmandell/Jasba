namespace SBAPro.Core.Interfaces;

/// <summary>
/// Service for managing tenant context in a multi-tenant application.
/// Provides the current tenant ID based on the authenticated user.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the current tenant ID from the authenticated user's claims.
    /// </summary>
    /// <returns>The tenant ID of the current user.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no tenant context is available.</exception>
    Guid GetTenantId();

    /// <summary>
    /// Attempts to get the current tenant ID.
    /// </summary>
    /// <returns>The tenant ID if available, otherwise null.</returns>
    Guid? TryGetTenantId();
}

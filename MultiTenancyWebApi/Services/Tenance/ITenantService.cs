using MultiTenancyWebApi.Services.Tenance.Settings;

namespace MultiTenancyWebApi.Services.Tenance
{
    public interface ITenantService
    {
        string? GetConnectionString();
        Tenant? GetCurrentTenant();
        string? GetDatabaseProvider();
    }
}

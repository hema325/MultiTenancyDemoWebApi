using Microsoft.Extensions.Options;
using MultiTenancyWebApi.Services.Tenance.Settings;

namespace MultiTenancyWebApi.Services.Tenance
{
    public class TenantService: ITenantService
    {
        private const string Tenant = "tenant";
        private readonly TenantSettings _tenantSettings;
        private readonly HttpContext _httpContext;

        private Tenant? _currentTenant;

        public TenantService(IOptions<TenantSettings> tenantSettings, IHttpContextAccessor httpContextAccessor)
        {
            _tenantSettings = tenantSettings.Value;
            _httpContext = httpContextAccessor.HttpContext!;

            if (_httpContext != null)
            {
                if (_httpContext.Request.Headers.TryGetValue(Tenant, out var id))
                    SetCurrentTenant(id);
                else
                    throw new Exception("Invalid tenant");
            }
        }

        private void SetCurrentTenant(string? tenantId)
        {
            _currentTenant = _tenantSettings.Tenants.FirstOrDefault(t => t.Id == tenantId);

            if (_currentTenant == null)
                throw new Exception("Invalid tenant");

            if (string.IsNullOrEmpty(_currentTenant.ConnectionString))
                _currentTenant.ConnectionString = _tenantSettings.Defaults.ConnectionString;
        }

        public Tenant? GetCurrentTenant() 
            => _currentTenant;


        public string? GetConnectionString()
            => _currentTenant?.ConnectionString ?? _tenantSettings.Defaults.ConnectionString;

        public string? GetDatabaseProvider()
            => _tenantSettings.Defaults.DatabaseProvider;
    }
}

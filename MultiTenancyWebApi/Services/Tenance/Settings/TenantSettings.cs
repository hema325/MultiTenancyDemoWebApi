namespace MultiTenancyWebApi.Services.Tenance.Settings
{
    public class TenantSettings
    {
        public const string SectionName = "Tenant";

        public Configuration Defaults { get; set; } = default!;
        public List<Tenant> Tenants { get; set; } = default!;
    }

    public class Configuration
    {
        public string DatabaseProvider { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;
    }

    public class Tenant
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;
    }
}

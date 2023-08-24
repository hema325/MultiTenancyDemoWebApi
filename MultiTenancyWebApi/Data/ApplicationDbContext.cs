using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MultiTenancyWebApi.Contracts;
using MultiTenancyWebApi.Entities;
using MultiTenancyWebApi.Services.Tenance;
using System.Linq.Expressions;

namespace MultiTenancyWebApi.Data
{
    public class ApplicationDbContext: DbContext
    {
        private readonly ITenantService _tenant;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenant) :base(options)
        {
            _tenant = tenant;   
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            AppendGlobalQueryFilter<IMustHaveTenant>(modelBuilder, t => t.TenantId == _tenant.GetCurrentTenant().Id);

        }

        private void AppendGlobalQueryFilter<TContract>(ModelBuilder modelBuilder, Expression<Func<TContract, bool>> globalQueryFilter)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var param = Expression.Parameter(entity.ClrType, "t");
                var body = ReplacingExpressionVisitor.Replace(globalQueryFilter.Parameters.Single(), param, globalQueryFilter.Body);

                if (entity.GetQueryFilter() is { } existingFilter)
                {
                    var existingBody = ReplacingExpressionVisitor.Replace(existingFilter.Parameters.Single(), param, existingFilter.Body);
                    body = Expression.AndAlso(body, existingBody);
                }

                entity.SetQueryFilter(Expression.Lambda(body, param));
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var provider = _tenant.GetDatabaseProvider();
            var connectionString = _tenant.GetConnectionString();

            if (provider.ToLower() == "mssql")
                optionsBuilder.UseSqlServer(connectionString);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<IMustHaveTenant>().Where(e => e.State == EntityState.Added);
            var tenantId = _tenant.GetCurrentTenant()?.Id;

            foreach (var entry in entries)
                entry.Entity.TenantId = tenantId;

            //Database.SetConnectionString(_tenant.GetConnectionString()); we can use it instead of using onConfiguring if the provider is the same in all the requests

            return base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<Product> Products { get; private set; }
    }
}

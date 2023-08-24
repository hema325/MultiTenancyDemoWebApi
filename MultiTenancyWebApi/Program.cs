using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MultiTenancyWebApi.Data;
using MultiTenancyWebApi.Services.Tenance;
using MultiTenancyWebApi.Services.Tenance.Settings;
using System.Runtime.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<TenantSettings>(builder.Configuration.GetSection(TenantSettings.SectionName));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
AddDbContext(builder.Services);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await InitializeDbAsync(app);

app.Run();


void AddDbContext(IServiceCollection services)
{
    var serviceProvider = services.BuildServiceProvider().CreateScope().ServiceProvider;
    var tenantSettings = serviceProvider.GetService<IOptions<TenantSettings>>().Value;


    if (tenantSettings.Defaults.DatabaseProvider.ToLower() == "mssql")
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer());

}

async Task InitializeDbAsync(WebApplication app)
{
    var serviceProvider = app.Services.CreateScope().ServiceProvider;
    var tenantSettings = serviceProvider.GetService<IOptions<TenantSettings>>().Value;
    var context = serviceProvider.GetService<ApplicationDbContext>();

    foreach(var tenant in tenantSettings.Tenants)
    {
        context.Database.SetConnectionString(tenant.ConnectionString ?? tenantSettings.Defaults.ConnectionString);

        if ((await context.Database.GetPendingMigrationsAsync()).Any())
            await context.Database.MigrateAsync();
    }
}
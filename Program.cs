using LocalGov360.Components;
using LocalGov360.Components.Account;
using LocalGov360.Data;
using LocalGov360.Data.Models;
using LocalGov360.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddScoped<IWorkflowFactory, WorkflowFactory>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IFormValidator, FormValidator>();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Warm up the database connection
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.OpenConnection();
        _ = context.Users.FirstOrDefault();
        context.Database.CloseConnection();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during database warm-up: {ex.Message}");
    }
}

// Add roles if they don't exist
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "admin", "developer", "CouncilAdmin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Add default user if not exists
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var org = context.Organisations.FirstOrDefault();
    if (org == null)
    {
        org = new Organisation
        {
            Id = Guid.NewGuid(),
            Name = "Default Organisation"
        };
        context.Organisations.Add(org);
        await context.SaveChangesAsync();
    }

    if (await userManager.FindByEmailAsync("admin@local.gov") == null)
    {
        var user = new ApplicationUser { UserName = "admin@local.gov", Email = "admin@local.gov" };
        await userManager.CreateAsync(user, "Test@1234");
        await userManager.AddToRoleAsync(user, "developer");
    }

    if (await userManager.FindByEmailAsync("admin@council.gov") == null)
    {
        var user = new ApplicationUser
        {
            UserName = "admin@council.gov",
            Email = "admin@council.gov",
            OrganisationId = org.Id
        };
        await userManager.CreateAsync(user, "Test@1234");
        await userManager.AddToRoleAsync(user, "admin");
    }
}

app.Run();
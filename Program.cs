using LocalGov360.Components;
using LocalGov360.Components.Account;
using LocalGov360.Data;
using LocalGov360.Data.Models;
using LocalGov360.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
builder.Services.AddHttpClient<TinggPaymentService>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddScoped<IWorkflowFactory, WorkflowFactory>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IFormValidator, FormValidator>();
builder.Services.AddScoped<TinggCallbackService>();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 1024;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1024 * 1024 * 1024;
    options.ValueLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
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
    var roles = new[] { "admin", "developer", "CouncilAdmin", "customer" };
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
        await userManager.AddToRoleAsync(user, "CouncilAdmin");
    }

    if (await userManager.FindByEmailAsync("bc@gmail.com") == null)
    {
        var user = new ApplicationUser
        {
            UserName = "bc@gmail.com",
            Email = "bc@gmail.com"
        };
        await userManager.CreateAsync(user, "Test@1234");
        await userManager.AddToRoleAsync(user, "customer");



       using (var serviceScope = app.Services.CreateScope())
{
    var db = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure DB exists (optional)
            db.Database.Migrate();

            var configId = Guid.Parse("94BEC6AE-3C27-4B92-A142-2211F8B245D8");

            if (!db.TinggConfigurations.Any(c => c.Id == configId))
            {
                db.TinggConfigurations.Add(new TinggConfiguration
                {
                    Id = configId,
                    OrganisationId = Guid.Parse("E30FB762-28A3-486D-8D3C-D696A4AE33C8"),
                    ApiBaseUrl = "https://sandbox.tingg.africa",
                    ApiKey = "ViR64sAFdAkvAoGaJqATWcW3tXREXGf",
                    AuthTokenRequestUrl = "https://api-approval.tingg.africa/v1/oauth/token/request",
                    CallbackUrl = "https://fd68ef02f459.ngrok-free.app/api/tinggcallback",
                    CheckoutRequestUrl = "https://api-approval.tingg.africa/v3/checkout-api/checkout/request",
                    ClientId = "c5549f4f-08da-4843-8dc4-32a567a1ef10",
                    ClientSecret = "mrmTNUmwhahKnVYTbSmIGXLJgaTuejBfLrBJqzv",
                    CountryCode = "ZMB",
                    CurrencyCode = "ZMW",
                    FailRedirectUrl = "https://fd68ef02f459.ngrok-free.app/your-fail-url",
                    PaymentModeCode = "STK_PUSH",
                    ServiceCode = "ECOBANK_ZAMBIA_COLLE",
                    SuccessRedirectUrl = "https://fd68ef02f459.ngrok-free.app/your-success-url"
                });

                db.SaveChanges();
            }
        }



        }
}

app.MapPost("/callback", async (HttpRequest request, TinggCallbackService callbackService) =>
{
    return await callbackService.ReceiveCallbackAsync(request);
});


app.Run();
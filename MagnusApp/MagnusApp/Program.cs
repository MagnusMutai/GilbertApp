using MagnusApp.Client.Pages;
using MagnusApp.Components;

using Microsoft.OpenApi.Models;
using MagnusApp.Shared.Configuration;
using MagnusApp.Repositories.EmailRepository;
using MagnusApp.Shared.Services.EmailService;
using MagnusApp.Data;
using MagnusApp.Components.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Syncfusion.Blazor;
using MagnusApp.Shared.Configuration.Aws;
using Amazon;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSyncfusionBlazor();

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzMzOTg2N0AzMjM1MmUzMDJlMzBWQmEwcXlNN2tkSVpoRE1WRlMvZVI0cWVFSy82OEt2bGx6emxPOS85bGRjPQ==");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();


builder.Services.AddControllers();

builder.Services.AddScoped<IEmailRepository, EmailRepository>();

builder.Services.AddHttpClient<IEmailService, EmailService>(client =>
{
    //dev baseaddress
    //client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BaseUri")!);
    //prod baseaddress
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ProdBaseUri")!);
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Example API",
        Version = "v1",
        Description = "An example of an ASP.NET Core Web API",
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Email = "example@example.com",
            Url = new Uri("https://localhost:44398/api/mail"),
        },
    });
});

//Contact Email form 
builder.Services.AddSingleton<IMailSettings, MailSettings>();

builder.Services.AddSingleton<IMessageOptions, MessageOptions>();

//Identity registration
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddGoogle(async options =>
{
bool isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
    if (isProduction)
    {
        //Get from aws secrets manager
        options.ClientId = await GoogleSecret.GetClientId();
        options.ClientSecret = await GoogleSecret.GetClientSecret();

    }

    //Get from secrets.json

    //options.ClientId = builder.Configuration.GetValue<string>("Authentication:Google:ClientId")!;
    //options.ClientSecret = builder.Configuration.GetValue<string>("Authentication:Google:ClientSecret")!;
    
    //Get from appsettings.json
    options.ClientId = builder.Configuration.GetValue<string>("Google:ClientId")!;
    options.ClientSecret = builder.Configuration.GetValue<string>("Google:ClientSecret")!;
})
.AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("MagnusAppAWSDb") ?? throw new InvalidOperationException("Connection string 'MagnusAWSDb' not found.");
//var connectionString = (DatabaseSecret.GetConnectionString()).ToString() ?? throw new InvalidOperationException("Connection string 'MagnusAWSDb' not found.");
builder.Services.AddDbContext<MagnusAppDbContext>(options =>
options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
       {
           options.SignIn.RequireConfirmedAccount = true;
           options.Tokens.ProviderMap.Add("CustomEmailConfirmation",
               new TokenProviderDescriptor(
                   typeof(CustomEmailConfirmationTokenProvider<ApplicationUser>)));
           options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
       
       })
      .AddEntityFrameworkStores<MagnusAppDbContext>()
      .AddSignInManager()
      .AddDefaultTokenProviders();

builder.Services.AddTransient<CustomEmailConfirmationTokenProvider<ApplicationUser>>();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, EmailSender>();

builder.Services.ConfigureApplicationCookie( options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(5);
    options.SlidingExpiration = true;
});

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromHours(3));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapControllers();
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MagnusApp.Client._Imports).Assembly);

// Add additional endpoints required by the  Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();

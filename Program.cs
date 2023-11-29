using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using patter_pal.dataservice.Azure;
using patter_pal.domain.Config;
using patter_pal.domain.Data;
using patter_pal.Logic;
using patter_pal.Logic.Cosmos;
using patter_pal.Logic.Interfaces;

static void ConfigureAuth(WebApplicationBuilder builder, AppConfig appConfig)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.AccessDeniedPath = "/Home/Index";
        options.LoginPath = "/Home/Index";
    })
    .AddGoogle(options =>
    {
        options.ClientId = appConfig.GoogleOAuthClientID;
        options.ClientSecret = appConfig.GoogleOAuthClientSecret;
    });
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("LoggedInPolicy", policy => policy.RequireAuthenticatedUser());
    });
}

static void ConfigureServices(WebApplicationBuilder builder, AppConfig appConfig)
{
    builder.Services.AddControllersWithViews();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSingleton(appConfig);

    // Data services
    builder.Services.AddSingleton<CosmosService>();
    builder.Services.AddScoped<IConversationService, ConversationService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IPronounciationAnalyticsService, PronounciationAnalyticsService>();
    
    // Logic services
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddSingleton<SpeechPronounciationService>();
    builder.Services.AddSingleton<OpenAiService>();
    builder.Services.AddSingleton<SpeechSynthesisService>();
    // Add client with lowered timeout for OpenAI
    builder.Services.AddHttpClient(Options.DefaultName, c => c.Timeout = TimeSpan.FromSeconds(appConfig.HttpTimeout));
}

// Start
var builder = WebApplication.CreateBuilder(args);
var appConfig = new AppConfig();

builder.Configuration.GetSection("AppConfig").Bind(appConfig);
appConfig.ValidateConfigInitialized();

ConfigureServices(builder, appConfig);
ConfigureAuth(builder, appConfig);

var app = builder.Build();

// WebSockets for live speech recognition and results
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(1),
    AllowedOrigins = { AppConfig.AppUrl }
};

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // In dev allow localhost
    webSocketOptions.AllowedOrigins.Add("https://localhost:7067");
    webSocketOptions.AllowedOrigins.Add("http://localhost:5189");
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseWebSockets(webSocketOptions);

app.MapControllerRoute(
       name: "WebSocket",
       pattern: "{controller=WebSocket}/{action=StartConversation}/{language}/{conversationId?}");
app.MapDefaultControllerRoute();
app.MapControllers();

// Init Cosmos DB
using (var scope = app.Services.CreateScope())
{
    CosmosService cosmosService = scope.ServiceProvider.GetService<CosmosService>()!;
    await cosmosService.InitializeService();

}

app.Run();
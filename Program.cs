using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Options;
using patter_pal.dataservice.Azure;
using patter_pal.Logic;
using patter_pal.Logic.Cosmos;
using patter_pal.Logic.Interfaces;
using patter_pal.Util;

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
    builder.Services.AddSingleton(sp => new CosmosService(appConfig.DbConnectionString, appConfig.CosmosDbDb1, appConfig.CosmosDbDb1C1, appConfig.CosmosDbDb1C1PK, appConfig.CosmosDbDb1C2, appConfig.CosmosDbDb1C2PK));
    builder.Services.AddScoped<IConversationService, ConversationService>();
    builder.Services.AddScoped<UserService>();
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
    // until #17 is done, use dummy conversation id

    /*
    IConversationService conversationService = scope.ServiceProvider.GetService<IConversationService>()!;
    string userId = "id";
    string userId2 = "aaaaa";

    // create new conversation
    var cd = ConversationData.Create(userId, "Test");
    await conversationService.AddConversationAsync(userId, cd);
    var cd2 = ConversationData.Create(userId2, "Test");
    await conversationService.AddConversationAsync(userId2, cd2);

    // talk + response
    await conversationService.AddChatAsync(userId, cd.Id, new ChatData(true, "request", "German"));
    await conversationService.AddChatAsync(userId, cd.Id, new ChatData(false, "response", "German"));

    // close app

    // reopen app, login, load all conversations (shallow, does not include messages)
    var cds = await conversationService.GetConversationsAsync(userId);

    // click on conversation => explicitly load
    var cd_full = await conversationService.GetConversationAndChatsAsync(userId, cds.First().Id);

    // talk + response + save
    await conversationService.AddChatAsync(userId, cd_full.Id, new ChatData(true, "request", "English"));
    await conversationService.AddChatAsync(userId, cd_full.Id, new ChatData(false, "response", "English"));

    // reopen app, login, load all conversations
    cds = await conversationService.GetConversationsAsync(userId);

    // click on conversation => explicitly load
    cd_full = await conversationService.GetConversationAndChatsAsync(userId, cds.First().Id);
    await conversationService.UpdateConversationAsync(userId, cd_full);

    // delete conversation
    await conversationService.DeleteConversationAsync(userId, cd_full.Id);
    //await cosmosService.DeleteAllUserData(userId);
    */

}

app.Run();
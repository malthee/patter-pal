using patter_pal.Logic;
using patter_pal.Util;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var appConfig = new AppConfig();
builder.Configuration.GetSection("AppConfig").Bind(appConfig);
builder.Services.AddSingleton(appConfig);
builder.Services.AddSingleton<SpeechPronounciationService>();
builder.Logging.AddConsole();

var app = builder.Build();
// WebSockets for Speech Recognition
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2),
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.UseWebSockets(webSocketOptions);

app.MapControllerRoute(
       name: "speech",
       pattern: "{controller=Speech}/{action=RecognizeWs}/{language}");
app.MapDefaultControllerRoute();

app.Run();

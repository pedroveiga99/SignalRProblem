using SignalRProblem;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Logging.AddConsole(options =>
{
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<Notifmanager>();
builder.Services.AddSingleton<NotificationHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.MapHub<NotificationHub>("/ws/notifications");
app.Services.GetRequiredService<NotificationHandler>();
await app.Services.GetRequiredService<Notifmanager>().Start();

app.Run();

using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddControllers();

// CORS setup - DEVELOPMENT ONLY
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});



var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseRouting();
app.UseWebSockets();   
app.MapHub<AlarmHub>("/alarmHub");
app.MapControllers();

app.Run();
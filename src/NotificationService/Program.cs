using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Dodavanje SignalR servisa
builder.Services.AddSignalR();

// 2. Dodavanje kontrolera (za API endpoint)
builder.Services.AddControllers();

// 3. CORS politika – dozvoli pristup sa bilo koje adrese (samo za razvoj)
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

// 4. Omogući CORS (mora biti prije MapHub i MapControllers)
app.UseCors("AllowFrontend");
app.UseRouting();
app.UseWebSockets();   // DODAJ OVO
app.MapHub<AlarmHub>("/alarmHub");
app.MapControllers();

// 7. Pokretanje aplikacije
app.Run();
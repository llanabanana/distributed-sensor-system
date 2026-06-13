using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Dodavanje SignalR servisa
builder.Services.AddSignalR();

// 2. Dodavanje kontrolera (za API endpoint)
builder.Services.AddControllers();

// 3. CORS politika – dozvoli pristup sa bilo koje adrese (samo za razvoj)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 4. Omogući CORS (mora biti prije MapHub i MapControllers)
app.UseCors("AllowAll");

// 5. Mapiranje SignalR Huba na putanju /alarmHub
app.MapHub<AlarmHub>("/alarmHub");

// 6. Mapiranje kontrolera (za /api/alarm/notify)
app.MapControllers();

// 7. Pokretanje aplikacije
app.Run();
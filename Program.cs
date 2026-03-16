using DSSStudentRisk.Data;
using DSSStudentRisk.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseMySql(
builder.Configuration.GetConnectionString("DefaultConnection"),
ServerVersion.AutoDetect(
builder.Configuration.GetConnectionString("DefaultConnection")
)));
builder.Services.AddScoped<AHPService>();
builder.Services.AddScoped<RiskService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutter",
        policy =>
        {
            policy.WithOrigins("http://localhost:50564")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("AllowFlutter");
app.MapControllers();

app.Run();
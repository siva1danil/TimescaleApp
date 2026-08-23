using Data;

using Microsoft.EntityFrameworkCore;

using Services.Implementations;
using Services.Interfaces;

using WebApi.ExceptionHandlers;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("TimescaleAppDatabase")
    ?? throw new InvalidOperationException("Connection string 'TimescaleAppDatabase' is not configured.");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<CsvValidationExceptionHandler>();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddScoped<IFileImportService, FileImportService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IValueService, ValueService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

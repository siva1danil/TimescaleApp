using Data;

using Microsoft.EntityFrameworkCore;

using Services.Implementations;
using Services.Interfaces;

using WebApi.ExceptionHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<CsvValidationExceptionHandler>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TimescaleApp"));
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

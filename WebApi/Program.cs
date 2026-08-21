using Services.Implementations;
using Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IFileImportService, FileImportService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IValueService, ValueService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

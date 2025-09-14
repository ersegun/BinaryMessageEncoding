using BinaryMessageEncodingAPI.Middleware;
using BinaryMessageEncodingAPI.Services;
using BinaryMessageEncodingAPI.Services.Validation;
using FluentValidation;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Options & services
builder.Services.Configure<MessageOptions>(builder.Configuration.GetSection("Codec"));
builder.Services.AddScoped<IMessageCodec, MessageCodec>();
builder.Services.AddValidatorsFromAssemblyContaining<MessageValidator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Binary Message Encoding API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();

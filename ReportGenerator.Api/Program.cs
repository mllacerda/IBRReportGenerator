
using ReportGenerator.Api.Infrastructure.Messaging;
using ReportGenerator.Domain.Interfaces;
using ReportGenerator.Domain.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "IBR Report Generator API", Version = "v1" });
    c.EnableAnnotations();
});
//RabbitMQ
builder.Services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();
builder.Services.AddSingleton<IMessageQueueService, RabbitMQService>();
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
   //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReportGenerator API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
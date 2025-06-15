
using ReportGenerator.Api.Infrastructure.Messaging;
using ReportGenerator.Domain.Interfaces;
using ReportGenerator.Domain.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new() { Title = "ReportGenerator API", Version = "v1" }); });

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
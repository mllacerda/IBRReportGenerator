
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ReportGenerator API", Version = "v1" });
});

//RabbitMQ
builder.Services.AddSingleton<ReportGenerator.Domain.Interfaces.IMessageQueueService, ReportGenerator.Api.Infrastructure.Messaging.RabbitMQService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReportGenerator API v1"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
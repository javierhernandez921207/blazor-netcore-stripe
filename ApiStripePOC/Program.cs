using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Stripe Configuration
// Set your secret key. Remember to switch to your live secret key in production.
// See your keys here: https://dashboard.stripe.com/apikeys
builder.Services.Configure<StripeClientOptions>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:ApiKey"];
var options = new PaymentMethodDomainCreateOptions { DomainName = "f82-185-197-192-87.ngrok-free.app" };
var service = new PaymentMethodDomainService();
service.Create(options);
// End Stripe Configuration

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod() // Permite todos los métodos (POST, GET, etc.)
               .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

using NimblePros.Customers.Web.Customers; // Namespace loaded

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<CustomerData>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Summon customer endpoints
app.MapCustomerEndpoints();


app.Run();


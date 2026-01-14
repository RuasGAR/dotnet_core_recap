using NimblePros.Customers.Web.Customers; // Namespace loaded


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<CustomerData>();

// Addind config service
var mailSettings = builder.Configuration.GetSection(nameof(MailSettings)).Get<MailSettings>();
builder.Services.AddSingleton(mailSettings);


builder.Services.AddTransient<IEmailMessageFactory, EmailMessageFactory>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddTransient<IEmailSenderService, ConsoleOnlyEmailSenderService>();
}
else
{
    builder.Services.AddTransient<IEmailSenderService, MailKitEmailSenderService>();
}
builder.Services.AddTransient<ICustomerEmailService, CustomerEmailService>();
builder.Services.AddTransient<CustomerEmailService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Summon customer endpoints
app.MapCustomerEndpoints();


app.Run();


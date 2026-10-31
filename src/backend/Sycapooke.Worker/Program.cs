using Sycapooke.Application;
using Sycapooke.Infrastructure;
using Sycapooke.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<CheckoutJobOptions>(builder.Configuration.GetSection("Checkout"));
builder.Services.Configure<OrderJobOptions>(builder.Configuration.GetSection("Orders"));
builder.Services.Configure<EmailDispatchJobOptions>(builder.Configuration.GetSection("Notifications"));
builder.Services.Configure<DraftCleanupJobOptions>(builder.Configuration.GetSection("Listings:DraftCleanup"));
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<ReservationExpiryJob>();
builder.Services.AddHostedService<ShipDeadlineJob>();
builder.Services.AddHostedService<OrderCompletionJob>();
builder.Services.AddHostedService<RefundRetryJob>();
builder.Services.AddHostedService<EraseClosedCustomersJob>();
builder.Services.AddHostedService<EmailDispatchJob>();
builder.Services.AddHostedService<ShipReminderJob>();
builder.Services.AddHostedService<DraftCleanupJob>();

var host = builder.Build();
host.Run();

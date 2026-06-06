using Microsoft.EntityFrameworkCore;
using Quartz;
using AlertNotificationSystem.Data;
using AlertNotificationSystem.Services;
using AlertNotificationSystem.Strategies;
using AlertNotificationSystem.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new() { Title = "Alert Notification System", Version = "v1" });
});

// Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// HttpClient for API calls from Blazor
builder.Services.AddHttpClient("API", client =>
{
	client.BaseAddress = new Uri("https://localhost:5001");
});

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Notification Strategies
builder.Services.AddScoped<INotificationStrategy, EmailNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, SlackNotificationStrategy>();
builder.Services.AddScoped<NotificationService>();

// API Service for Blazor
builder.Services.AddScoped<ApiService>();

// RSS Feed Service
builder.Services.AddScoped<RssFeedService>();
builder.Services.AddHttpClient();

// Quartz for background jobs
builder.Services.AddQuartz(q =>
{
	q.UseMicrosoftDependencyInjectionJobFactory();

	var jobKey = new JobKey("NotificationJob");
	q.AddJob<NotificationJob>(opts => opts.WithIdentity(jobKey));

	q.AddTrigger(opts => opts
		.ForJob(jobKey)
		.WithIdentity("NotificationJob-trigger")
		.WithSimpleSchedule(x => x
			.WithIntervalInSeconds(30)
			.RepeatForever()));

	var rssFeedJobKey = new JobKey("RssFeedCheckJob");
	q.AddJob<RssFeedCheckJob>(opts => opts.WithIdentity(rssFeedJobKey));

	q.AddTrigger(opts => opts
		.ForJob(rssFeedJobKey)
		.WithIdentity("RssFeedCheckJob-trigger")
		.WithSimpleSchedule(x => x
			.WithIntervalInMinutes(5)
			.RepeatForever()));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	db.Database.EnsureCreated();
}

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

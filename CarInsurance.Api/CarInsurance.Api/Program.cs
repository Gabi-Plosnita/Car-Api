using CarInsurance.Api.Data;
using CarInsurance.Api.Jobs;
using CarInsurance.Api.Middleware;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;
using TimeZoneConverter;

var builder = WebApplication.CreateBuilder(args);

var appTz = TZConvert.GetTimeZoneInfo("Europe/Bucharest");
builder.Services.AddSingleton(appTz);

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<ICarValidatorService, CarValidatorService>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();
builder.Services.AddScoped<IInsuranceValidatorService, InsuranceValidatorService>();
builder.Services.AddScoped<IClaimService, ClaimService>();

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddScoped<IPolicyExpirationProcessor, PolicyExpirationProcessor>();
builder.Services.AddHostedService<PolicyExpirationWorker>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure DB and seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	//db.Database.EnsureCreated();  No longer needed since we just need to apply the migrations to be up to date with the database
	db.Database.Migrate();
	SeedData.EnsureSeeded(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

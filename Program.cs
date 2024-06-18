using Microsoft.EntityFrameworkCore;
using SegalAPI.Data;
using SegalAPI.Interfaces;
using SegalAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();  // Register HttpClient
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

var env = builder.Environment;
var configuration = builder.Configuration;

// Use Middleware to handle exceptions
if (env.IsDevelopment() || configuration.GetValue<bool>("EnvironmentSettings:ShowDeveloperExceptions"))
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseMiddleware<ExceptionMiddleware>();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler("/Home/Error");
app.UseHsts();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Redirect root URL to Swagger UI
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger");
        return;
    }
    await next();
});

// Get the port from the environment variable or use a default port (e.g., 5000)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";

app.Run($"http://*:{port}");

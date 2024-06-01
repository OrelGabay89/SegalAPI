var builder = WebApplication.CreateBuilder(args);

// Set up Kestrel and other services
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Check if the PORT environment variable is set (which it will be on Heroku)
    var port = Environment.GetEnvironmentVariable("PORT");
    if (string.IsNullOrEmpty(port))
    {
        // If no PORT variable is found, default to 5001 for local development
        serverOptions.ListenLocalhost(5001);
    }
    else
    {
        // Use the Heroku-assigned port when running on Heroku
        serverOptions.ListenAnyIP(int.Parse(port));
    }
});

// Add services to the container.
builder.Services.AddControllers();

// Additional configuration...

var app = builder.Build();

// Configure the HTTP request pipeline and other middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

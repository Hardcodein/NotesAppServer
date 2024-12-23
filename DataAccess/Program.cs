var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; // Èãíîðèðóåì null-çíà÷åíèÿ
}); 


builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DbContextService>();
builder.Services.AddTransient<NoteRepositoryService>();
builder.Services.AddTransient<UserRepositoryService>();

builder.Services.AddCors(options =>

{   
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:8099");
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
    options.AddDefaultPolicy(policy =>
    {

        policy.WithOrigins("http://localhost:7777");
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
await using var dbContext = scope.ServiceProvider.GetService<DbContextService>();
await dbContext!.Database.EnsureCreatedAsync();

    
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors();
app.MapControllers();

app.Run();

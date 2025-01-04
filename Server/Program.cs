using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;
using ServerLibrary.Repositories.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//
builder.Services.AddDbContext<AppDbcontext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException
        ("SORRY, YOUR CONNECTION NOT FOUND"));
});

builder.Services.Configure<JwtSection>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IUserAccount, UserAccountRepository>();
// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllBlazorWasm", builder =>
    {
        builder.WithOrigins("https://localhost:5245", "https://localhost:7239")
        .AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllBlazorWasm");
app.UseAuthorization();

app.MapControllers();

app.Run();

using BackendAPI.Application.Interfaces;
using BackendAPI.Application.Services;
using BackendAPI.Application.Mapping;
using BackendAPI.Domain.Interfaces;
using BackendAPI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<GitHubApiService>();


builder.Services.AddScoped<IRepositoryService, GitHubApiService>();
builder.Services.AddScoped<IRepositoryAppService, RepositoryAppService>();
builder.Services.AddScoped<IRelevanceCalculator, RelevanceCalculator>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowAngular");

app.UseAuthorization();
app.MapControllers();

app.Run();
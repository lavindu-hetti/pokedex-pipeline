using PokemonAPI.Data;
using PokemonAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register DB
builder.Services.AddDbContext<AppDbContext>();

// ✅ Register Service
builder.Services.AddScoped<PokemonDbService>();
builder.Services.AddHttpClient<PokemonLiveService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();

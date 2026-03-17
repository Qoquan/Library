using Library.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Singletons en mémoire (pas de base de données)
builder.Services.AddSingleton<IUserStore, UserStore>();
builder.Services.AddSingleton<IUserBookStore, UserBookStore>();

// HttpClient pour OpenLibrary
builder.Services.AddHttpClient<IOpenLibraryService, OpenLibraryService>();

builder.Services.AddCors(options =>
    options.AddPolicy("All", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("All");
app.MapControllers();

Console.WriteLine("=== Library API démarrée ===");
Console.WriteLine("Swagger : http://localhost:5000/swagger");
Console.WriteLine("API     : http://localhost:5000/api/books");

app.Run();

public partial class Program { }
using Microsoft.EntityFrameworkCore;
using Library.API.Data;
using Library.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlite("Data Source=library.db"));

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddHttpClient<IOpenLibraryService, OpenLibraryService>();

builder.Services.AddCors(options =>
    options.AddPolicy("All", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("All");
app.MapControllers();

Console.WriteLine("=== Library API démarrée ===");
Console.WriteLine("Swagger : http://localhost:5000/swagger");
Console.WriteLine("API     : http://localhost:5000/api/books");

app.Run();

public partial class Program { }
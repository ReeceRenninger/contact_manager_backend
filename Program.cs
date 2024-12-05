using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Configure EF Core with SQL Server
builder.Services.AddDbContext<ContactContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

// Ensure the database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ContactContext>();
    dbContext.Database.EnsureCreated();
}

app.MapGet("/contacts", async (ContactContext db) =>
{
    return await db.Contacts.ToListAsync();
})
.WithName("GetContacts");

app.MapGet("/contacts/{id}", async (int id, ContactContext db) =>
{
    var contact = await db.Contacts.FindAsync(id);
    return contact is not null ? Results.Ok(contact) : Results.NotFound();
})
.WithName("GetContactById");

app.MapPost("/contacts", async (Contact contact, ContactContext db) =>
{
    db.Contacts.Add(contact);
    await db.SaveChangesAsync();
    return Results.Created($"/contacts/{contact.Id}", contact);
})
.WithName("CreateContact");

app.MapPut("/contacts/{id}", async (int id, Contact updatedContact, ContactContext db) =>
{
    var contact = await db.Contacts.FindAsync(id);
    if (contact is null) return Results.NotFound();

    contact.Name = updatedContact.Name;
    contact.Email = updatedContact.Email;
    contact.Phone = updatedContact.Phone;

    await db.SaveChangesAsync();
    return Results.Ok(contact);
})
.WithName("UpdateContact");

app.MapDelete("/contacts/{id}", async (int id, ContactContext db) =>
{
    var contact = await db.Contacts.FindAsync(id);
    if (contact is null) return Results.NotFound();

    db.Contacts.Remove(contact);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteContact");

app.Run();

public record Contact
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}

public class ContactContext : DbContext
{
    public ContactContext(DbContextOptions<ContactContext> options) : base(options) { }

    public DbSet<Contact> Contacts { get; set; }
}

public static class ContactEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder ContactGet(this IEndpointRouteBuilder endpoints, string pattern, Delegate handler)
    {
        return endpoints.MapGet(pattern, handler);
    }

    public static IEndpointConventionBuilder ContactPost(this IEndpointRouteBuilder endpoints, string pattern, Delegate handler)
    {
        return endpoints.MapPost(pattern, handler);
    }

    public static IEndpointConventionBuilder ContactPut(this IEndpointRouteBuilder endpoints, string pattern, Delegate handler)
    {
        return endpoints.MapPut(pattern, handler);
    }

    public static IEndpointConventionBuilder ContactDelete(this IEndpointRouteBuilder endpoints, string pattern, Delegate handler)
    {
        return endpoints.MapDelete(pattern, handler);
    }
}


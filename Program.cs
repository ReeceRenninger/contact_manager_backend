var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var contacts = new List<Contact>
{
    new Contact { Id = 1, Name = "John Doe", Email = "john.doe@example.com", Phone = "123-456-7890" },
    new Contact { Id = 2, Name = "Jane Smith", Email = "jane.smith@example.com", Phone = "111-111-1111" }
};

app.ContactGet("/contacts", () => contacts)
    .WithName("GetContacts");

app.ContactGet("/contacts/{id}", (int id) =>
{
    var contact = contacts.FirstOrDefault(c => c.Id == id);
    if (contact == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(contact);
})
.WithName("GetContactById");

app.ContactPost("/contacts", (Contact contact) =>
{
    contact.Id = contacts.Max(c => c.Id) + 1;
    contacts.Add(contact);
    return Results.Created($"/contacts/{contact.Id}", contact);
})
.WithName("CreateContact");

app.ContactPut("/contacts/{id}", (int id, Contact contact) =>
{
    var existingContact = contacts.FirstOrDefault(c => c.Id == id);
    if (existingContact == null)
    {
        return Results.NotFound();
    }
    existingContact.Name = contact.Name;
    existingContact.Email = contact.Email;
    existingContact.Phone = contact.Phone;
    return Results.Ok(existingContact);
})
.WithName("UpdateContact");

app.ContactDelete("/contacts/{id}", (int id) =>
{
    var contact = contacts.FirstOrDefault(c => c.Id == id);
    if (contact is null) return Results.NotFound();

    contacts.Remove(contact);
    return Results.NoContent();
})
.WithName("DeleteContact");

app.Run();

record Contact
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
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
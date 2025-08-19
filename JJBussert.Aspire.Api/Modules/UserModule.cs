using Carter;
using JJBussert.Aspire.Data;
using JJBussert.Aspire.Domain;
using Microsoft.EntityFrameworkCore;

namespace JJBussert.Aspire.Api.Modules;

public class UserModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", GetUsers);
        group.MapGet("/{id:int}", GetUser);
        group.MapPost("/", CreateUser);
        group.MapPut("/{id:int}", UpdateUser);
        group.MapDelete("/{id:int}", DeleteUser);
    }

    private static async Task<IResult> GetUsers(AspireDbContext context)
    {
        var users = await context.Users
            .Include(u => u.Organization)
            .ToListAsync();
        return Results.Ok(users);
    }

    private static async Task<IResult> GetUser(int id, AspireDbContext context)
    {
        var user = await context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id);
        
        return user is not null ? Results.Ok(user) : Results.NotFound();
    }

    private static async Task<IResult> CreateUser(User user, AspireDbContext context)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return Results.Created($"/api/users/{user.Id}", user);
    }

    private static async Task<IResult> UpdateUser(int id, User updatedUser, AspireDbContext context)
    {
        var user = await context.Users.FindAsync(id);
        if (user is null) return Results.NotFound();

        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;
        user.Role = updatedUser.Role;
        user.OrganizationId = updatedUser.OrganizationId;

        await context.SaveChangesAsync();
        return Results.Ok(user);
    }

    private static async Task<IResult> DeleteUser(int id, AspireDbContext context)
    {
        var user = await context.Users.FindAsync(id);
        if (user is null) return Results.NotFound();

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }
}

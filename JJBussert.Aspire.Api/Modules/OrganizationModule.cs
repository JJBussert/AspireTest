using Carter;
using JJBussert.Aspire.Data;
using JJBussert.Aspire.Domain;
using Microsoft.EntityFrameworkCore;

namespace JJBussert.Aspire.Api.Modules;

public class OrganizationModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organizations").WithTags("Organizations");

        group.MapGet("/", GetOrganizations).RequireAuthorization("AuthenticatedUser");
        group.MapGet("/{id:int}", GetOrganization).RequireAuthorization("AuthenticatedUser");
        group.MapPost("/", CreateOrganization).RequireAuthorization("AdminOnly");
        group.MapPut("/{id:int}", UpdateOrganization).RequireAuthorization("AdminOnly");
        group.MapDelete("/{id:int}", DeleteOrganization).RequireAuthorization("AdminOnly");
    }

    private static async Task<IResult> GetOrganizations(AspireDbContext context)
    {
        var organizations = await context.Organizations
            .Include(o => o.Users)
            .ToListAsync();
        return Results.Ok(organizations);
    }

    private static async Task<IResult> GetOrganization(int id, AspireDbContext context)
    {
        var organization = await context.Organizations
            .Include(o => o.Users)
            .FirstOrDefaultAsync(o => o.Id == id);
        
        return organization is not null ? Results.Ok(organization) : Results.NotFound();
    }

    private static async Task<IResult> CreateOrganization(Organization organization, AspireDbContext context)
    {
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();
        return Results.Created($"/api/organizations/{organization.Id}", organization);
    }

    private static async Task<IResult> UpdateOrganization(int id, Organization updatedOrganization, AspireDbContext context)
    {
        var organization = await context.Organizations.FindAsync(id);
        if (organization is null) return Results.NotFound();

        organization.Name = updatedOrganization.Name;
        organization.Description = updatedOrganization.Description;

        await context.SaveChangesAsync();
        return Results.Ok(organization);
    }

    private static async Task<IResult> DeleteOrganization(int id, AspireDbContext context)
    {
        var organization = await context.Organizations.FindAsync(id);
        if (organization is null) return Results.NotFound();

        context.Organizations.Remove(organization);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }
}

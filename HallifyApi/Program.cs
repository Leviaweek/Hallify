using System.Text.Json.Serialization;
using HallifyApi.Queries;
using HallifyDatabase;
using HallifyDatabase.Models;
using HallifyDatabase.Models.Halls;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        corsPolicyBuilder =>
        {
            corsPolicyBuilder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddDbContextFactory<HallDbContext>(h =>
{
    var connectionString = builder.Configuration.GetConnectionString(HallDbContext.OptionName);
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    var dataSource = dataSourceBuilder.Build();
    h.UseNpgsql(dataSource);
});

builder.Services.AddScoped<HallDb>();

var app = builder.Build();

//app.UseHttpsRedirection();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/api/halls", async Task<Ok<HallDto[]>>
    ([FromServices]HallDb db, CancellationToken cancellationToken) =>
{
    var halls = await db.GetAllHalls(cancellationToken);
    return TypedResults.Ok(halls);
});

app.MapGet("/api/halls/{id:guid}", async Task<Results<Ok<HallDto>, NotFound>>
    ([FromRoute]Guid id, [FromServices]HallDb db, CancellationToken cancellationToken) =>
{
    var hall = await db.GetHallAsync(id, cancellationToken);

    if (hall is null) return TypedResults.NotFound();
    
    return TypedResults.Ok(hall);
});

app.MapPost("/api/halls", async Task<Results<Created<Guid>, BadRequest>> 
    ([FromBody]HallDto request, [FromServices]HallDb db, CancellationToken cancellationToken) =>
{
    var id = await db.AddHallAsync(request, cancellationToken);
    
    if (id is null) return TypedResults.BadRequest();

    return TypedResults.Created($"/api/halls/{id.Value}", id.Value);
});

app.MapDelete("/api/halls/{id:guid}", async Task<Results<Ok, BadRequest>>
    ([FromRoute]Guid id, [FromServices]HallDb db, CancellationToken cancellationToken) =>
{
    var result = await db.DeleteHallAsync(id, cancellationToken);

    if (!result) return TypedResults.BadRequest();

    return TypedResults.Ok();
});

app.MapGet("/api/halls/available", async Task<Ok<HallDto[]>>
    ([AsParameters] HallSearchQuery query, [FromServices] HallDb db, CancellationToken cancellationToken) =>
{
    var halls = await db.GetAvailableHalls(query.StartAt, query.EndAt, query.Capacity, cancellationToken);

    return TypedResults.Ok(halls);
});

app.MapPut("/api/halls/{id:guid}",
    async Task<Results<Ok, BadRequest>> ([FromRoute] Guid id,
        [FromBody] UpdateHallDto request,
        [FromServices] HallDb db,
        CancellationToken cancellationToken) =>
    {
        var result = await db.UpdateHall(id, request,
            cancellationToken);

        if (!result)
            return TypedResults.BadRequest();

        return TypedResults.Ok();
    });

app.MapPost("/api/halls/{id:guid}/bookings", async Task<Results<Ok<decimal>, BadRequest>>
    ([FromRoute]Guid id, [FromBody] BookingDto request, [FromServices] HallDb db, CancellationToken cancellationToken) =>
{
    var book = await db.BookHall(id, request, cancellationToken);

    if (book is null) return TypedResults.BadRequest();

    return TypedResults.Ok(book.Value);
});

await app.RunAsync();

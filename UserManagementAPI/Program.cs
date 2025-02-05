using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using UserManagementAPI.Middleware;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddLogging();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Configure middleware pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

var usersApi = app.MapGroup("/api/users");

usersApi.MapPost("/", Results<Created<User>, BadRequest<string>> (User user, IUserService userService) =>
{
    if (!Validator.TryValidateObject(user, new ValidationContext(user), null, true))
        return TypedResults.BadRequest("Invalid user data.");

    userService.AddUser(user);
    return TypedResults.Created($"/api/users/{user.Id}", user);
}).WithOpenApi(operation =>
{
    operation.Summary = "Create user";
    operation.Description = "Creates a new user.";
    return operation;
});

usersApi.MapGet("/", (int pageNumber, int pageSize, IUserService userService) =>
{
    var users = userService.GetAllUsers(pageNumber, pageSize);
    return TypedResults.Ok(users);
}).WithOpenApi(operation =>
{
    operation.Parameters[0].Description = "The page number to retrieve.";
    operation.Parameters[1].Description = "The number of users per page.";
    operation.Summary = "Get all users";
    operation.Description = "Retrieves a list of all users.";
    return operation;
});

usersApi.MapGet("/{id:int:min(0)}", Results<Ok<User>, NotFound> (int id, IUserService userService) =>
{
    var user = userService.GetUserById(id);
    return user is not null ? TypedResults.Ok(user) : TypedResults.NotFound();
}).WithOpenApi(operation =>
{
    operation.Parameters[0].Description = "The unique identifier of the user to retrieve.";
    operation.Summary = "Get user by ID";
    operation.Description = "Retrieves a user by their unique identifier.";
    return operation;
});

usersApi.MapPut("/{id:int:min(0)}", Results<NoContent, BadRequest<string>, NotFound> (
    int id, User user, IUserService userService) =>
{
    if (!Validator.TryValidateObject(user, new ValidationContext(user), null, true))
        return TypedResults.BadRequest("Invalid user data.");

    var existingUser = userService.GetUserById(id);
    if (existingUser is null)
        return TypedResults.NotFound();

    userService.UpdateUser(user);
    return TypedResults.NoContent();
}).WithOpenApi(operation =>
{
    operation.Parameters[0].Description = "The unique identifier of the user to update.";
    operation.Summary = "Update user by ID";
    operation.Description = "Updates a user by their unique identifier.";
    return operation;
});

usersApi.MapDelete("/{id:int:min(0)}", Results<NoContent, NotFound> (int id, IUserService userService) =>
{
    var user = userService.GetUserById(id);
    if (user is null)
        return TypedResults.NotFound();

    userService.DeleteUser(id);
    return TypedResults.NoContent();
}).WithOpenApi(operation =>
{
    operation.Parameters[0].Description = "The unique identifier of the user to delete.";
    operation.Summary = "Delete user by ID";
    operation.Description = "Deletes a user by their unique identifier.";

    return operation;
});

app.Run();
using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

public class UserService : IUserService
{
    private readonly List<User> _users =
    [
        new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" },
        new() { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com" },
    ];

    private int _nextId;

    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _nextId = _users.Max(u => u.Id);
        _logger = logger;
    }

    public IEnumerable<User> GetAllUsers(int pageNumber, int pageSize) =>
        _users.Skip((pageNumber - 1) * pageSize).Take(pageSize);

    public User? GetUserById(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user is null)
            _logger.LogWarning($"{nameof(User)} with ID {id} not found.");

        return user;
    }

    public void AddUser(User user)
    {
        user.Id = ++_nextId;
        _users.Add(user);
    }

    public void UpdateUser(User user)
    {
        var existingUser = GetUserById(user.Id);
        if (existingUser is null)
        {
            _logger.LogWarning($"{nameof(User)} with ID {user.Id} not found.");
            return;
        }

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
    }

    public void DeleteUser(int id)
    {
        var user = GetUserById(id);
        if (user is not null)
            _users.Remove(user);
        else
            _logger.LogWarning($"{nameof(User)} with ID {id} not found.");
    }
}
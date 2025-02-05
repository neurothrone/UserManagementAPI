using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

public interface IUserService
{
    IEnumerable<User> GetAllUsers(int pageNumber, int pageSize);
    User? GetUserById(int id);
    void AddUser(User user);
    void UpdateUser(User user);
    void DeleteUser(int id);
}
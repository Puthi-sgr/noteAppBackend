using NoteApp.Domain;

namespace NoteApp.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<int> CreateAsync(User user);
    }
}

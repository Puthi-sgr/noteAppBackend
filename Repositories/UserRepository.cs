using System.Data;
using Dapper;
using NoteApp.Domain;

namespace NoteApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _db;
        public UserRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            await EnsureOpenAsync();
            const string sql = @"SELECT Id, Email, PasswordHash, CreatedAt FROM dbo.Users WHERE Email = @Email";
            return await _db.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<int> CreateAsync(User user)
        {
            await EnsureOpenAsync();
            const string sql = @"INSERT INTO dbo.Users (Email, PasswordHash)
                                 OUTPUT INSERTED.Id
                                 VALUES (@Email, @PasswordHash)";
            var id = await _db.ExecuteScalarAsync<int>(sql, new { user.Email, user.PasswordHash });
            return id;
        }

        private async Task EnsureOpenAsync()
        {
            if (_db.State != ConnectionState.Open)
            {
                if (_db is Microsoft.Data.SqlClient.SqlConnection sc)
                {
                    await sc.OpenAsync();
                }
                else
                {
                    _db.Open();
                }
            }
        }
    }
}

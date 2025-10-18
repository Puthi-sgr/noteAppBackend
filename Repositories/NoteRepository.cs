using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using NoteApp.Domain;

namespace NoteApp.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly IDbConnection _db;
        public NoteRepository(IDbConnection db)
        {
            _db = db;
        }

        // Dapper async methods
        public async Task<Note?> GetByIdAsync(int id) //A task that returns type note 
        {
            await EnsureOpenAsync();
            const string sql = @"SELECT Id, UserId, Title, Content, CreatedAt, UpdatedAt FROM dbo.Notes WHERE Id = @Id";
            return await _db.QuerySingleOrDefaultAsync<Note>(sql, new { Id = id }); //Maps the result to Note in ma domain
        }

        public async Task<int> CreateAsync(Note note)
        {
            await EnsureOpenAsync();
            const string sql = @"INSERT INTO dbo.Notes (UserId, Title, Content)
                                 OUTPUT INSERTED.Id
                                 VALUES (@UserId, @Title, @Content)";
            var id = await _db.ExecuteScalarAsync<int>(sql, new { note.UserId, note.Title, note.Content });
            return id;
        }

        public async Task<IReadOnlyList<Note>> GetAsync(int userId, int skip = 0, int take = 50, string sort = "createdAt", bool desc = true)
        {
            await EnsureOpenAsync();
            // Whitelist sort columns to avoid SQL injection
            var sortColumn = sort?.ToLowerInvariant() switch
            {
                "title" => "Title",
                "updatedat" => "UpdatedAt",
                _ => "CreatedAt"
            };
            var order = desc ? "DESC" : "ASC";
            var sql = $@"SELECT Id, UserId, Title, Content, CreatedAt, UpdatedAt
                        FROM dbo.Notes WHERE UserId = @UserId
                        ORDER BY {sortColumn} {order}
                        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
            var rows = await _db.QueryAsync<Note>(sql, new { UserId = userId, Skip = skip, Take = take });
            return rows.AsList();
        }  

        public async Task<bool> UpdateAsync(Note note)
        {
            await EnsureOpenAsync();
            const string sql = @"UPDATE dbo.Notes SET Title = @Title, Content = @Content WHERE Id = @Id";
            var rows = await _db.ExecuteAsync(sql, new { note.Title, note.Content, note.Id });
            return rows > 0;
        }

        public Note? GetById(int id)
        {
            EnsureOpen();
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"SELECT Id, UserId, Title, Content, CreatedAt, UpdatedAt FROM dbo.Notes WHERE Id = @Id";
            var p = cmd.CreateParameter();
            p.ParameterName = "@Id";
            p.Value = id;
            cmd.Parameters.Add(p);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return Map(reader);
        }

        public IEnumerable<Note> GetByUser(int userId, int? top = null, int? skip = null)
        {
            EnsureOpen();
            // Basic pagination using OFFSET/FETCH
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"SELECT Id, UserId, Title, Content, CreatedAt, UpdatedAt
                                 FROM dbo.Notes WHERE UserId = @UserId
                                 ORDER BY CreatedAt DESC
                                 OFFSET @Skip ROWS FETCH NEXT @Top ROWS ONLY";

            var pUser = cmd.CreateParameter();
            pUser.ParameterName = "@UserId";
            pUser.Value = userId;
            cmd.Parameters.Add(pUser);

            var pSkip = cmd.CreateParameter();
            pSkip.ParameterName = "@Skip";
            pSkip.Value = skip ?? 0;
            cmd.Parameters.Add(pSkip);

            var pTop = cmd.CreateParameter();
            pTop.ParameterName = "@Top";
            pTop.Value = top ?? 50;
            cmd.Parameters.Add(pTop);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                yield return Map(reader);
            }
        }

        public int Create(Note note)
        {
            EnsureOpen();
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"INSERT INTO dbo.Notes (UserId, Title, Content)
                                OUTPUT INSERTED.Id
                                VALUES (@UserId, @Title, @Content)";
            AddParam(cmd, "@UserId", note.UserId);
            AddParam(cmd, "@Title", note.Title);
            AddParam(cmd, "@Content", (object?)note.Content ?? DBNull.Value);
            var id = (int)(cmd.ExecuteScalar()!);
            return id;
        }

        public bool Update(Note note)
        {
            EnsureOpen();
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"UPDATE dbo.Notes SET Title=@Title, Content=@Content WHERE Id=@Id";
            AddParam(cmd, "@Id", note.Id);
            AddParam(cmd, "@Title", note.Title);
            AddParam(cmd, "@Content", (object?)note.Content ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            EnsureOpen();
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"DELETE FROM dbo.Notes WHERE Id=@Id";
            AddParam(cmd, "@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void AddParam(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }

        private static Note Map(IDataRecord r) => new()
        {
            Id = r.GetInt32(r.GetOrdinal("Id")),
            UserId = r.GetInt32(r.GetOrdinal("UserId")),
            Title = r.GetString(r.GetOrdinal("Title")),
            Content = r.IsDBNull(r.GetOrdinal("Content")) ? null : r.GetString(r.GetOrdinal("Content")),
            CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
            UpdatedAt = r.IsDBNull(r.GetOrdinal("UpdatedAt")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("UpdatedAt"))
        };

        private void EnsureOpen()
        {
            if (_db.State != ConnectionState.Open)
            {
                _db.Open();
            }
        }

        private async Task EnsureOpenAsync()
        {
            if (_db.State != ConnectionState.Open)
            {
                if (_db is SqlConnection sc)
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

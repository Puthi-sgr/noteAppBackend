using NoteApp.Domain;

namespace NoteApp.Repositories
{
    public interface INoteRepository
    {
        // Existing sync methods (kept for compatibility)
        Note? GetById(int id);
        IEnumerable<Note> GetByUser(int userId, int? top = null, int? skip = null);
        int Create(Note note);
        bool Update(Note note);
        bool Delete(int id);

        // Dapper async methods
        Task<Note?> GetByIdAsync(int id);
        Task<int> CreateAsync(Note note);
        Task<IReadOnlyList<Note>> GetAsync(int userId, int skip = 0, int take = 50, string sort = "createdAt", bool desc = true);
        Task<bool> UpdateAsync(Note note);
        Task<bool> DeleteAsync(int id, int userId);
    }
}

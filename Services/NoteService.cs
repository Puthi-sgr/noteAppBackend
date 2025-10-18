using NoteApp.Domain;
using NoteApp.Dtos;
using NoteApp.Repositories;

namespace NoteApp.Services
{
    public class NoteService
    {
        private readonly INoteRepository _repo;
        public NoteService(INoteRepository repo)
        {
            _repo = repo;
        }

        // List notes for a specific user with paging/sorting
        public async Task<IReadOnlyList<NoteReadDto>> ListAsync(int userId, int skip = 0, int take = 50, string sort = "createdAt", bool desc = true)
        {
            var notes = await _repo.GetAsync(userId, skip, take, sort, desc);
            return notes.Select(Map).ToList();
        }

        // Create a note for a specific user; title is required and trimmed
        public async Task<NoteReadDto> CreateAsync(int userId, NoteCreateDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var title = (dto.Title ?? string.Empty).Trim();
            if (title.Length == 0) throw new ArgumentException("Title is required.", nameof(dto.Title));

            var note = new Note
            {
                UserId = userId,
                Title = title,
                Content = dto.Content
            };

            var id = await _repo.CreateAsync(note);
            var created = await _repo.GetByIdAsync(id);
            if (created is null || created.UserId != userId)
            {
                throw new InvalidOperationException("Failed to create note or ownership mismatch.");
            }
            return Map(created);
        }

        // Get a single note by id for a specific user (ownership enforced)
        public async Task<NoteReadDto?> GetAsync(int userId, int id)
        {
            var note = await _repo.GetByIdAsync(id);
            if (note is null || note.UserId != userId)
            {
                return null; // Not found or not owned by this user
            }
            return Map(note);
        }

        // Update a note for a specific user
        public async Task<NoteReadDto?> UpdateAsync(int userId, int id, NoteUpdateDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var existing = await _repo.GetByIdAsync(id);
            if (existing is null || existing.UserId != userId)
            {
                return null; // Not found or not owned by this user
            }

            var title = (dto.Title ?? string.Empty).Trim();
            if (title.Length == 0) throw new ArgumentException("Title is required.", nameof(dto.Title));

            existing.Title = title;
            existing.Content = dto.Content;

            var ok = await _repo.UpdateAsync(existing);
            if (!ok) return null;

            var updated = await _repo.GetByIdAsync(id);
            return updated is null ? null : Map(updated);
        }

        private static NoteReadDto Map(Note n) => new()
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            Content = n.Content,
            CreatedAt = n.CreatedAt,
            UpdatedAt = n.UpdatedAt
        };
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteApp.Data;
using NoteApp.Domain;
using NoteApp.Dtos;
using System.Threading.Tasks;

namespace NoteApp.Controllers
{
    [Route("api/EFnote")]
    [ApiController]
    public class EFnoteController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EFnoteController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<List<Note>> Get(CancellationToken ct)
        {
            return await _db.Notes
                .AsNoTracking()
                .OrderByDescending(n => n.Id).ToListAsync(ct);
        }

        [HttpPost]
        public async Task<ActionResult<Note>> Create(NoteCreateDto req, CancellationToken ct)
        {
            var note = new Note { Title = req.Title, Content = req.Content };
            _db.Notes.Add(note);

            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Note>> GetById(int id, CancellationToken ct)
        {
            var note = await _db.Notes.FindAsync(new object?[] { id }, ct);
            return note is null ? NotFound() : Ok(note);
        }

        [HttpGet("{id:int}/with-comments")]
        public async Task<ActionResult<NoteReadDto>> GetWithComments(int id, CancellationToken ct)
        {
            var note = await _db.Notes
                 .AsNoTracking()
                 .Include(n => n.Comments)
                 .FirstOrDefaultAsync(n => n.Id == id, ct);

            if (note is null)
                return NotFound();

            // Map entity to DTO
            var noteDto = new NoteReadDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                Comments = note.Comments.Select(c => new CommentReadDto
                {
                    Id = c.Id,
                    NoteId = c.NoteId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                }).ToList()
            };

            return Ok(noteDto);
        }

        [HttpPost("{id:int}/comment")]
        public async Task<ActionResult> AddComment(int id, CommentCreateDto req, CancellationToken ct)
        {
            var tx = await _db.Database.BeginTransactionAsync(ct);

            var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id, ct);

            if (note is null)
            {
                return NotFound();
            }

            var result = _db.Comments.Add(new Comment
            {
                NoteId = note.Id,
                UserId = 0, // Placeholder for UserId
                Content = req.Text,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);

            // Return DTO instead of entity
            var commentDto = new CommentReadDto
            {
                Id = result.Entity.Id,
                NoteId = result.Entity.NoteId,
                Content = result.Entity.Content,
                CreatedAt = result.Entity.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = note.Id }, commentDto);
        }
    }
}

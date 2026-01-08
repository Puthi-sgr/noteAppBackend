using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteApp.Data;
using NoteApp.Domain;
using NoteApp.Dtos;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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

        // GET: api/<ValuesController>
        [HttpGet]
        public async Task<List<Note>> Get(CancellationToken ct)
        {
            return await _db.Notes.AsNoTracking().OrderByDescending(n => n.Id).ToListAsync(ct);
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
            var note = await _db.Notes.FindAsync(new object?[] { id }, ct); //We use dbcontext note entity to generate sql and find the note by id
            return note is null ? NotFound() : Ok(note);
        }

        [HttpGet("{id:int}/with-comments")]
        public async Task<ActionResult<NoteReadDto>> GetWithComments(int id, CancellationToken ct)
        {
            var note = await _db.Notes
                 .AsNoTracking()
                 .Include(n => n.Comments)
                 .FirstOrDefaultAsync(n => n.Id == id, ct);

            return note is null ? NotFound() : Ok(note);
        }
    }
}

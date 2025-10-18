using Microsoft.AspNetCore.Mvc;
using NoteApp.Dtos;
using NoteApp.Services;

namespace NoteApp.Controllers
{
    [ApiController]
    [Route("notes")]
    public class NotesController : ControllerBase
    {
        private readonly NoteService _service;
        public NotesController(NoteService service)
        {
            _service = service;
        }

        // GET /notes?skip=0&take=50&sort=createdAt&desc=true
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<NoteReadDto>>> List([FromQuery] int skip = 0, [FromQuery] int take = 50, [FromQuery] string sort = "createdAt", [FromQuery] bool desc = true)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized(); //returns bool and out param userId
            var items = await _service.ListAsync(userId, skip, take, sort, desc);
            return Ok(items);
        }

        // POST /notes
        [HttpPost]
        public async Task<ActionResult<NoteReadDto>> Create([FromBody] NoteCreateDto dto)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            var created = await _service.CreateAsync(userId, dto); //This dto is actually fromt the body :>
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // GET /notes/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<NoteReadDto>> GetById(int id)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            var note = await _service.GetAsync(userId, id);
            if (note is null) return NotFound();
            return Ok(note);
        }

        // PUT /notes/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult<NoteReadDto>> Update(int id, [FromBody] NoteUpdateDto dto)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            var updated = await _service.UpdateAsync(userId, id, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // DELETE /notes/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            var deleted = await _service.DeleteAsync(userId, id);
            if (!deleted) return NotFound();
            return Ok(new { message = $"Note has been deleted deleted" });
        }

        // Reads demo user id from header until auth is implemented
        private bool TryGetUserId(out int userId)
        {
            userId = default;
            if (!Request.Headers.TryGetValue("X-Demo-UserId", out var values)) //If true assign the x-header value to values    
                return false;
            return int.TryParse(values.FirstOrDefault(), out userId); //return true if parse is successful
            //
        }
    }
}

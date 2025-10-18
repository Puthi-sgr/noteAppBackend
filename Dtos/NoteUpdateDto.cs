using System.ComponentModel.DataAnnotations;

namespace NoteApp.Dtos
{
    public class NoteUpdateDto
    {
        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }
    }
}

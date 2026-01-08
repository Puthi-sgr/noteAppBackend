using System.ComponentModel.DataAnnotations;

namespace NoteApp.Dtos
{
    public class CommentCreateDto
    {
        [Required, StringLength(1000)]
        public string Text { get; set; } = string.Empty;
    }
}

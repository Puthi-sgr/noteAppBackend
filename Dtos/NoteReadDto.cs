namespace NoteApp.Dtos
{
    public class NoteReadDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public List<CommentReadDto> Comments { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

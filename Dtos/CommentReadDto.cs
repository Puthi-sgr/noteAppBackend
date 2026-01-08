namespace NoteApp.Dtos
{
    public class CommentReadDto
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

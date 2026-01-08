namespace NoteApp.Domain
{
    public class Comment
    {
        public int Id { get; set; }
        public int NoteId { get; set; } //The FK value
        public int UserId { get; set; }
        public Note Note { get; set; } = null!; //Its for navigation. You can load Note as a whole object from c.Note
        public string Content { get; set; } = string.Empty;
        public DateTime? lastCommentUTC { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

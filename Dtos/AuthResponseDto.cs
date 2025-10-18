namespace NoteApp.Dtos
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int ExpiresInSeconds { get; set; }
    }
}

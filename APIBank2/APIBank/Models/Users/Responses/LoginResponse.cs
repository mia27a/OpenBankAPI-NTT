using System.Text.Json.Serialization;

namespace APIBank.Models.Users.Responses
{
    public class LoginResponse
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public DateTime PasswordChangedAt { get; set; }

        public string Token { get; set; }

        //TODO
        //[Required]
        //public DateTime TokenExpiresAt { get; set; }

        //TODO
        //public string SessionId { get; set; }

        [JsonIgnore] // refresh token is returned in http only cookie
        public string RefreshToken { get; set; }

        public LoginResponse(User user, string token, string refreshToken)
        {
            Id = user.Id;
            CreatedAt = user.CreatedAt;
            Email = user.Email;
            FullName = user.FullName;
            Username = user.Username;
            PasswordChangedAt = user.PasswordChangedAt;
            Token = token;
            RefreshToken = refreshToken;
        }
    }
}
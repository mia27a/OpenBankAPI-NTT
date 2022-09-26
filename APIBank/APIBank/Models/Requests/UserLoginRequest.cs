namespace APIBank.Models.Requests
{
    public class UserLoginRequest
    {
        [Required]
        [MinLength(8)]
        public string Username { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }
    }
}
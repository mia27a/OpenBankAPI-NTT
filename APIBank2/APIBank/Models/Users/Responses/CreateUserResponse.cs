namespace APIBank.Models.Users.Responses
{
    public class CreateUserResponse
    {
        //TODO
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
    }
}

namespace APIBank.Models.Requests
{
    public class UserCreateRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(16)]
        public string FullName { get; set; }

        [Required]
        [MinLength(8)]
        public string Username { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }
        /*[Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }*/
    }
}
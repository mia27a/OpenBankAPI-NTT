namespace APIBank.Models.MyRequestResponses
{
    public class UserUpdateRequest
    {
        [EmailAddress]
        public string? Email { get; set; }
        [MinLength(16)]
        public string? FullName { get; set; }
        [MinLength(8)]
        public string? Username { get; set; }
        [MinLength(8)]
        public string? Password { get; set; }
        /*[Compare("Password")]
        public string ConfirmPassword { get; set; }*/
        public DateTime? PasswordChangedAt { get; set; }
    }
}
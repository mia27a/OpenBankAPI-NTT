namespace APIBank.Models.Users.Responses
{
    public class LoginResponse
    {
        
        public string Token { get; set; }

        //TODO
        //[Required]
        //public DateTime TokenExpiresAt { get; set; }

        //TODO
        //public string SessionId { get; set; }

        public CreateUserResponse CreateUserResponse { get; set; }
    }
}
namespace APIBank.Models.Responses
{
    public class AccountMovims
    {
        public AccountRequestResponse Account { get; set; }
        public Movim[] Movims { get; set; }
    }
}
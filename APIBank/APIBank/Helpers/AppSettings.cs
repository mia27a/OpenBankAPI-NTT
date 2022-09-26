namespace APIBank.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public int RefreshTokenRemover { get; set; } //Remover Refresh Token da database passado X n�mero de dias
        public int Access_Token_Expires_At { get; set; }
        public int Refresh_Token_Expires_At { get; set; }
    }
}
namespace APIBank.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }

        //Remover Refresh Token da database passado X n�mero de dias
        public int RefreshTokenRemover { get; set; }
    }
}
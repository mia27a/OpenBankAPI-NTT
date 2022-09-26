namespace APIBank.Authorization
{
    public interface IJwtUtils
    {
        public string GenerateToken(User user, RefreshToken refToken);//AQUI!
        public int? ValidateJWTToken(string token);
        public RefreshToken GenerateRefreshToken(string ipAddress);
    }
}


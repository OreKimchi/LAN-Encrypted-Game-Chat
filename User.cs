namespace SecureChat4InARow
{
    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }       // SHA-256 hashed
        public string Salt { get; set; }               // Will be random per user
                                                       // public string PublicKey { get; set; }       // Optional: for RSA later
    }

}

namespace WebVersion.AdditionalClasses
{
    public class SignInResponse
    {
        public string idToken { get; set; }
        public string email { get; set; }
        public string displayName { get; set; }
        public string localId { get; set; }
        public bool registered { get; set; }
        public string profilePicture { get; set; }
        public string refreshToken { get; set; }
        public int expiresIn { get; set; }
    }
}

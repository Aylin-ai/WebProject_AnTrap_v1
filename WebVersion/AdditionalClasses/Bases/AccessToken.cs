using Newtonsoft.Json;

namespace WebVersion.AdditionalClasses.Bases
{
    public class AccessToken
    {
        public string Access_Token { get; set; }
        public string TokenType { get; set; } = "Bearer";

        public string ExpiresIn { get; set; }

        public string RefreshToken { get; set; }

        public string Scope { get; set; }

        public string CreatedAt { get; set; }
    }
}
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System.Data.Common;
using System.Net.Http;
using WebVersion.AdditionalClasses;
using WebVersion.AdditionalClasses.Bases;

namespace WebVersion.Models
{
    public static class APIToken
    {
        private static string connectionString = "Server=DESKTOP-H0P5N48;Database=AnTrap;Trusted_Connection=True;";
        private const char V = '"';
        private static IHttpClientFactory _httpClientFactory;
        public static readonly string clientID = "Qi3gXQQHHX35yM5lz2S8DZojHS9QWiP2zxaKR9NHanA";
        public static readonly string clientSecret = "dscisAHfUgUYwduO_BO59LARB0FY-fuy6I9TwBIldPE";
        static List<AccessToken> accessTokens = new List<AccessToken>();

        public static async Task<AccessToken> GetAccessToken(string authCode)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://shikimori.one");

            HttpMessageHandler handler = new HttpClientHandler();

            using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://shikimori.one/oauth/token"))
            {
                request.Headers.TryAddWithoutValidation("User-Agent", "ShikiOAuthTest");

                var multipartContent = new MultipartFormDataContent
                {
                        { new StringContent("authorization_code"), "grant_type" },
                        { new StringContent(clientID), "client_id" },
                        { new StringContent(clientSecret), "client_secret" },
                        { new StringContent(authCode), "code" },
                        { new StringContent("urn:ietf:wg:oauth:2.0:oob"), "redirect_uri" }
                    };
                request.Content = multipartContent;

                var response = await httpClient.SendAsync(request);
                var access = await response.Content.ReadAsStringAsync();
                AccessToken accessToken = FromStringToAccessToken(access);
                return accessToken;
            }
        }

        public static async Task<AccessToken> RefreshAccessToken(string refreshtoken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://shikimori.one/oauth/token"))
            {
                request.Headers.TryAddWithoutValidation("User-Agent", "ShikiOAuthTest");

                var multipartContent = new MultipartFormDataContent();
                multipartContent.Add(new StringContent("refresh_token"), "grant_type");
                multipartContent.Add(new StringContent(clientID), "client_id");
                multipartContent.Add(new StringContent(clientSecret), "client_secret");
                multipartContent.Add(new StringContent(refreshtoken), "refresh_token");
                request.Content = multipartContent;

                var response = await httpClient.SendAsync(request);
                var access = await response.Content.ReadAsStringAsync();
                AccessToken newAccessToken = FromStringToAccessToken(access);
                return newAccessToken;
            }
        }

        public static AccessToken FromStringToAccessToken(string responceContent)
        {
            try
            {
                string[] strings = responceContent.Split('"', ',', ':', '{', '}');
                strings.ToList().Remove(",");
                strings.ToList().Remove(":");
                strings.ToList().Remove("{");
                strings.ToList().Remove("}");
                strings.ToList().Remove(V.ToString());
                AccessToken accessToken = new AccessToken()
                {
                    Access_Token = strings[5],
                    CreatedAt = strings[32],
                    ExpiresIn = strings[16],
                    RefreshToken = strings[21],
                    Scope = strings[27],
                    TokenType = strings[11]
                };
                return accessToken;
            }
            catch
            {
                return new AccessToken() { };
            }
        }

        public static async Task<string> GetLastRefreshToken(MySqlConnection conn)
        {
            string sql = "Select * from access_tokens where id_access_tokens = (select max(id_access_tokens) from access_tokens)";
            string token = "null";
            string refresh = "null";
            int token_id;

            // Создать объект Command.
            MySqlCommand cmd = new MySqlCommand();

            // Сочетать Command с Connection.
            cmd.Connection = conn;
            cmd.CommandText = sql;


            using (DbDataReader reader = await cmd.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    if (reader.Read())
                    {
                        token_id = reader.GetInt32(0);
                        token = reader.GetString(1);
                        refresh = reader.GetString(2);
                    }
                }
            }
            return refresh;
        }

        public static async void AddAccessTokenToDB(MySqlConnection mySqlConnection, AccessToken accessToken)
        {
            string sql = $"Insert into access_tokens (access_tokens, refresh_tokens) values (@access_tokens, @refresh_tokens)";
            MySqlCommand cmd = new MySqlCommand();

            // Сочетать Command с Connection.
            cmd.Connection = mySqlConnection;
            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@access_tokens", accessToken.Access_Token);
            cmd.Parameters.AddWithValue("@refresh_tokens", accessToken.RefreshToken);

            int rows = await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<string> UpdateAccessTokenAndAddLastToDB()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();
            try
            {
                string refresh_token = await GetLastRefreshToken(conn);
                AccessToken accessToken = await RefreshAccessToken(refresh_token);
                AddAccessTokenToDB(conn, accessToken);
                return await GetLastRefreshToken(conn);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}

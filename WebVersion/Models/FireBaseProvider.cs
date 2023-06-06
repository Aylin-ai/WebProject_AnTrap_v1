using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace WebVersion.Models
{
    public class FirebaseAppProvider
    {
        private static FirebaseApp _app;
        public static string FIREBASE_ID_TOKEN;

        public static FirebaseApp GetFirebaseApp()
        {
            if (_app == null)
            {
                _app = FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("C:\\Users\\ajlin\\Desktop\\antrap-firebase-firebase-adminsdk-oc7cx-6e72638be5.json")
                });
            }

            return _app;
        }
    }

}

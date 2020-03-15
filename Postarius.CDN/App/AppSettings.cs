using System.Collections.Generic;

namespace Web.StaticFiles.App
{
    public class AppSettings
    {
        public string SigninSecretKey { get; set; }
        public IEnumerable<string> FrontendUrls { get; set; }
        public IEnumerable<string> IssuerNames { get; set; }
    }
}
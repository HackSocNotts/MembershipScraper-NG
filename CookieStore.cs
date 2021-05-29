using System;
using System.IO;

namespace MembershipScraperNG
{
    public interface ICookieStore
    {
        string Cookie { get; set; }
    }

    public class CookieStoreFile : ICookieStore
    {
        public CookieStoreFile(string path = "cookie")
        {
            this.path = path;
        }

        private string path;
        public string Cookie { get { return getCookie(); } set { setCookie(value); } }

        private string getCookie()
        {
            return File.ReadAllText(path);
        }

        private void setCookie(string val)
        {
            File.WriteAllText(path, val);
        }
    }
}
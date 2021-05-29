using System;

namespace MembershipScraperNG
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new CookieStoreFile();
            var s = new Scraper(c);
            s.GetMembers();
        }
    }
}

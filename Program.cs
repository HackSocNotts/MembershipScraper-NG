using System;
using System.Threading.Tasks;

namespace MembershipScraperNG
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var c = new CookieStoreFile();
            var s = new Scraper(c);

            while(true){
                Console.WriteLine("Starting Task");
                var x = s.GetMembers();
                await Task.Delay((int)(1000*60*2.5));
                await x;
            }
        }
    }
}

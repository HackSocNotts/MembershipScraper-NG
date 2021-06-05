using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ScrapySharp.Extensions;
using ScrapySharp.Html.Dom;

using MongoDB.Driver;
using MongoDB.Bson;

namespace MembershipScraperNG
{
    public class Scraper
    {
        public ICookieStore cookieStore;

        string heartbeatUrl;
        string mongoUrl;
        string mongoCollection;
        string sumsUrl;

        MongoClient client;
        IMongoDatabase database;
        IMongoCollection<BsonDocument> collection;

        public Scraper(ICookieStore c)
        {
            this.cookieStore = c;

            heartbeatUrl = Environment.GetEnvironmentVariable("heartbeatUrl");
            mongoUrl = Environment.GetEnvironmentVariable("mongoUrl");
            mongoCollection = Environment.GetEnvironmentVariable("mongoCollection");

            sumsUrl = Environment.GetEnvironmentVariable("sumsUrl");

            client = new MongoClient(new MongoUrl(mongoUrl));
            database = client.GetDatabase(mongoCollection);
            collection = database.GetCollection<BsonDocument>("members");
        }

        public async Task GetMembers()
        {
            #region make_request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sumsUrl);

            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Cookie("su_session", cookieStore.Cookie, "/", "student-dashboard.sums.su"));

            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
            StreamReader readStream = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            #endregion

            var members = ScrapeHtml(readStream);
            Console.WriteLine("Scraped {0} Members", members.Count);
            await updateMembers(members);

            #region update_cookie
            string s = response.Headers["Set-Cookie"];
            Console.WriteLine(s);
            if (s != null)
            {
                var val = CookieValue(s, "su_session");
                Console.WriteLine(val);
                cookieStore.Cookie = val.Remove(val.Length - 1, 1); ;
            }
            #endregion

            #region send heartbeat
            HttpWebRequest heartbeat = (HttpWebRequest)WebRequest.Create(heartbeatUrl);
            await heartbeat.GetResponseAsync();
            #endregion

            #region  cleanup
            response.Close();
            readStream.Close();
            #endregion
        }

        protected List<Member> ScrapeHtml(StreamReader s)
        {
            var src = s.ReadToEnd();
            var html = HDocument.Parse(src);

            var rs = html.CssSelect("#group-member-list-datatable > tbody > tr");
            var ds = rs.Select(r => r.Children.Select(vs => vs.InnerText));

            var ms = new List<Member>();

            foreach (var r in ds)
            {
                var x = r.ToList();

                if (x.Count != 11)
                {
                    foreach (var z in x)
                    {
                        Console.WriteLine(z.Trim());
                    }
                    Console.WriteLine();
                    continue;
                }

                var m = new Member(Convert.ToUInt32(x[1].Trim()), x[3].Trim(), x[5].Trim(), x[9].Trim());
                ms.Add(m);
            }

            return ms;
        }
        protected async Task updateMembers(List<Member> ms)
        {

            var xs = new List<Task>();
            foreach (var m in ms)
            {
                xs.Add(AddOrUpdate(m));
            }

            foreach (var x in xs)
            {
                await x;
            }
        }

        public async Task AddOrUpdate(Member m)
        {
            var document = BsonDocument.Parse(JsonSerializer.Serialize(m));
            var filter = Builders<BsonDocument>.Filter.Eq("ID", m.ID);
            var nMatches = await collection.Find(filter).CountDocumentsAsync();

            if (nMatches == 0)
            {
                await collection.InsertOneAsync(document);
            }
        }

        private string CookieValue(string header, string name)
        {
            MatchCollection Ms = Regex.Matches(header, string.Format("{0}=(?<value>.*?);", name));
            return Ms.Select(M => M.ToString().Split('=')[1]).Where(x => x != "deleted;").First();
        }

    }
}
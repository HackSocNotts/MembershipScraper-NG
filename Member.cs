using System;

namespace MembershipScraperNG
{
    public class Member
    {
        public uint ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Joined { get; set; }

        public string DiscordID { get; set; }

        public override string ToString()
        {
            return String.Format("{0}, '{1}', '{2}', '{3}'", ID, Name, Type, Joined);
        }

        public Member(uint id, string name, string type, string joined, string discordID = null)
        {
            ID = id;
            Name = name;
            Type = type;
            Joined = joined;
            DiscordID = DiscordID;
        }
    }
}
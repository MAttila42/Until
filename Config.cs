using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Until
{
    public class Config
    {
        public string Token { get; private set; }
        public ulong OwnerID { get; private set; }
        public List<ulong> EmojiServers { get; private set; }

        public Config(string path)
        {
            using (StreamReader stream = File.OpenText(path))
            {
                XDocument config = XDocument.Load(stream);
                XElement configElement = config.Element("config");

                this.Token = configElement.Element("token").Value;
                this.OwnerID = ulong.Parse(configElement.Element("ownerid").Value);
                this.EmojiServers = new List<ulong>();

                configElement.Element("emojiservers").Elements("server").ToList().ForEach(s => this.EmojiServers.Add(ulong.Parse(s.Value)));
            }
        }
    }
}

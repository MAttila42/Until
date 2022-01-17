using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Until
{
	public class Config
	{
		public string Token;
		public ulong OwnerID;

		public Config() { }
		public Config(string token, ulong ownerId)
		{
			this.Token = token;
			this.OwnerID = ownerId;
		}

		public static Config FromXML(string path)
		{
			Config temp = new Config();
			using (StreamReader stream = File.OpenText(path))
			{
				XDocument config = XDocument.Load(stream);
				temp.Token = config.Element("config").Descendants().ToList()[0].Value;
				temp.OwnerID = ulong.Parse(config.Element("config").Descendants().ToList()[1].Value);
			}
			return temp;
		}
	}
}

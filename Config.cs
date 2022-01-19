using System.IO;
using System.Xml.Linq;

namespace Until
{
	public class Config
	{
		public string Token;
		public ulong OwnerID;

		public Config() { }

		public static Config FromXML(string path)
		{
			Config temp = new Config();
			using (StreamReader stream = File.OpenText(path))
			{
				XDocument config = XDocument.Load(stream);
				var configElement = config.Element("config");
				temp.Token = configElement.Element("token").Value;
				temp.OwnerID = ulong.Parse(configElement.Element("ownerid").Value);
			}
			return temp;
		}
	}
}

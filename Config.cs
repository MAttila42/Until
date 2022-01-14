using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Until
{
	public class Config
	{
		public string Token;

		public Config() { }
		public Config(string token)
		{
			this.Token = token;
		}

		public static Config FromXML(string path)
		{
			Config temp = new Config();
			using (StreamReader stream = File.OpenText(path))
			{
				XDocument config = XDocument.Load(stream);
				temp.Token = config.Element("config").Descendants().First().Value;
			}
			return temp;
		}
	}
}

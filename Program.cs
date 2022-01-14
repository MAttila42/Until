namespace Until
{
	public class Program
	{
		public static void Main()
		{
			Until bot = new Until(Config.FromXML("config.xml"));
			bot.MainAsync().GetAwaiter().GetResult();
		}
	}
}

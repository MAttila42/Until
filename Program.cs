namespace Until
{
    public class Program
    {
        public static void Main()
        {
            Until bot = new Until(new Config("config.xml"));
            bot.MainAsync().GetAwaiter().GetResult();
        }
    }
}

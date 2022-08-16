namespace Until
{
    public class Program
    {
        public static void Main()
            => new Until(new Config("config.xml")).MainAsync().GetAwaiter().GetResult();
    }
}
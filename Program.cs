namespace Until
{
    public class Program
    {
        public static void Main()
            => new Until(new("config.xml")).MainAsync().GetAwaiter().GetResult();
        protected Program() { }
    }
}
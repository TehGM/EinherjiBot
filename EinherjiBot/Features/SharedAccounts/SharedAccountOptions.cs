namespace TehGM.EinherjiBot.SharedAccounts
{
    public class SharedAccountOptions
    {
        public IDictionary<SharedAccountType, string> ImageURLs { get; set; } = new Dictionary<SharedAccountType, string>()
        {
            { SharedAccountType.Netflix, "https://historia.org.pl/wp-content/uploads/2018/04/netflix-logo.jpg" }
        };
    }
}

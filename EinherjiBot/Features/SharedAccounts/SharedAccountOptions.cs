namespace TehGM.EinherjiBot.SharedAccounts
{
    public class SharedAccountOptions
    {
        public IDictionary<SharedAccountType, string> ImageURLs { get; set; } = new Dictionary<SharedAccountType, string>()
        {
            { SharedAccountType.Netflix, "https://historia.org.pl/wp-content/uploads/2018/04/netflix-logo.jpg" },
            { SharedAccountType.NordVPN, "https://nordvpn.com/wp-content/uploads/2020/07/favicon-196x196-1.png" }
        };
    }
}

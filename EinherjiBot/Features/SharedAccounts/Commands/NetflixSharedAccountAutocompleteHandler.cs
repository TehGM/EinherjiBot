namespace TehGM.EinherjiBot.SharedAccounts.Commands
{
    public class NetflixSharedAccountAutocompleteHandler : SharedAccountAutocompleteHandlerBase
    {
        protected override SharedAccountType AccountType => SharedAccountType.Netflix;
        protected override bool ForModeration => false;
    }

    public class NetflixSharedAccountModerationAutocompleteHandler : SharedAccountAutocompleteHandlerBase
    {
        protected override SharedAccountType AccountType => SharedAccountType.Netflix;
        protected override bool ForModeration => true;
    }
}

namespace TehGM.EinherjiBot.PlaceholdersEngine.API
{
    public interface IPlaceholdersService
    {
        Task<PlaceholdersConvertResponse> ConvertAsync(PlaceholdersConvertRequest request, CancellationToken cancellationToken = default);
    }
}

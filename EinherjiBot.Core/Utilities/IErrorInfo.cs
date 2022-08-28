namespace TehGM.EinherjiBot
{
    public interface IErrorInfo
    {
        public DateTime Timestamp { get; }
        public string Message { get; }
    }
}

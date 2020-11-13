using System.Diagnostics;

namespace TehGM.EinherjiBot
{
    static class BotInfoUtility
    {
        public static string GetVersion()
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(typeof(BotInfoUtility).Assembly.Location);
            if (!string.IsNullOrWhiteSpace(versionInfo.ProductVersion))
                return versionInfo.ProductVersion;
            string result = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}";
            if (versionInfo.FilePrivatePart != 0)
                result += $".{versionInfo.FilePrivatePart}";
            return result;
        }
    }
}

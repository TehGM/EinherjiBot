namespace TehGM.EinherjiBot
{
    public static class MentionID
    {
        public static string User(ulong ID, bool useNickname = true)
        {
            if (useNickname)
                return $"<@!{ID}>";
            return $"<@{ID}>";
        }

        public static string Channel(ulong ID)
            => $"<#{ID}>";

        public static string Role(ulong ID)
            => $"<@&{ID}>";
    }
}

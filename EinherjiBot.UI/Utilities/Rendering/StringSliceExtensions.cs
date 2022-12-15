using Markdig.Helpers;

namespace TehGM.EinherjiBot.UI.Utilities.Rendering
{
    public static class StringSliceExtensions
    {
        // for some reason markdig makes this internal
        // but we need it. Bad markdig
        public static int CountAndSkipChar(this StringSlice slice, char matchChar)
        {
            string text = slice.Text;
            int end = slice.End;
            int current = slice.Start;

            while (current <= end && (uint)current < (uint)text.Length && text[current] == matchChar)
            {
                current++;
            }

            int count = current - slice.Start;
            slice.Start = current;
            return count;
        }
    }
}

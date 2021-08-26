using System;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    public class SummaryAttribute : Attribute
    {
        public string Text { get; }

        public SummaryAttribute(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));

            this.Text = text;
        }
    }
}

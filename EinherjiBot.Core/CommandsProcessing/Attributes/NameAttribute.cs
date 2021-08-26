using System;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    public class NameAttribute : Attribute
    {
        public string Text { get; }

        public NameAttribute(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));

            this.Text = text;
        }
    }
}

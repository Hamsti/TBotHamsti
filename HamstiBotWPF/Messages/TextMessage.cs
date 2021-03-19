using System.Windows;

namespace TBotHamsti.Messages
{
    public class TextMessage : IMessage
    {
        public TextMessage(string text)
        {
            Text = text;
        }

        public TextMessage(string text, HorizontalAlignment horizontalAlignment) : this(text)
        {
            HorizontalAlignment = horizontalAlignment;
        }

        public string Text { get; set; }

        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;
    }
}

using System.Windows;

namespace TBotHamsti.Models.Messages
{
    public class TextMessage : IMessage
    {
        public string Text { get; set; }

        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;
        
        public TextMessage(string text)
        {
            Text = text;
        }

        public TextMessage(string text, HorizontalAlignment horizontalAlignment) : this(text)
        {
            HorizontalAlignment = horizontalAlignment;
        }
    }
}

using System;
using System.Windows;

namespace TBotHamsti.Models.Messages
{
    public class TextMessage : IMessage
    {
        public string Text { get; private set; }
        public DateTime DateTimeGetMessage { get; private set; }
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;


        public TextMessage(string text)
        {
            Text = text;
            DateTimeGetMessage = DateTime.Now;
        }

        public TextMessage(string text, HorizontalAlignment horizontalAlignment) : this(text)
        {
            HorizontalAlignment = horizontalAlignment;
        }
    }
}

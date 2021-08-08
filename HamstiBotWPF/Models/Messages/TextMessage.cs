using System;
using System.Windows;

namespace TBotHamsti.Models.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class TextMessage : IMessage
    {
        /// <value>
        /// Log text without date
        /// </value>
        public string Text { get; }
        public DateTime DateTimeGetMessage { get; }
        public HorizontalAlignment HorizontalAlignment { get; }

        /// <summary>
        /// <inheritdoc cref="TextMessage(string, HorizontalAlignment)" path="/summary"/> = <see cref="HorizontalAlignment.Stretch"/>
        /// </summary>
        /// <inheritdoc cref="TextMessage(string, HorizontalAlignment)" path="/param"/>
        public TextMessage(string text) : this(text, HorizontalAlignment.Stretch) { }

        /// <summary>
        /// Creating the new <see cref="IMessage"/> = <paramref name="text"/> type containing the current <see cref="DateTimeGetMessage"/> with <paramref name="horizontalAlignment"/>
        /// </summary>
        /// <param name="text"><inheritdoc cref="Text" path="/value"/></param>
        /// <param name="horizontalAlignment"><inheritdoc cref="HorizontalAlignment" path="/value"/></param>
        public TextMessage(string text, HorizontalAlignment horizontalAlignment)
        {
            Text = text;
            DateTimeGetMessage = DateTime.Now;
            HorizontalAlignment = horizontalAlignment;
        }
    }
}

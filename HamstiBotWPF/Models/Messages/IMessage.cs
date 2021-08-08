using System;
using System.Windows;

namespace TBotHamsti.Models.Messages
{
    /// <summary>
    /// Common <see cref="IMessage"/> interface for logging
    /// </summary>
    public interface IMessage
    {
        /// <value>
        /// <see cref="DateTime"/> when the log <see cref="IMessage"/> was created
        /// </value>
        DateTime DateTimeGetMessage { get; }

        /// <value>
        /// <see cref="HorizontalAlignment"/> of the <see cref="IMessage"/> on the <see cref="System.Windows.Controls.Page"/>
        /// </value>
        HorizontalAlignment HorizontalAlignment { get; }
    }
}

using System;

namespace TBotHamsti.Models
{
    public static class HandlerException
    {
        /// <summary>
        /// The index <see cref="Exception.Data"/> of an additional error message
        /// </summary>
        private const string ADD_EX_INDEX = "additional_exception";

        /// <summary>
        /// Adding an additional error <paramref name="message"/> to ex
        /// </summary>
        /// <param name="ex">A thrown exception</param>
        /// <param name="message">An additional error description (line break is added automatically)</param>
        /// <exception cref="ArgumentException">If <paramref name="message"/> is null</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="message"/> is empty or contain only white space</exception>
        public static void AppendExceptionMessage(this Exception ex, string message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message of additional exception mustn't be empty", nameof(message));
            }

            if (ex.Data.Contains(ADD_EX_INDEX))
            {
                ex.Data[ADD_EX_INDEX] += '\n' + message;
            }
            else
            {
                ex.Data.Add(ADD_EX_INDEX, message);
            }
        }

        /// <summary>
        /// Getting an additional error from <paramref name="ex"/>
        /// </summary>
        /// <param name="ex"><inheritdoc cref="AppendExceptionMessage(Exception, string)" path="/param[1]"/></param>
        /// <returns>A summary description of the whole error message</returns>
        /// <exception cref="ArgumentNullException">If any message of the <paramref name="ex"/> is null</exception>
        public static string GetExceptionMessage(Exception ex)
        {
            string exMessage = "The exception: " + (ex.InnerException?.Message ?? ex.Message) ?? throw new ArgumentNullException(nameof(ex), "Message of exception is null");
            return ex.Data.Contains(ADD_EX_INDEX) ? exMessage + "\nMore info: " + ex.Data[ADD_EX_INDEX] : exMessage;
        }
    }
}
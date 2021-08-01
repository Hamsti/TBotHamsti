using System;

namespace TBotHamsti.Models
{
    public static class HandlerException
    {
        public const string ADD_EX_INDEX = "additional_exception";

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

        public static string GetExceptionMessage(Exception ex)
        {
            string exMessage = "The exception: " + (ex.InnerException?.Message ?? ex.Message) ?? throw new ArgumentNullException(nameof(ex), "Message of exception is null");
            return ex.Data.Contains(ADD_EX_INDEX) ? exMessage + "\nMore info: " + ex.Data[ADD_EX_INDEX] : exMessage;
        }
    }
}
using Domain.DtoModel;
using System;

namespace WebAPI.Services
{
    /// <summary>
    /// Event arguments for when a message is received
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// The received message
        /// </summary>
        public MessageDto Message { get; }

        /// <summary>
        /// Initializes a new instance of the MessageReceivedEventArgs class
        /// </summary>
        /// <param name="message">The received message</param>
        public MessageReceivedEventArgs(MessageDto message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
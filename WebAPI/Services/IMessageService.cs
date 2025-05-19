using Domain.DtoModel;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Services
{
    /// <summary>
    /// Interface for message service operations
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Event that is raised when a message is received
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Connects to the message hub
        /// </summary>
        Task Connect();

        /// <summary>
        /// Sends a message to a recipient
        /// </summary>
        /// <param name="recipientId">ID of the recipient</param>
        /// <param name="message">Message content</param>
        /// <param name="messageType">Type of the message (default: Text)</param>
        /// <returns>True if the message was sent successfully, false otherwise</returns>
        Task<bool> SendMessage(string recipientId, string message, string messageType = "Text");

        /// <summary>
        /// Sends a message to a conversation
        /// </summary>
        /// <param name="conversationId">ID of the conversation</param>
        /// <param name="message">Message content</param>
        /// <param name="messageType">Type of the message (default: Text)</param>
        /// <returns>True if the message was sent successfully, false otherwise</returns>
        Task<bool> SendMessageToConversation(int conversationId, string message, string messageType = "Text");

        /// <summary>
        /// Gets messages for a conversation
        /// </summary>
        /// <param name="conversationId">ID of the conversation</param>
        /// <returns>List of messages</returns>
        Task<List<MessageDto>> GetMessages(int conversationId);

        /// <summary>
        /// Gets conversations for the current user
        /// </summary>
        /// <returns>List of conversations</returns>
        Task<List<ConversationDto>> GetConversations();

        /// <summary>
        /// Marks messages in a conversation as read
        /// </summary>
        /// <param name="conversationId">ID of the conversation</param>
        Task MarkAsRead(int conversationId);
    }
}
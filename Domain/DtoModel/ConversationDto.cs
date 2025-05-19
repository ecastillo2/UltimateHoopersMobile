using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class ConversationDto
    {
        public int ConversationId { get; set; }
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string OtherUserAvatar { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }

        // UI helpers
        public bool HasUnread => UnreadCount > 0;
        public bool HasAvatar => !string.IsNullOrEmpty(OtherUserAvatar);
        public string OtherUserInitials => string.Join("", OtherUserName?.Split(' ').Select(n => n[0]) ?? Array.Empty<char>());
    }

    public class MessageDto
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; } = "Text";
        public DateTime Timestamp { get; set; }

        // UI helpers
        public bool IsSentByMe => App.CurrentUserId != null && SenderId.ToString() == App.CurrentUserId;
        public bool IsReceivedByMe => !IsSentByMe;
        public bool IsTextMessage => MessageType == "Text";
        public bool IsImageMessage => MessageType == "Image";
        public bool IsFileMessage => MessageType == "File";
        public bool IsLocationMessage => MessageType == "Location";
    }

    public class SendMessageRequest
    {
        public int? ConversationId { get; set; }
        public int RecipientId { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; }
    }
}
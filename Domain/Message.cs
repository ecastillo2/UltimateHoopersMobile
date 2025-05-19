using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    // Add to Domain folder
    public class Message
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public string SenderId { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; } = "Text";
        public DateTime SentAt { get; set; }

        // Navigation properties
        public Conversation Conversation { get; set; }
    }

    public class Conversation
    {
        public int ConversationId { get; set; }
        public DateTime LastMessageAt { get; set; }

        // Navigation properties
        public ICollection<Message> Messages { get; set; }
        public ICollection<ConversationParticipant> Participants { get; set; }
    }

    public class ConversationParticipant
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string UserId { get; set; }
        public int? LastReadMessageId { get; set; }

        // Navigation properties
        public Conversation Conversation { get; set; }
    }
}

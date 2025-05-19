using DataLayer;
using DataLayer.DAL;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.Swagger;
using System.Security.Claims;
using WebAPI.Models;

namespace WebApi.Controllers
{
    /// <summary>
    /// Contact Controller
    /// </summary>
    // For ASP.NET Core API
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly HUDBContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(HUDBContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Get conversations for a user
        [HttpGet("conversations")]
        public async Task<ActionResult<List<ConversationDto>>> GetConversations()
        {
            var userId = GetCurrentUserId();

            var conversations = await _context.ConversationParticipant
                .Where(cp => cp.UserId == userId)
                .Select(cp => new
                {
                    cp.ConversationId,
                    Participants = _context.ConversationParticipant
                        .Where(p => p.ConversationId == cp.ConversationId && p.UserId != userId)
                        .Select(p => p.UserId).ToList(),
                    LastMessage = _context.Message
                        .Where(m => m.ConversationId == cp.ConversationId)
                        .OrderByDescending(m => m.SentAt)
                        .FirstOrDefault(),
                    UnreadCount = _context.Message
                        .Count(m => m.ConversationId == cp.ConversationId &&
                               m.SenderId != userId &&
                               m.MessageId > (cp.LastReadMessageId ?? 0))
                })
                .ToListAsync();

            var conversationDtos = new List<ConversationDto>();

            foreach (var conv in conversations)
            {
                // Get other user details - assuming 1:1 chats for simplicity
                var otherUserId = conv.Participants.FirstOrDefault();
                var otherUser = await _context.User.FindAsync(otherUserId);

                conversationDtos.Add(new ConversationDto
                {
                    ConversationId = conv.ConversationId,
                    OtherUserId = otherUserId,
                    OtherUserName = otherUser.DisplayName,
                    OtherUserAvatar = otherUser.AvatarUrl,
                    LastMessage = conv.LastMessage.Content,
                    LastMessageTime = conv.LastMessage.SentAt,
                    UnreadCount = conv.UnreadCount
                });
            }

            return conversationDtos.OrderByDescending(c => c.LastMessageTime).ToList();
        }

        // Get messages for a conversation
        [HttpGet("conversation/{conversationId}")]
        public async Task<ActionResult<List<MessageDto>>> GetMessages(int conversationId)
        {
            var userId = GetCurrentUserId();

            // Check if user is part of conversation
            var isParticipant = await _context.ConversationParticipant
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (!isParticipant)
                return Forbid();

            var messages = await _context.Message
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    MessageId = m.MessageId,
                    SenderId = m.SenderId,
                    Content = m.Content,
                    MessageType = m.MessageType,
                    Timestamp = m.SentAt
                })
                .ToListAsync();

            // Mark messages as read
            var participant = await _context.ConversationParticipant
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (participant != null && messages.Any())
            {
                participant.LastReadMessageId = messages.Last().MessageId;
                await _context.SaveChangesAsync();
            }

            return messages;
        }

        // Send a message
        [HttpPost]
        public async Task<ActionResult<MessageDto>> SendMessage(SendMessageRequest request)
        {
            var userId = GetCurrentUserId();
            int conversationId;

            // Check if conversation exists or create new one
            if (request.ConversationId.HasValue)
            {
                conversationId = request.ConversationId.Value;

                // Verify user is part of conversation
                var isParticipant = await _context.ConversationParticipant
                    .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

                if (!isParticipant)
                    return Forbid();
            }
            else
            {
                // Check if conversation already exists between these users
                var existingConversation = await _context.ConversationParticipant
                    .Where(cp => cp.UserId == userId)
                    .Select(cp => new
                    {
                        cp.ConversationId,
                        OtherParticipant = _context.ConversationParticipant
                            .Any(p => p.ConversationId == cp.ConversationId && p.UserId == request.RecipientId)
                    })
                    .FirstOrDefaultAsync(c => c.OtherParticipant);

                if (existingConversation != null)
                {
                    conversationId = existingConversation.ConversationId;
                }
                else
                {
                    // Create new conversation
                    var conversation = new Conversation();
                    _context.Conversation.Add(conversation);
                    await _context.SaveChangesAsync();

                    conversationId = conversation.ConversationId;

                    // Add participants
                    _context.ConversationParticipant.Add(new ConversationParticipant
                    {
                        ConversationId = conversationId,
                        UserId = userId
                    });

                    _context.ConversationParticipant.Add(new ConversationParticipant
                    {
                        ConversationId = conversationId,
                        UserId = request.RecipientId
                    });
                }
            }

            // Create message
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                Content = request.Content,
                MessageType = request.MessageType ?? "Text",
                SentAt = DateTime.UtcNow
            };

            _context.Message.Add(message);

            // Update conversation LastMessageAt
            var conversation = await _context.Conversation.FindAsync(conversationId);
            conversation.LastMessageAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Create DTO for response
            var messageDto = new MessageDto
            {
                MessageId = message.MessageId,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                Content = message.Content,
                MessageType = message.MessageType,
                Timestamp = message.SentAt
            };

            // Notify other participants via SignalR
            var participants = await _context.ConversationParticipant
                .Where(cp => cp.ConversationId == conversationId && cp.UserId != userId)
                .Select(cp => cp.UserId.ToString())
                .ToListAsync();

            foreach (var participantId in participants)
            {
                await _hubContext.Clients.User(participantId).SendAsync("ReceiveMessage", messageDto);
            }

            return messageDto;
        }

        // Mark messages as read
        [HttpPost("read/{conversationId}")]
        public async Task<ActionResult> MarkAsRead(int conversationId)
        {
            var userId = GetCurrentUserId();

            // Find last message in conversation
            var lastMessageId = await _context.Message
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.MessageId)
                .Select(m => m.MessageId)
                .FirstOrDefaultAsync();

            if (lastMessageId == 0)
                return Ok(); // No messages

            // Update last read message ID
            var participant = await _context.ConversationParticipant
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (participant != null)
            {
                participant.LastReadMessageId = lastMessageId;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        private int GetCurrentUserId()
        {
            // Get user ID from authenticated user
            // Implementation depends on your auth system
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}

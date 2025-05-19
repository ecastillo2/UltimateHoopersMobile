using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace WebAPI.Services
{
    public class MessageService : IMessageService
    {
        private readonly HttpClient _httpClient;
        private readonly HubConnection _hubConnection;
        private readonly string _userId;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public MessageService(IAuthService authService)
        {
            _userId = authService.GetUserId();

            // Set up HTTP client
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://your-api-url.com/")
            };

            string token = authService.GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Set up SignalR
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"https://your-api-url.com/chatHub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                })
                .WithAutomaticReconnect()
                .Build();

            // Set up event handler for new messages
            _hubConnection.On<MessageDto>("ReceiveMessage", (message) =>
            {
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
            });
        }

        public async Task Connect()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }
        }

        public async Task<bool> SendMessage(string recipientId, string message, string messageType = "Text")
        {
            try
            {
                var request = new SendMessageRequest
                {
                    RecipientId = int.Parse(recipientId),
                    Content = message,
                    MessageType = messageType
                };

                var response = await _httpClient.PostAsJsonAsync("api/messages", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending message: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendMessageToConversation(int conversationId, string message, string messageType = "Text")
        {
            try
            {
                var request = new SendMessageRequest
                {
                    ConversationId = conversationId,
                    Content = message,
                    MessageType = messageType
                };

                var response = await _httpClient.PostAsJsonAsync("api/messages", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending message: {ex.Message}");
                return false;
            }
        }

        public async Task<List<MessageDto>> GetMessages(int conversationId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<MessageDto>>($"api/messages/conversation/{conversationId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting messages: {ex.Message}");
                return new List<MessageDto>();
            }
        }

        public async Task<List<ConversationDto>> GetConversations()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<ConversationDto>>("api/messages/conversations");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting conversations: {ex.Message}");
                return new List<ConversationDto>();
            }
        }

        public async Task MarkAsRead(int conversationId)
        {
            try
            {
                await _httpClient.PostAsync($"api/messages/read/{conversationId}", null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error marking as read: {ex.Message}");
            }
        }
    }
}

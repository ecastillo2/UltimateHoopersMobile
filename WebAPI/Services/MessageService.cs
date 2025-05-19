using Domain.DtoModel;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
// Make sure to explicitly import the namespace containing MessageReceivedEventArgs
using WebAPI.Services;

namespace WebAPI.Services
{
    /// <summary>
    /// Implementation of the message service
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly HttpClient _httpClient;
        private readonly HubConnection _hubConnection;
        private readonly string _userId;

        /// <summary>
        /// Event that is raised when a message is received
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Initializes a new instance of the MessageService
        /// </summary>
        /// <param name="authService">Authentication service</param>
        /// <param name="configuration">Configuration</param>
        public MessageService(IAuthService authService, IConfiguration configuration)
        {
            if (authService == null) throw new ArgumentNullException(nameof(authService));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _userId = authService.GetUserId();

            // Set up HTTP client
            var apiBaseUrl = configuration["ApiClient:BaseUrl"] ?? "https://your-api-url.com/";
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiBaseUrl)
            };

            string token = authService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Set up SignalR
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{apiBaseUrl}chatHub", options =>
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        options.AccessTokenProvider = () => Task.FromResult(token);
                    }
                })
                .WithAutomaticReconnect()
                .Build();

            // Set up event handler for new messages
            _hubConnection.On<MessageDto>("ReceiveMessage", (message) =>
            {
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
            });
        }

        /// <summary>
        /// Connects to the message hub
        /// </summary>
        public async Task Connect()
        {
            try
            {
                if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StartAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error connecting to hub: {ex.Message}");
                // Consider logging or handling the exception as appropriate
            }
        }

        /// <summary>
        /// Sends a message to a recipient
        /// </summary>
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

        /// <summary>
        /// Sends a message to a conversation
        /// </summary>
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

        /// <summary>
        /// Gets messages for a conversation
        /// </summary>
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

        /// <summary>
        /// Gets conversations for the current user
        /// </summary>
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

        /// <summary>
        /// Marks messages in a conversation as read
        /// </summary>
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
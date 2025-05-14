using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using WebPush;

namespace DataLayer.DAL
{
    public class PushSubscriptionRepository : IPushSubscriptionRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       

        public PushSubscriptionRepository(HUDBContext context, IConfiguration configuration)
        {
            this.Configuration = configuration;
            this._context = context;
            
           
        }

        public async Task Subscribe(Domain.PushSubscription model)
        {
            using (var context = _context)
            {
                try
                {
                    Random random = new Random();

                    // Generate a random number between a range (e.g., 100000 and 999999)
                    int randomId = random.Next(100000, 1000000); // Will generate a 6-digit random number
                    model.SubscriptionId = randomId;
                    model.ExpirationTime = DateTime.UtcNow;

                    await context.PushSubscription.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
                //return Ok();
            }
        }

        public async Task SendNotification(NotificationMessage message, string userId)
        {
            using (var context = _context)
            {
                try
                {
                    // Retrieve the push subscription for the user
                    var pushSub = await context.PushSubscription
                                               .FirstOrDefaultAsync(model => model.UserId == userId);

                    if (pushSub == null)
                    {
                        Console.WriteLine($"No push subscription found for user: {userId}");
                        return;
                    }

                    // Extract VAPID details from configuration
                    var vapidDetails = new VapidDetails(
                        Configuration.GetSection("PushNotificationVar")["Subject"],
                        Configuration.GetSection("PushNotificationVar")["PublicKey"],
                        Configuration.GetSection("PushNotificationVar")["PrivateKey"]
                    );

                    var webPushClient = new WebPushClient();

                    // Create the notification payload
                    var payload = JsonSerializer.Serialize(new
                    {
                        title = message.Title,
                        body = message.Body,
                        url = message.Url
                    });

                    try
                    {
                        // Send the push notification
                        var subscription = new WebPush.PushSubscription(
                            pushSub.Endpoint,
                            pushSub.P256dh,
                            pushSub.Auth);

                        await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
                        Console.WriteLine($"Notification sent successfully to user: {userId}");
                    }
                    catch (WebPushException ex)
                    {
                        Console.WriteLine($"Error sending notification: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in SendNotification: {ex.Message}");
                }
            }
        }


        private object await(object value)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

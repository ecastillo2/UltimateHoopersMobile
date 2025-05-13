using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using SocialMedia.Common;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace DataLayer.DAL
{
    public class NotificationRepository : INotificationRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       

        public NotificationRepository(HUDBContext context)
        {
            this._context = context;
            
           
        }



        /// <summary>
        /// Get Tags
        /// </summary>
        /// <returns></returns>
        public async Task<List<Notification>> GetNotifications()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all tags and include the post count for each tag
                    var query = await context.Notification.ToListAsync();

                    foreach (var item in query)
                    {
                        item.Profile = await context.Profile
                           .Where(f => f.ProfileId == item.ProfileId)
                           .FirstOrDefaultAsync();

                        
                    }

                        return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return null;
                }
            }
        }


        /// <summary>
        /// Insert Tag
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertNotification(Notification model)
        {
            using (var context = _context)
            {
                try
                {
                    model.NotificationId = Guid.NewGuid().ToString();
                    model.CreatedDate = DateTime.Now.ToString();

                    await context.Notification.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }

        public async Task<List<Notification>> GetNotificationByProfileId(string ProfileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.Notification
                                       where model.ProfileId == ProfileId
                                       select model).ToListAsync();
                    foreach(var item in query)
                    {
                        // Convert Date to Relative Time
                        if (DateTime.TryParse(item.CreatedDate, out DateTime dateTime))
                        {
                            item.RelativeTime = RelativeTime.GetRelativeTime(dateTime, "America/New_York");
                        }
                        else
                        {
                            item.RelativeTime = "Unknown"; // Handle parsing errors
                        }
                    }

                   

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return null;
                }
            }
        }


        /// <summary>
        /// Update Post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateNotification(Notification model)
        {
            using (var context = _context)
            {
                var existingItem = context.Notification.Where(s => s.NotificationId == model.NotificationId).FirstOrDefault<Notification>();

                if (existingItem != null)
                {
                    existingItem.Read = model.Read;
                    existingItem.Title = model.Title;
                    existingItem.Description = model.Description;
                    existingItem.Status = model.Status;
              

                    context.Notification.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update Profile UserName
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateNotificationRead(string NotificationId)
        {
            using (var context = _context)
            {
                var existingItem = context.Notification.Where(s => s.NotificationId == NotificationId).FirstOrDefault<Notification>();

                if (existingItem != null)
                {
                    existingItem.Read = true;

                    context.Notification.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Get Product By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<Notification> GetNotificationById(string NotificationId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.Notification
                                       where model.NotificationId == NotificationId
                                       select model).FirstOrDefaultAsync();


                   
                        // Convert Date to Relative Time
                        if (DateTime.TryParse(query.CreatedDate, out DateTime dateTime))
                        {
                        query.RelativeTime = RelativeTime.GetRelativeTime(dateTime, "America/New_York");
                        }
                        else
                        {
                        query.RelativeTime = "Unknown"; // Handle parsing errors
                        }
                    


                    return query;
                }
                catch (Exception ex)
                {
                    // Handle the exception or log it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// DeleteTag
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task DeleteNotification(string NotificationId)
        {
            using (var context = _context)
            {
                Notification obj = (from u in context.Notification
                           where u.NotificationId == NotificationId
                           select u).FirstOrDefault();



                _context.Notification.Remove(obj);
                await Save();
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }
         
       


    }
}

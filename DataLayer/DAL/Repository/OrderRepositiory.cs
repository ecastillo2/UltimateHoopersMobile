using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.Context;

namespace DataLayer.DAL.Repository
{
    public class OrderRepository : IOrderRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private ApplicationContext _context;
       
        public OrderRepository(ApplicationContext context)
        {
            _context = context;

        }

        /// <summary>
        /// Get Order By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<Order> GetOrderById(string OrderId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.Order
                                       where model.OrderId == OrderId
                                       select model).FirstOrDefaultAsync();

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
        /// Get Order By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<List<Order>> GetOrderByProfileId(string profileId)
        {
            try
            {
                var query = await (from order in _context.Order
                                   join profile in _context.Profile
                                   on order.ProfileId equals profile.ProfileId

                                   join privateRun in _context.Run
                                   on order.RunId equals privateRun.RunId into privateRunGroup
                                   from privateRun in privateRunGroup.DefaultIfEmpty() // Left join to handle nulls

                                   where order.ProfileId == profileId // Filter for specific ProfileId

                                   select new Order
                                   {
                                       OrderId = order.OrderId,
                                       Id = order.Id,
                                       ProfileId = order.ProfileId,
                                       Name = order.Name,
                                       OrderNumber = order.OrderNumber,
                                       ConfirmationCode = order.ConfirmationCode,
                                       Status = order.Status,
                                       OrderDate = order.OrderDate,
                                       CompletedOrderDate = order.CompletedOrderDate,
                                       TransId = order.TransId,
                                       Notes = order.Notes,
                                       Comments = order.Comments,
                                       Payment = order.Payment,
                                       OrderJson = order.OrderJson,
                                       Type = order.Type,
                                       State = order.State,
                                       Zip = order.Zip,
                                       TrackingNumber = order.TrackingNumber,
                                       ShippingAddress = order.ShippingAddress,
                                       ShippingFirstName = order.ShippingFirstName,
                                       ShippingLastName = order.ShippingLastName,
                                       ShippingCity = order.ShippingCity,
                                       ShippingState = order.ShippingState,
                                       ShippingZip = order.ShippingZip,
                                       PointsUsed = order.PointsUsed,
                                       PaymentMethod = order.PaymentMethod,

                                       Profile = new Profile
                                       {
                                           ProfileId = profile.ProfileId,
                                           UserId = profile.UserId,
                                           UserName = profile.UserName,
                                           ImageURL = profile.ImageURL,
                                       },

                                       Run = privateRun != null ? new Run
                                       {
                                           RunId = privateRun.RunId,
                                           RunNumber = privateRun.RunNumber
                                       } : null // Handle null PrivateRun
                                   }).ToListAsync();

                return query;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching orders: {ex.Message}");
                return new List<Order>(); // Return an empty list instead of null
            }
        }


        /// <summary>
        /// Get Orders
        /// </summary>
        /// <returns></returns>
        public async Task<List<Order>> GetOrders()
        {
            using (var context = _context)
            {
                try
                {
                    var query = await (from order in _context.Order
                                       join profile in _context.Profile
                                       on order.ProfileId equals profile.ProfileId
                                       join privateRun in _context.Run
                                       on order.RunId equals privateRun.RunId into privateRunGroup
                                       from privateRun in privateRunGroup.DefaultIfEmpty() // Left join to handle nulls

                                       select new Order
                                       {
                                           OrderId = order.OrderId,
                                           Id = order.Id,
                                           ProfileId = order.ProfileId,
                                           Name = order.Name,
                                           OrderNumber = order.OrderNumber,
                                           ConfirmationCode = order.ConfirmationCode,
                                           Status = order.Status,
                                           OrderDate = order.OrderDate,
                                           CompletedOrderDate = order.CompletedOrderDate,
                                           TransId = order.TransId,
                                           Notes = order.Notes,
                                           Comments = order.Comments,
                                           Payment = order.Payment,
                                           OrderJson = order.OrderJson,
                                           Type = order.Type,
                                           State = order.State,
                                           Zip = order.Zip,
                                           TrackingNumber = order.TrackingNumber,
                                           ShippingAddress = order.ShippingAddress,
                                           ShippingFirstName = order.ShippingFirstName,
                                           ShippingLastName = order.ShippingLastName,
                                           ShippingCity = order.ShippingCity,
                                           ShippingState = order.ShippingState,
                                           ShippingZip = order.ShippingZip,
                                           PointsUsed = order.PointsUsed,
                                           PaymentMethod = order.PaymentMethod,

                                           Profile = new Profile
                                           {
                                               ProfileId = profile.ProfileId,
                                               UserId = profile.UserId,
                                               UserName = profile.UserName,
                                               ImageURL = profile.ImageURL,
                                           },

                                           Run = privateRun != null ? new Run
                                           {
                                               RunId = privateRun.RunId,
                                               RunNumber = privateRun.RunNumber
                                           } : null // Handle null PrivateRun
                                       }).ToListAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    Console.WriteLine($"Error fetching orders: {ex.Message}");
                    return null;
                }
            }
        }


        /// <summary>
        /// Insert Tag
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertOrder(Order model)
        {
            using (var context = _context)
            {
                try
                {

                    model.OrderDate = DateTime.Now;

                    await context.Order.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }

        /// <summary>
        /// Update Post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateOrder(Order model)
        {
            using (var context = _context)
            {
                var existingItem = context.Order.Where(s => s.OrderId == model.OrderId).FirstOrDefault<Order>();

                if (existingItem != null)
                {

                    existingItem.Status = model.Status;
                    existingItem.Notes = model.Notes;
                    existingItem.TrackingNumber = model.TrackingNumber;



                    context.Order.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Delete Order
        /// </summary>
        /// <param name="CourtId"></param>
        /// <returns></returns>
        public async Task DeleteOrder(string OrderId)
        {
            using (var context = _context)
            {
                Order obj = (from u in context.Order
                             where u.OrderId == OrderId
                             select u).FirstOrDefault();

                _context.Order.Remove(obj);
                await Save();
            }
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}

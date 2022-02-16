using AutoMapper;
using Mango.Services.OrderAPI.DbContexts;
using Mango.Services.OrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.OrderAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContext;
        public OrderRepository(DbContextOptions<ApplicationDbContext> dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> AddOrder(OrderHeader orderHeader)
        {
            await using var db = new ApplicationDbContext(_dbContext);
            db.OrderHeaders.Add(orderHeader);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task UpdateOrderPaymentStatus(int orderHeaderId, bool paid)
        {
            await using var db = new ApplicationDbContext(_dbContext);
            var orderHeaderFromDb = await db.OrderHeaders.FirstOrDefaultAsync(x => x.OrderHeaderId == orderHeaderId);
            if (orderHeaderFromDb != null)
            {
                orderHeaderFromDb.PaymentStatus = paid;
                await db.SaveChangesAsync();
            }
        }
    }
}

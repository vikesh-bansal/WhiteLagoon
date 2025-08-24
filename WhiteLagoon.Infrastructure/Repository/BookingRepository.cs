using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        ApplicationDbContext _context;
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public void Update(Booking entity)
        {
            _context.Update(entity);
        }

        public void UpdateStatus(int bookingId, string bookingStatus)
        {
            var bookingDb = _context.Bookings.Where(x => x.Id == bookingId).FirstOrDefault();
            if (bookingDb != null)
            {
                bookingDb.Status = bookingStatus;
                if (bookingDb.Status == SD.StatusCheckedIn)
                {
                    bookingDb.ActualCheckInDate = DateTime.Now;
                }

                if (bookingDb.Status == SD.StatusCompleted)
                {
                    bookingDb.ActualCheckOutDate = DateTime.Now;
                }
            }
        }

        public void UpdateStripePaymentId(int bookingId, string sessionId, string paymentIntentId)
        {
            var bookingFromDb = _context.Bookings.FirstOrDefault(x => x.Id == bookingId);
            if (bookingFromDb != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    bookingFromDb.StripeSessionId= sessionId;
                }

                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    bookingFromDb.StripePaymentIntentId= paymentIntentId;
                    bookingFromDb.PaymentDate=DateTime.Now;
                    bookingFromDb.IsPaymentSuccessful= true;
                }
            }
        }
    }
}

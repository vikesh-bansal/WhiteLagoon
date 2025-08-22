using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    public class BookingController : Controller
    {
        IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork) { 
        _unitOfWork = unitOfWork;
        }
        public IActionResult FinalizeBooking(int villaId, DateOnly checkIndate, int nights)
        {
            Booking booking = new Booking
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkIndate,
                Nights = nights,
                CheckOutDate= checkIndate.AddDays(nights)
            };
            return View(booking);
        }
    }
}

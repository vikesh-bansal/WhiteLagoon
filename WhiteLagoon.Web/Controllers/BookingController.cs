using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout; 
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    public class BookingController : Controller
    {
        IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork) { 
        _unitOfWork = unitOfWork;
        }
        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkIndate, int nights)
        {
            var claimsIdentity =(ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ApplicationUser user = _unitOfWork.User.Get(u => u.Id == userId);

            Booking booking = new Booking
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkIndate,
                Nights = nights,
                CheckOutDate= checkIndate.AddDays(nights),
                UserId= userId,
                Phone=user.PhoneNumber,
                Email=user.Email,
                Name=user.Name
            };
            booking.Totalcost = booking.Villa.Price * nights;
            return View(booking);
        }
        [HttpPost]
        [Authorize]
        public IActionResult FinalizeBooking(Booking booking) { 
            var villa = _unitOfWork.Villa.Get(x=> x.Id == booking.VillaId);
                booking.Totalcost = villa.Price * booking.Nights;
                booking.Status = SD.StatusPending;
                booking.BookingDate=DateTime.Now;
                _unitOfWork.Booking.Add(booking);
                _unitOfWork.Save();
            //return RedirectToAction(nameof(BookingConfirmation), new { bookingId = booking.Id });
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"/booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"/booking/FinalBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}"
            };

            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount=(long)(booking.Totalcost*100),
                    Currency="usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name
                        //Images=new List<string> { domain + villa.ImageUrl }
                    }
                },
                Quantity=1,
            });

            var service = new SessionService();
            Session session=service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId) { 
            return View(bookingId);
        }
    }
}

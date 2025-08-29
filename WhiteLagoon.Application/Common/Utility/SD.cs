using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Utility
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public static int VillaRoomsAvailable_Count(int villaId, List<VillaNumber> villaNumberList, DateOnly checkInDate, int nights, List<Booking> bookings)
        {
            List<int> bookingInDate = new List<int>();
            int finalAvaliableRoomForAllNights = int.MaxValue;

            var roomsInVilla = villaNumberList.Where(x => x.VillaId == villaId).Count();
            for(int i = 0; i < nights; i++)
            {
                var villaBooked= bookings.Where(x => x.CheckInDate <= checkInDate.AddDays(i) && x.CheckOutDate > checkInDate.AddDays(i) && x.VillaId == villaId);
                foreach(var booking in villaBooked)
                {
                    bookingInDate.Add(booking.Id);
                }

                var totalAvaliableRooms = roomsInVilla - bookingInDate.Count;
                if (totalAvaliableRooms == 0)
                {
                    return 0;
                }
                else
                {
                    if (finalAvaliableRoomForAllNights > totalAvaliableRooms)
                    {
                        finalAvaliableRoomForAllNights = totalAvaliableRooms;
                    }
                }
            }


            return finalAvaliableRoomForAllNights;
        }
    }
}

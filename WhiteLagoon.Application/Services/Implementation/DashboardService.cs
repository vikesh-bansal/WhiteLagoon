using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Application.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<PieChartDto> GetBookingPieChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) && (u.Status != SD.StatusPending && u.Status != SD.StatusCancelled));
            var customerWithOneBooking = totalBookings.GroupBy(b => b.UserId).Where(x => x.Count() == 1).Select(x => x.Key).ToList();

            int bookingsByNewCustomer = customerWithOneBooking.Count();
            int bookingsByReturningCustomer = totalBookings.Count() - bookingsByNewCustomer;

            PieChartDto PieChartDto = new()
            {
                Labels = new string[] { "New Customer Bookings", "Returning Customer Bookings" },
                Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer }
            };
            return PieChartDto;
        }

        public async Task<LineChartDto> GetMemberAndBookingLineChartData()
        {
            var bookingData=_unitOfWork.Booking.GetAll(x=> x.BookingDate>=DateTime.Now.AddDays(-30) && x.BookingDate< DateTime.Now).GroupBy(x=> x.BookingDate.Date).Select(x=> new { DateTime=x.Key, NewBookingCount=x.Count() });
            var customerData = _unitOfWork.User.GetAll(x=> x.CreatedAt>=DateTime.Now.AddDays(-30) && x.CreatedAt<DateTime.Now).GroupBy(x=> x.CreatedAt.Date).Select(x=> new { DateTime = x.Key,NewCustomerCount=x.Count() });

            var allDates = bookingData.Select(b => b.DateTime).Union(customerData.Select(c => c.DateTime)).Distinct().OrderBy(d => d);
            var mergedData = allDates.Select(date => new { DateTime =date, NewBookingCount = bookingData.FirstOrDefault(x=> x.DateTime==date)?.NewBookingCount ??0, 
                                                           NewCustomerCount =customerData.FirstOrDefault(x=> x.DateTime ==date)?.NewCustomerCount??0 }); 

            var newBookingData=mergedData.Select(x => x.NewBookingCount).ToArray();
            var newCustomerData= mergedData.Select(x=> x.NewCustomerCount).ToArray();
            var categories = mergedData.Select(x => x.DateTime.ToString("MM/dd/yyyy")).ToArray();
            List<ChartData> chartDataList = new List<ChartData>()
            {
                new ChartData
                {
                    Name="New Booking",
                    Data=newBookingData
                },
                new ChartData {
                    Name="New Members",
                    Data=newCustomerData
                }
            };
            LineChartDto lineChartDto = new LineChartDto()
            {
                Categories = categories,
                Series = chartDataList
            };
            return lineChartDto;
        }


        public async Task<RadialBarChartDto> GetRegisteredUserChartData()
        {
            var totalUsers = _unitOfWork.User.GetAll();
            var countByCurrentMonth = totalUsers.Count(u => u.CreatedAt >= currentMonthStartDate && u.CreatedAt <= DateTime.Now);

            var countByPreviousMonth = totalUsers.Count(u=> u.CreatedAt>previousMonthStartDate && u.CreatedAt <= currentMonthStartDate);
            return SD.GetRadialChartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth);
        }
        
        public async Task<RadialBarChartDto> GetRevenueChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.Status != SD.StatusPending && u.Status != SD.StatusCancelled);
            var totalRevenue = Convert.ToInt32(totalBookings.Sum(u=> u.Totalcost));

            var countByCurrentMonth = totalBookings.Where(u=> u.BookingDate>=currentMonthStartDate && u.BookingDate<=DateTime.Now).Sum(u=> u.Totalcost);
            var countByPreviousMonth = totalBookings.Where(u=> u.BookingDate>=previousMonthStartDate && u.BookingDate<=currentMonthStartDate).Sum(u=> u.Totalcost);
            return SD.GetRadialChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth);
        }

        public async Task<RadialBarChartDto> GetTotalBookingRadialChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u=> u.Status != SD.StatusPending && u.Status != SD.StatusCancelled);
            var countByCurrentMonth = totalBookings.Count(u=> u.BookingDate>=currentMonthStartDate && u.BookingDate<=DateTime.Now);
            var countByPreviousMonth = totalBookings.Count(u=> u.BookingDate> previousMonthStartDate && u.BookingDate> currentMonthStartDate);
            return SD.GetRadialChartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth);
        }
    }
}

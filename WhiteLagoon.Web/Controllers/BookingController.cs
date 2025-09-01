using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;


namespace WhiteLagoon.Web.Controllers
{
    public class BookingController : Controller
    {
        IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookingController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment) { 
        _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
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
            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u=> u.Status == SD.StatusApproved || u.Status == SD.StatusCheckedIn).ToList();
            int roomAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumberList, booking.CheckInDate, booking.Nights, bookedVillas);
            if(roomAvailable == 0)
            {
                TempData["error"] = "Room has been sold out";
                return RedirectToAction(nameof(FinalizeBooking), new {villaId = booking.VillaId, checkInDate = booking.CheckInDate, nights = booking.Nights});
            }

                _unitOfWork.Booking.Add(booking);
                _unitOfWork.Save();
            //return RedirectToAction(nameof(BookingConfirmation), new { bookingId = booking.Id });
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}"
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

            _unitOfWork.Booking.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId) { 
         
            Booking bookingFromDb=_unitOfWork.Booking.Get(u=> u.Id==bookingId, includeProperties:"User,Villa");

            if(bookingFromDb.Status == SD.StatusPending)
            {
                // this is a pending order, we need to confirm if payment was successful

                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);
                if (session.PaymentStatus == "paid")
                {
                    _unitOfWork.Booking.UpdateStatus(bookingId, SD.StatusApproved,0);
                    _unitOfWork.Booking.UpdateStripePaymentId(bookingId, session.Id,session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }
            return View(bookingId);
        }

        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> bookings;
            if (User.IsInRole(SD.Role_Admin))
            { 
                bookings=_unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
            }
            else
            {
                var claimsIdentity=(ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                bookings = _unitOfWork.Booking.GetAll((u => u.UserId == userId), includeProperties: "User,Villa");
            }

            if (!string.IsNullOrEmpty(status))
            {
                bookings=bookings.Where(x=> x.Status.ToLower().Equals(status.ToLower())).ToList();
            }
            return Json(new { data = bookings });
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
            if(bookingFromDb.VillaNumber==0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumber=AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);

                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll(u => u.VillaId == bookingFromDb.VillaId && availableVillaNumber.Any(x => x == u.Villa_Number)).ToList();

            }
            return View(bookingFromDb);
        }
        [Authorize(Roles =SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails),new {bookingId=booking.Id});
        }
        [Authorize(Roles =SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Completed Successfully";
            return RedirectToAction(nameof(BookingDetails),new {bookingId= booking.Id});
        }
        [Authorize(Roles =SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Cancelled Successfully";
            return RedirectToAction(nameof(BookingDetails),new { bookingId=booking.Id});
        }
        private List<int> AssignAvailableVillaNumberByVilla(int villaId) {

            List<int> availableVillaNumbers = new List<int>();
            var villaNumbers=_unitOfWork.VillaNumber.GetAll(u=> u.VillaId==villaId);

            var checkedInVilla=_unitOfWork.Booking.GetAll(u=> u.VillaId == villaId && u.Status ==SD.StatusCheckedIn).Select(u=> u.VillaNumber).ToList();

            foreach(var villaNum in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNum.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNum.Villa_Number); ;
                }
            }
            return availableVillaNumbers;
        }

        [HttpPost]
        [Authorize]
        public IActionResult GenerateInvoice(int id, string downloadType) {

            string basePath = _webHostEnvironment.WebRootPath;

            WordDocument wordDocument = new WordDocument();
            string dataPath = basePath + @"/exports/BookingDetails.docx";
            using FileStream fileStream = new (dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            wordDocument.Open(fileStream, Syncfusion.DocIO.FormatType.Automatic);

            //Update Template
            Booking bookingFromDb=_unitOfWork.Booking.Get(u=> u.Id==id, includeProperties: "User,Villa");

            TextSelection textSelection = wordDocument.Find("xx_customer_name", false, true);
            WTextRange textRange=textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Name;

            textSelection =wordDocument.Find("xx_customer_phone",false,true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text=bookingFromDb.Name;

            textSelection = wordDocument.Find("xx_customer_email",false, true);
            textRange=textSelection.GetAsOneRange();
            textRange.Text=bookingFromDb.Email;

            textSelection = wordDocument.Find("XX_BOOKING_NUMBER", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING ID - " + bookingFromDb.Id;
            textRange = textSelection.GetAsOneRange();


            textSelection = wordDocument.Find("XX_BOOKING_DATE", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING DATE - " + bookingFromDb.BookingDate.ToShortDateString();

            textSelection = wordDocument.Find("xx_payment_date", false, true);
            textRange= textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.PaymentDate.ToShortDateString();
            textSelection = wordDocument.Find("xx_checkin_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckInDate.ToShortDateString();

            textSelection = wordDocument.Find("xx_checkout_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text=bookingFromDb.CheckOutDate.ToShortDateString();
            textSelection = wordDocument.Find("xx_booking_total",false,true);
            textRange=textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Totalcost.ToString("c");

            WTable table = new(wordDocument);
            table.TableFormat.Borders.LineWidth = 1f;
            table.TableFormat.Borders.Color = Color.Black;
            table.TableFormat.Paddings.Top = 7f;
            table.TableFormat.Paddings.Bottom = 7f;
            table.TableFormat.Borders.Horizontal.LineWidth = 1f;

            int rows = bookingFromDb.VillaNumber > 0 ? 3 : 2;
            table.ResetCells(rows, 4);

            WTableRow row0=table.Rows[0];
            row0.Cells[0].AddParagraph().AppendText("NIGHTS");
            row0.Cells[0].Width = 80;
            row0.Cells[1].AddParagraph().AppendText("VILLA");
            row0.Cells[1].Width = 220;
            row0.Cells[2].AddParagraph().AppendText("PRICE PER NIGHT");
            row0.Cells[3].AddParagraph().AppendText("TOTAL");
            row0.Cells[2].Width = 80;

            WTableRow row1=table.Rows[1];
            row1.Cells[0].AddParagraph().AppendText(bookingFromDb.Nights.ToString());
            row1.Cells[0].Width = 80;
            row1.Cells[1].AddParagraph().AppendText(bookingFromDb.Villa.Name);
            row1.Cells[1].Width = 220;
            row1.Cells[2].AddParagraph().AppendText((bookingFromDb.Totalcost/bookingFromDb.Nights).ToString("c"));
            row1.Cells[3].AddParagraph().AppendText(bookingFromDb.Totalcost.ToString("c"));
            row1.Cells[3].Width = 80;

            if (bookingFromDb.VillaNumber > 0)
            {
                WTableRow row2= table.Rows[2];
                row2.Cells[0].Width = 80;
                row2.Cells[1].AddParagraph().AppendText("Villa Number - " + bookingFromDb.VillaNumber.ToString());
                row2.Cells[1].Width = 220;
                row2.Cells[3].Width = 80;
            }
            WTableStyle tableStyle = wordDocument.AddTableStyle("CustomStyle") as WTableStyle;
            tableStyle.TableProperties.RowStripe = 1;
            tableStyle.TableProperties.ColumnStripe = 2;
            tableStyle.TableProperties.Paddings.Top = 2;
            tableStyle.TableProperties.Paddings.Bottom = 1;
            tableStyle.TableProperties.Paddings.Left = 5.4f;
            tableStyle.TableProperties.Paddings.Right = 5.4f;

            ConditionalFormattingStyle firstRowStyle= tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
            firstRowStyle.CharacterFormat.Bold = true;
            firstRowStyle.CharacterFormat.TextColor= Color.FromArgb(255,255, 255, 255);
            firstRowStyle.CellProperties.BackColor = Color.Black;
            table.ApplyStyle("CustomStyle");
            TextBodyPart bodyPart=new(wordDocument);
            bodyPart.BodyItems.Add(table);
            wordDocument.Replace("<ADDTABLEHERE>", bodyPart, false, false);
           


            using DocIORenderer render = new();
            MemoryStream stream = new MemoryStream();
            if (downloadType == "word")
            {
                wordDocument.Save(stream, Syncfusion.DocIO.FormatType.Docx);
                stream.Position = 0;

            }
            else
            {
                PdfDocument pdfDocument = render.ConvertToPDF(wordDocument);
                pdfDocument.Save(stream);
                stream.Position = 0;
                return File(stream, "application/pdf", "BookingDetails.pdf");
            }
            stream.Position = 0;

            return File(stream, "application/docx", "BookingDetails.docx");

        }
    }
}


using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities; 
namespace WhiteLagoon.Application.Services.Implementation
{
    public class VillaService : IVillaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VillaService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public void CreateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\Villa");
                using (var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                {
                    villa.Image.CopyTo(fileStream);
                }
                villa.ImageUrl = @"images\Villa\" + fileName;
            }
            else {
                villa.ImageUrl = "https://placehold.co/600x400";
            }
            _unitOfWork.Villa.Add(villa);
            _unitOfWork.Save();

        }

        public bool DeleteVilla(int id)
        {
            try
            {
                Villa? _villa = _unitOfWork.Villa.Get(x => x.Id == id);
                if (_villa != null)
                {
                    if (_villa.ImageUrl != null)
                    {
                        string oldImageUrl = Path.Combine(_webHostEnvironment.WebRootPath, _villa.ImageUrl.TrimStart('\\'));
                        if (File.Exists(oldImageUrl))
                        {
                            File.Delete(oldImageUrl);
                        }
                    }
                    _unitOfWork.Villa.Delete(_villa);
                    _unitOfWork.Save();
                }
                return true;
            }
            catch(Exception) {
                return false;
            }
            
        }

        public IEnumerable<Villa> GetAllVillas()
        {
           return _unitOfWork.Villa.GetAll(includeProperties:"VillaAmenity");
        }

        public Villa GetVillaId(int id)
        {
           return _unitOfWork.Villa.Get(x=> x.Id == id, includeProperties: "VillaAmenity");
        }

        public IEnumerable<Villa> GetVillasAvailabilityByDate(int nights, DateOnly checkInDate)
        {

            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved || u.Status == SD.StatusCheckedIn).ToList();

            foreach (var villa in villaList) {
                int roomAvaliable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumbersList, checkInDate, nights, bookedVillas);
                villa.IsAvailable = roomAvaliable > 0 ? true : false;
            }
            return villaList;
        }

        public bool IsVillaAvailableByDate(int villaId, int nights, DateOnly checkInDate)
        {
          var villaNumberList=_unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas=_unitOfWork.Booking.GetAll(u=> u.Status==SD.StatusApproved || u.Status==SD.StatusCheckedIn).ToList();
            int roomAvailable = SD.VillaRoomsAvailable_Count(villaId, villaNumberList, checkInDate, nights, bookedVillas);

            return roomAvailable > 0;
        }

        public void UpdateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\Villa");
                using (var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                {
                    villa.Image.CopyTo(fileStream);
                }
                if (!string.IsNullOrEmpty(villa.ImageUrl))
                {
                    string imageOldUrl = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));
                    if (!File.Exists(imageOldUrl))
                    {
                        File.Delete(imageOldUrl);
                    }
                }
                villa.ImageUrl = @"images\Villa\" + fileName;
            }
            _unitOfWork.Villa.Update(villa);
            _unitOfWork.Save();
        }
    }
}

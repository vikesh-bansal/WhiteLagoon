using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Repository;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork _unityOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AmenityController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unityOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {
            var villaList = _unityOfWork.Amenity.GetAll(includeProperties: "Villa");
            return View(villaList);
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> list = _unityOfWork.Amenity.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            AmenityVM amenity = new AmenityVM { VillaList = list };
            return View(amenity);
        }
        [HttpPost]
        public IActionResult Create(AmenityVM amenityVM)
        { 
            if (ModelState.IsValid)
            {
                _unityOfWork.Amenity.Add(amenityVM.Amenity);
                _unityOfWork.Save();

                TempData["success"] = "The amenity has been created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                IEnumerable<SelectListItem> list = _unityOfWork.Villa.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                amenityVM.VillaList = list;
                return View(amenityVM);
            }
        }
        public IActionResult Update(int Id)
        {
            Amenity? amenity = _unityOfWork.Amenity.Get(x => x.Id == Id);
            if (amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }
            else
            {
                IEnumerable<SelectListItem> list = _unityOfWork.Villa.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                AmenityVM amenityVM = new AmenityVM { VillaList = list };
                return View(amenityVM);
            }
        }

        [HttpPost]
        public IActionResult Update(AmenityVM amenityVM)
        {


            if (ModelState.IsValid)
            {
                _unityOfWork.Amenity.Update(amenityVM.Amenity);
                _unityOfWork.Save();

                TempData["success"] = "The amenity has been updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {

                IEnumerable<SelectListItem> list = _unityOfWork.Villa.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                amenityVM.VillaList = list;
                return View(amenityVM);
            }
        }

        public IActionResult Delete(int amenityId)
        {
            Amenity? amenity = _unityOfWork.Amenity.Get(x => x.Id == amenityId);
            if (amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }
            else
            {
                IEnumerable<SelectListItem> list = _unityOfWork.Villa.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                AmenityVM amenityVM = new AmenityVM { Amenity = amenity, VillaList = list };
                return View(amenityVM);
            }
        }

        [HttpPost]
        public IActionResult Delete(AmenityVM amenityVM)
        {

            Amenity? _amenity = _unityOfWork.Amenity.Get(x => x.Id == amenityVM.Amenity.Id);
            if (_amenity != null)
            {
                _unityOfWork.Amenity.Delete(_amenity);
                _unityOfWork.Save();
                TempData["success"] = "The amenity has been deleted successfully.";
                return RedirectToAction("Index");
            }
            else
            {

                IEnumerable<SelectListItem> list = _unityOfWork.Villa.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                amenityVM.VillaList = list;
                return View(amenityVM);
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities; 
using WhiteLagoon.Web.ViewModels; 
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;
namespace WhiteLagoon.Web.Controllers
{
    [Authorize(Roles =SD.Role_Admin)]
    public class AmenityController : Controller
    {
        private readonly IAmenityService _amenityService;
        private readonly IVillaService _villaService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AmenityController(IAmenityService amenityService, IVillaService villaService, IWebHostEnvironment webHostEnvironment)
        {
            _amenityService = amenityService;
            _villaService = villaService;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {
            var villaList = _amenityService.GetAllAmenities();
            return View(villaList);
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> list = _amenityService.GetAllAmenities().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            AmenityVM amenity = new AmenityVM { VillaList = list };
            return View(amenity);
        }
        [HttpPost]
        public IActionResult Create(AmenityVM amenityVM)
        { 
            if (ModelState.IsValid)
            {
                _amenityService.CreateAmenity(amenityVM.Amenity); 

                TempData["success"] = "The amenity has been created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                IEnumerable<SelectListItem> list = _villaService.GetAllVillas().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                amenityVM.VillaList = list;
                return View(amenityVM);
            }
        }
        public IActionResult Update(int Id)
        {
            Amenity? amenity = _amenityService.GetAmenityById(Id);
            if (amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }
            else
            {
                IEnumerable<SelectListItem> list = _villaService.GetAllVillas().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                AmenityVM amenityVM = new AmenityVM { VillaList = list };
                return View(amenityVM);
            }
        }

        [HttpPost]
        public IActionResult Update(AmenityVM amenityVM)
        {


            if (ModelState.IsValid)
            {
                _amenityService.UpdateAmenity(amenityVM.Amenity); 

                TempData["success"] = "The amenity has been updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {

                IEnumerable<SelectListItem> list = _villaService.GetAllVillas().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                amenityVM.VillaList = list;
                return View(amenityVM);
            }
        }

        public IActionResult Delete(int amenityId)
        {
            Amenity? amenity = _amenityService.GetAmenityById(amenityId);
            if (amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }
            else
            {
                IEnumerable<SelectListItem> list = _villaService.GetAllVillas().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                AmenityVM amenityVM = new AmenityVM { Amenity = amenity, VillaList = list };
                return View(amenityVM);
            }
        }

        [HttpPost]
        public IActionResult Delete(AmenityVM amenityVM)
        {
             
            if (_amenityService.DeleteAmenity(amenityVM.Amenity.Id))
            { 
                TempData["success"] = "The amenity has been deleted successfully.";
                return RedirectToAction("Index");
            }
            else
            {

                IEnumerable<SelectListItem> list = _villaService.GetAllVillas().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                amenityVM.VillaList = list;
                return View(amenityVM);
            }
        }
    }
}

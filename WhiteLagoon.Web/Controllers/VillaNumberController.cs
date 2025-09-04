using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Implementation;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities; 
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IVillaNumberService _villaNumberService;
        private readonly IVillaService _villaService;
        public VillaNumberController(IVillaNumberService villaNumberService, VillaService villaService)
        {
            _villaNumberService = villaNumberService;
            _villaService = villaService;
        }

        public IActionResult Index()
        { 
            return View(_villaNumberService.GetAllVillaNumbers());
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> list = _villaService.GetAllVillas().Select(u=> new SelectListItem { Text=u.Name, Value=u.Id.ToString() });
            VillaNumberVM villaNumber = new VillaNumberVM { VillaList = list };
            return View(villaNumber);
        }
        [HttpPost]
        public IActionResult Create(VillaNumberVM villaNumberVM)
        {
            bool roomNumberExists = _villaNumberService.CheckVillaNumberExists(villaNumberVM.VillaNumber.Villa_Number);
            if (roomNumberExists) {
               TempData["error"]= "This villa number is already exists";           
            }

            if (ModelState.IsValid && !roomNumberExists)
            {
                _villaNumberService.CreateVillaNumber(villaNumberVM.VillaNumber); 

                TempData["success"] = "The villa number has been created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                IEnumerable<SelectListItem> list = _villaService.GetAllVillas().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
                villaNumberVM.VillaList = list;
                return View(villaNumberVM);
            }
        }
        public IActionResult Update(int villaNumberId)
        {
            VillaNumber? villaNumber = _villaNumberService.GetVillaNumberById(villaNumberId);
            if (villaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }
            else
            {
                IEnumerable<SelectListItem> list = _villaService.GetAllVillas().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
                VillaNumberVM villaNumberVM = new VillaNumberVM { VillaNumber = villaNumber, VillaList = list };
                return View(villaNumberVM);
            }
        }

        [HttpPost]
        public IActionResult Update(VillaNumberVM villaNumberVM)
        {
             

            if (ModelState.IsValid)
            {
                _villaNumberService.UpdateVillaNumber(villaNumberVM.VillaNumber);

                TempData["success"] = "The villa number has been updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {

                IEnumerable<SelectListItem> list = _villaService.GetAllVillas().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
                villaNumberVM.VillaList = list;
                return View(villaNumberVM);
            }
        }

        public IActionResult Delete(int villaNumberId)
        {
            VillaNumber? villaNumber = _villaNumberService.GetVillaNumberById(villaNumberId);
            if (villaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }
            else
            {
                IEnumerable<SelectListItem> list = _villaService.GetAllVillas().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
                VillaNumberVM villaNumberVM = new VillaNumberVM { VillaNumber = villaNumber, VillaList = list };
                return View(villaNumberVM);
            }
        }

        [HttpPost]
        public IActionResult Delete(VillaNumberVM villaNumberVM)
        {
             
            if (_villaNumberService.DeleteVillaNumber(villaNumberVM.VillaNumber.Villa_Number))
            { 
                TempData["success"] = "The villa number has been deleted successfully.";
                return RedirectToAction("Index");
            }
            else
            {

                IEnumerable<SelectListItem> list = _villaService.GetAllVillas().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
                villaNumberVM.VillaList = list;
                return View(villaNumberVM);
            }
        }
    }
}

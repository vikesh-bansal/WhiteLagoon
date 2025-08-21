using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities; 
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IUnitOfWork _unityOfWork;
        public VillaNumberController(IUnitOfWork unitOfWork)
        {
            _unityOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var villaList = _unityOfWork.VillaNumber.GetAll(includeProperties: "Villa");
            return View(villaList);
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> list = _unityOfWork.Villa.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
            VillaNumberVM villaNumber = new VillaNumberVM { VillaList = list };
            return View(villaNumber);
        }
        [HttpPost]
        public IActionResult Create(VillaNumberVM villaNumberVM)
        {
            bool roomNumberExists = _unityOfWork.VillaNumber.GetAll(u => u.Villa_Number == villaNumberVM.VillaNumber.Villa_Number).Any();
            if (roomNumberExists) {
               TempData["error"]= "This villa number is already exists";           
            }

            if (ModelState.IsValid && !roomNumberExists)
            {
                _unityOfWork.VillaNumber.Add(villaNumberVM.VillaNumber);
                _unityOfWork.Save();

                TempData["success"] = "The villa number has been created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                IEnumerable<SelectListItem> list = _unityOfWork.Villa.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                villaNumberVM.VillaList = list;
                return View(villaNumberVM);
            }
        }
        public IActionResult Update(int villaNumberId)
        {
            VillaNumber? villaNumber = _unityOfWork.VillaNumber.Get(x => x.Villa_Number == villaNumberId);
            if (villaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }
            else
            {
                IEnumerable<SelectListItem> list = _unityOfWork.Villa.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                VillaNumberVM villaNumberVM = new VillaNumberVM { VillaNumber = villaNumber, VillaList = list };
                return View(villaNumberVM);
            }
        }

        [HttpPost]
        public IActionResult Update(VillaNumberVM villaNumberVM)
        {
             

            if (ModelState.IsValid)
            {
                _unityOfWork.VillaNumber.Update(villaNumberVM.VillaNumber);
                _unityOfWork.Save();

                TempData["success"] = "The villa number has been updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {

                IEnumerable<SelectListItem> list = _unityOfWork.Villa.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                villaNumberVM.VillaList = list;
                return View(villaNumberVM);
            }
        }

        public IActionResult Delete(int villaNumberId)
        {
            VillaNumber? villaNumber = _unityOfWork.VillaNumber.Get(x => x.Villa_Number == villaNumberId);
            if (villaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }
            else
            {
                IEnumerable<SelectListItem> list = _unityOfWork.Villa.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                VillaNumberVM villaNumberVM = new VillaNumberVM { VillaNumber = villaNumber, VillaList = list };
                return View(villaNumberVM);
            }
        }

        [HttpPost]
        public IActionResult Delete(VillaNumberVM villaNumberVM)
        {

            VillaNumber? _villaNumber = _unityOfWork.VillaNumber.Get(x => x.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);
            if (_villaNumber != null)
            {
                _unityOfWork.VillaNumber.Delete(_villaNumber);
                _unityOfWork.Save();
                TempData["success"] = "The villa number has been deleted successfully.";
                return RedirectToAction("Index");
            }
            else
            {

                IEnumerable<SelectListItem> list = _unityOfWork.Villa.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                villaNumberVM.VillaList = list;
                return View(villaNumberVM);
            }
        }
    }
}

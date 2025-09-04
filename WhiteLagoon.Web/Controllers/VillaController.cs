using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize]
    public class VillaController : Controller
    {
        private IVillaService _villaService;
        public VillaController( IVillaService villaService)
        {
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            return View(_villaService.GetAllVillas());
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if (villa.Name == villa.Description){
                ModelState.AddModelError("name", "The description cannot exactly match the Name.");
            }
            if ((ModelState.IsValid))
            {
              _villaService.CreateVilla(villa);
                TempData["success"] = "The villa has been created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                return View(villa);
            }
        }
        public IActionResult Update (int villaId)
        {
            Villa? villa = _villaService.GetVillaId(villaId);
            if (villa==null)
            {
                return RedirectToAction("Error", "Home");
            }
            else
            {
              return  View(villa);
            }
        }
         
        [HttpPost]
        public IActionResult Update(Villa villa)
        {
             
            if ((ModelState.IsValid))
            {
             _villaService.UpdateVilla(villa);
                TempData["success"] = "The villa has been updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                return View(villa);
            }
        }

        public IActionResult Delete(int villaId)
        {
            Villa? villa = _villaService.GetVillaId(villaId);
            if (villa == null)
            {
                return RedirectToAction("Error", "Home");
            }
            else
            {
                return View(villa);
            }
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
             
            if (_villaService.DeleteVilla(villa.Id))
            { 
                TempData["success"] = "The villa has been deleted successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Failed to delete the villa."; 
            }
            return View(villa);
        }
    }
}

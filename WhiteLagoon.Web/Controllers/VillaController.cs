using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var villaList = _unitOfWork.Villa.GetAll();
            return View(villaList);
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
                if (villa.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\Villa");
                    using(var fileStream=new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                    {
                        villa.Image.CopyTo(fileStream);
                    }
                    villa.ImageUrl = @"\images\VillaImage\" + fileName;
                }
                else
                {
                    villa.ImageUrl = "https://placehold.co/600x400";
                }
                    _unitOfWork.Villa.Add(villa);
                _unitOfWork.Save();

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
            Villa? villa = _unitOfWork.Villa.Get(x => x.Id == villaId);
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
                if (villa.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\Villa");
                    using (var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                    {
                        villa.Image.CopyTo(fileStream);
                    }

                    if (!string.IsNullOrEmpty(villa.ImageUrl))
                    {
                        string oldImageUrl = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImageUrl))
                        {
                            System.IO.File.Delete(oldImageUrl);
                        }
                    }
                    
                    villa.ImageUrl = @"\images\Villa\" + fileName;


                }
                _unitOfWork.Villa.Update(villa);
                _unitOfWork.Save();

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
            Villa? villa = _unitOfWork.Villa.Get(x => x.Id == villaId);
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

            Villa? _villa = _unitOfWork.Villa.Get(x => x.Id == villa.Id);
            if (_villa!=null)
            {
                if (!string.IsNullOrEmpty(_villa.ImageUrl))
                {
                    string oldImageUrl = Path.Combine(_webHostEnvironment.WebRootPath, _villa.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImageUrl))
                    {
                        System.IO.File.Delete(oldImageUrl);
                    }
                }
                _unitOfWork.Villa.Delete(_villa);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been deleted successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                return View(villa);
            }
        }
    }
}

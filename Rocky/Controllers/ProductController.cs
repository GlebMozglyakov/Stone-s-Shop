using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Rocky_DataAccess.Data;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _prodPero;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductRepository prodRepo, IWebHostEnvironment webHostEnvironment)
        {
            _prodPero = prodRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objlist = _prodPero.GetAll(includeProperties : "Category,ApplicationType");

            //foreach (var obj in objlist)
            //{
            //    obj.Category = _db.Category.FirstOrDefault(u => u.Id == obj.CategoryId);
            //    obj.ApplicationType = _db.ApplicationType.FirstOrDefault(u => u.Id == obj.ApplicationTypeId);
            //}

            return View(objlist);
        }


        //GET - UPSERT
        public IActionResult Upsert(int? id)
        {
            //IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()
            //});

            ////ViewBag.CategoryDropDown = CategoryDropDown;
            //ViewData["CategoryDropDown"] = CategoryDropDown;

            //Product product = new Product();

            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _prodPero.GetAllDropdownList(WC.CategoryName),
				ApplicationTypeSelectList = _prodPero.GetAllDropdownList(WC.ApplicationTypeName)
			};
		


            if (id == null)
            {
                //this is for create
                return View(productVM);
            }
            else
            {
                productVM.Product = _prodPero.Find(id.GetValueOrDefault());
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM); 
            }
        }

        //POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var fiels = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if (productVM.Product.Id== 0)
                {
                    //Creating
                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(fiels[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        fiels[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extension;

                    _prodPero.Add(productVM.Product);
                }
                else
                {
                    //Updating
                    var objFromDb = _prodPero.FirstOrDefault(u => u.Id == productVM.Product.Id, isTracking : false);

                    if (fiels.Count > 0)
                    {
                        string upload = webRootPath + WC.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(fiels[0].FileName);

                        var oldFile = Path.Combine(upload, objFromDb.Image);
                        
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            fiels[0].CopyTo(fileStream);
                        }

                        productVM.Product.Image = fileName + extension;
                    }
                    else
                    {
                        productVM.Product.Image = objFromDb.Image;
                    }
                    _prodPero.Update(productVM.Product);
                }

                _prodPero.Save();
                return RedirectToAction("Index");
            }

            productVM.CategorySelectList = _prodPero.GetAllDropdownList(WC.CategoryName);
            productVM.ApplicationTypeSelectList = _prodPero.GetAllDropdownList(WC.ApplicationTypeName);

            return View(productVM);
        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product product = _prodPero.FirstOrDefault(u => u.Id == id, includeProperties : "Category,ApplicationType");
            //product.Category = _db.Category.Find(product.Category.Id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _prodPero.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }

            string upload = _webHostEnvironment.WebRootPath + WC.ImagePath;
            var oldFile = Path.Combine(upload, obj.Image);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }

            _prodPero.Remove(obj);
            _prodPero.Save();
            return RedirectToAction("Index");
        }
    }
}

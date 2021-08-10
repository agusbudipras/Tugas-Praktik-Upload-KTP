using IronOcr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OCR.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OCR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["nik"] = TempData.ContainsKey("nik") ? TempData["nik"] : string.Empty;
            ViewData["nama"] = TempData.ContainsKey("nama") ? TempData["nama"] : string.Empty;
            ViewData["domisili"] = TempData.ContainsKey("domisili") ? TempData["domisili"] : string.Empty;
            ViewData["firstLoad"] = TempData.ContainsKey("firstLoad") ? TempData["firstLoad"] : true;
            return View();
        }

        [HttpPost]
        public ActionResult Upload(IFormFile image)
        {
            try
            {
                if (image != null)
                {

                    //Set Key Name
                    string ImageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

                    //Get url To Save
                    string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", ImageName);
                    string nama = string.Empty;
                    string domisili = string.Empty;
                    string Snik = string.Empty;
                    using (var stream = new FileStream(SavePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }
                    var Result = new IronTesseract().Read(SavePath);
                    // Check jika paragraf ke dua adalah NIK 
                    if (Result.Paragraphs[1].Block.Text.ToUpper().Contains("NIK"))
                    {
                        Snik = string.Join("", Result.Paragraphs[1].Block.Text.ToCharArray().Where(Char.IsDigit));
                        nama = Result.Paragraphs[2].ToString().ToLower().Split("nama")[1];
                        domisili = Result.Paragraphs[5].ToString().ToLower().Split("alamat")[1];
                    }

                    string textFromImage = Result.Text;

                    TempData["nik"] = Snik;
                    TempData["nama"] = nama;
                    TempData["domisili"] = domisili;
                    TempData["firstLoad"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["nik"] = string.Empty;
                TempData["firstLoad"] = false;
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

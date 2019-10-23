using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FaceAnalyzer.Services;
using FaceAnalyzer.Models;
using Microsoft.Extensions.Configuration;

namespace FaceAnalyzer.Controllers
{
    [Route("Camera")]
    public class CameraController : Controller
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration Config;                

        public CameraController(IWebHostEnvironment env, IConfiguration configuration)
        {
            Environment = env;
            Config = configuration;
        }

        [HttpGet]
        [Route("Capture")]
        public IActionResult GetCapture()
        {
            return View("Capture");            
        }

        [HttpPost]
        [Route("Capture")]
        public IActionResult PostCapture(string name)
        {
            try
            {
                var files = HttpContext.Request.Form.Files;
                if (files != null)
                {                    
                    foreach (var file in files)
                    {                        
                        if (file.Length > 0)
                        {                            
                            // Getting Filename
                            var fileName = file.FileName;
                            // Unique filename "Guid"
                            var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                            // Getting Extension
                            var fileExtension = Path.GetExtension(fileName);
                            // Concating filename + fileExtension (unique filename)
                            var newFileName = string.Concat(myUniqueFileName, fileExtension);
                            //  Generating Path to store photo 
                            var filepath = Path.Combine(Environment.WebRootPath, "CameraPhotos") + $@"\{newFileName}";

                            if (!string.IsNullOrEmpty(filepath))
                            {
                                // Storing Image in Folder
                                StoreInFolder(file, filepath);
                                
                                TempData["file"] = newFileName;

                                break;
                            }

                        }
                    }                    
                    return Json(true);  
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpGet]
        [Route("Analysis")]
        public IActionResult Analysis()
        {
            string fileName = TempData["file"].ToString();

            string webFile = "~/CameraPhotos" + $@"/{fileName}";
            string filePath = Path.Combine(Environment.WebRootPath, "CameraPhotos") + $@"\{fileName}";
            
            FaceAttributesViewModel FaceAttributes = null;
            try
            {

                DetectFace detector = new DetectFace(filePath, Config);
                var detectorTask = detector.Run();
                var faceList = detectorTask.Result;

                if (faceList.Count() > 0)
                {
                    FaceAttributes = new FaceAttributesViewModel
                    {
                        Age = faceList[0].FaceAttributes.Age,
                        Gender = faceList[0].FaceAttributes.Gender.ToString().ToLower() == "male" ? "Hombre" : "Mujer",
                        Makeup = faceList[0].FaceAttributes.Makeup.LipMakeup || faceList[0].FaceAttributes.Makeup.EyeMakeup ? "Si" : "No",
                        FacialHair = ((faceList[0].FaceAttributes.FacialHair.Beard > 0) ||
                                 (faceList[0].FaceAttributes.FacialHair.Moustache > 0) ||
                                 (faceList[0].FaceAttributes.FacialHair.Sideburns > 0)) ? "Si" : "No",
                        Glasses = faceList[0].FaceAttributes.Glasses.ToString(),
                        Smile = faceList[0].FaceAttributes.Smile > 0.2 ? "Si" : "No",
                        Image = webFile,
                        Hair = faceList[0].FaceAttributes.Hair.HairColor[0].Color.ToString()
                    };
                }
                else
                {
                    FaceAttributes = new FaceAttributesViewModel
                    {
                        Image = webFile,
                        Age = -1
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
             




            ////Testing data
            //faceAttributes = new FaceAttributesViewModel
            //{
            //    Age = 43,
            //    Gender = "Hombre",
            //    Makeup = "No",
            //    FacialHair = "No",
            //    Glasses = "Si",
            //    Smile = "No",
            //    Hair = "Brown",
            //    Image = webFile
            //};

            return View(FaceAttributes);

        }

        private void StoreInFolder(IFormFile file, string fileName)
        {
            using (FileStream fs = System.IO.File.Create(fileName))
            {
                file.CopyTo(fs);
                fs.Flush();
            }
        }

    }
}

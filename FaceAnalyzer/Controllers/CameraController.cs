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
            
            FaceAttributesViewModel faceAttributes = null;
            IList<FaceAttributesViewModel> listOfFaces = new List<FaceAttributesViewModel>();
            try
            {

                DetectFace detector = new DetectFace(filePath, Config);
                var detectorTask = detector.Run();
                var csFaceList = detectorTask.Result;                

                if (csFaceList.Count() > 0)
                {
                    foreach ( var face in csFaceList)
                    {
                        faceAttributes = new FaceAttributesViewModel
                        {
                            Age = face.FaceAttributes.Age,
                            Gender = face.FaceAttributes.Gender.ToString().ToLower() == "male" ? "Hombre" : "Mujer",
                            Makeup = face.FaceAttributes.Makeup.LipMakeup || face.FaceAttributes.Makeup.EyeMakeup ? "Si" : "No",
                            FacialHair = ((face.FaceAttributes.FacialHair.Beard > 0) ||
                                            (face.FaceAttributes.FacialHair.Moustache > 0) ||
                                            (face.FaceAttributes.FacialHair.Sideburns > 0)) ? "Si" : "No",
                            Glasses = face.FaceAttributes.Glasses.ToString(),
                            Smile = face.FaceAttributes.Smile > 0.2 ? "Si" : "No",
                            Image = webFile,
                            Hair = face.FaceAttributes.Hair.HairColor[0].Color.ToString()
                        };
                        listOfFaces.Add(faceAttributes);
                    }                    
                }
                else
                {
                    faceAttributes = new FaceAttributesViewModel
                    {
                        Image = webFile,
                        Age = -1
                    };
                    listOfFaces.Add(faceAttributes);
                }
            }
            catch (Exception)
            {
                throw;
            }
            
            ////Data for testing the view if you don't want to call CS
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
            //listOfFaces.Add(faceAttributes);

            return View(listOfFaces);

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

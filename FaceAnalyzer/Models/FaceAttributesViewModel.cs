using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FaceAnalyzer.Models
{
    public class FaceAttributesViewModel
    {
        [Display(Name = "Edad")]
        public double? Age { get; set; }

        [Display(Name = "Género")]
        public string Gender { get; set; }

        [Display(Name = "Maquillaje")]
        public string Makeup { get; set; }

        [Display(Name = "Bello facial")]
        public string FacialHair { get; set; }

        [Display(Name = "Anteojos")]
        public string Glasses { get; set; }

        [Display(Name = "Sonrisa")]
        public string Smile { get; set; }

        public string Image { get; set; }
        
        [Display(Name = "Cabello")]
        public string Hair { get; set; }
    }
}

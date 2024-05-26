using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ejemplo.Models.ViewModels
{
    public class CargoVM
    {
        public Cargo oCargo { get; set; }
        public List<SelectListItem> oListaCargo { get; set; }

        public CargoVM()
        {
            oCargo = new Cargo();
            oListaCargo = new List<SelectListItem>();
        }
    }
}
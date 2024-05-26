using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ejemplo.Models;
using ejemplo.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace ejemplo.Controllers
{
    public class ejemploController : Controller
    {
        private readonly EJEMPLOContext _DBContext;

        public ejemploController(EJEMPLOContext context)
        {
            _DBContext = context;
        }

        public IActionResult GestionEmpleado()
        {
            List<Empleado> lista = _DBContext.Empleados.Include(c => c.oCargo).ToList();
            return View(lista);
        }
        /*** Acción para mostrar la vista de detalle de empleado en modo de lectura o edición.
         * @param idEmpleado: ID del empleado.
         * @returns Vista de detalle de empleado.
        /****/

        [HttpGet]
        public IActionResult Empleado_Detalle(int idEmpleado)
        {
            EmpleadoVM oEmpleadoVM = new EmpleadoVM()
            {
                oEmpleado = new Empleado(),
                oCargo = _DBContext.Cargos.Select(cargo => new SelectListItem()
                {
                    Text = cargo.Descripcion,
                    Value = cargo.IdCargo.ToString()

                }).ToList()
            };

            if (idEmpleado != 0)
            {
                oEmpleadoVM.oEmpleado = _DBContext.Empleados.Find(idEmpleado);
            }

            return View(oEmpleadoVM);
        }
        
        
        /*** Acción para guardar los cambios realizados en el detalle de empleado.
        * @param oEmpleadoVM: Modelo de vista con los datos del empleado.
        * @returns Redirección a la acción de gestión de empleados.
       /****/


        [HttpPost]
        public IActionResult Empleado_Detalle(EmpleadoVM oEmpleadoVM)
        {
            if (oEmpleadoVM.oEmpleado.IdEmpleado == 0)
            {
                _DBContext.Empleados.Add(oEmpleadoVM.oEmpleado);
            }
            else
            {
                _DBContext.Empleados.Update(oEmpleadoVM.oEmpleado);
            }

            _DBContext.SaveChanges();

            return RedirectToAction("GestionEmpleado");
        }

        /*** Acción para mostrar la vista de confirmación de eliminación de empleado.
        * @param idEmpleado: ID del empleado a eliminar.
        * @returns Vista de confirmación de eliminación de empleado.
       /****/
        [HttpGet]
        public IActionResult EliminarEmpleado(int idEmpleado)
        {
            Empleado oEmpleado = _DBContext.Empleados.Include(c => c.oCargo).FirstOrDefault(e => e.IdEmpleado == idEmpleado);
            return View(oEmpleado);
        }

        /*** Acción para eliminar un empleado.
        * @param oEmpleado: Empleado a eliminar.
        * @returns Redirección a la acción de gestión de empleados.
       /****/
        [HttpPost]
        public IActionResult Eliminar(Empleado oEmpleado)
        {
            _DBContext.Empleados.Remove(oEmpleado);
            _DBContext.SaveChanges();

            return RedirectToAction("GestionEmpleado");
        }





        public IActionResult GestionCargo()
        {
            List<Cargo> lista = _DBContext.Cargos.ToList();
            return View(lista);
        }

        /** Muestra la vista de detalle de cargo en modo de lectura o edición 
          * @param idCargo: ID del cargo 
          * @returns: Vista de detalle de cargo **/
        [HttpGet]
        public IActionResult Cargo_Detalle(int idCargo)
        {
            CargoVM oCargoVM = new CargoVM()
            {
                oCargo = new Cargo()
            };

            if (idCargo != 0)
            {
                oCargoVM.oCargo = _DBContext.Cargos.Find(idCargo);
            }

            return View(oCargoVM);
        }
        /** Guarda los cambios realizados en el detalle de cargo 
          * @param oCargoVM: Modelo de vista con los datos del cargo 
          * @returns: Redirección a la acción de gestión de cargos **/
        [HttpPost]
        public IActionResult Cargo_Detalle(CargoVM oCargoVM)
        {
            if (ModelState.IsValid)
            {
                if (oCargoVM.oCargo.IdCargo == 0)
                {
                    _DBContext.Cargos.Add(oCargoVM.oCargo);
                }
                else
                {
                    _DBContext.Cargos.Update(oCargoVM.oCargo);
                }

                _DBContext.SaveChanges();

                return RedirectToAction("GestionCargo");
            }

            return View(oCargoVM);
        }

        /** Muestra la vista de confirmación de eliminación de cargo 
          * @param idCargo: ID del cargo a eliminar 
          * @returns: Vista de confirmación de eliminación de cargo **/

        [HttpGet]
        public IActionResult EliminarCargo(int idCargo)
        {
            Cargo oCargo = _DBContext.Cargos.Include(c => c.Empleados).FirstOrDefault(cargo => cargo.IdCargo == idCargo);
            return View(oCargo);
        }

        /** Acción para eliminar un cargo.
       * @param oEmpleado: E a eliminar.
       * @returns Redirección a la acción de gestión de empleados.
        **/

        [HttpPost]
        public IActionResult EliminarCargo(Cargo oCargo)
        {
            // Eliminar los empleados relacionados en la tabla 'EMPLEADO'
            var empleadosRelacionados = _DBContext.Empleados.Where(e => e.IdCargo == oCargo.IdCargo);
            _DBContext.Empleados.RemoveRange(empleadosRelacionados);

            // Eliminar el cargo en la tabla 'CARGO'
            _DBContext.Cargos.Remove(oCargo);

            _DBContext.SaveChanges();

            return RedirectToAction("GestionCargo");
        }

        public List<Empleado> ListaEmpleados()
        {
            List<Empleado> Lista =_DBContext.Empleados.Include(c => c.oCargo).ToList();
            return Lista;
        }

        public ActionResult GenerarPDF()
        {
            List<Empleado> empleados = ListaEmpleados();
            var document = new iTextSharp.text.Document(PageSize.A4, 50, 50, 25, 25);
            using (var memoryStream = new MemoryStream())
            {
                var writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                var titulo = new Paragraph("Lista de empleados", new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD));
                titulo.Alignment = Element.ALIGN_CENTER;
                document.Add(titulo);

                var espacio = new Paragraph("");
                document.Add(espacio);

                var tabla = new PdfPTable(4);

                tabla.AddCell("Nombre");
                tabla.AddCell("Teléfono");
                tabla.AddCell("Correo");
                tabla.AddCell("Cargo");

                foreach (var empleado in empleados)
                {
                    tabla.AddCell(empleado.NombreCompleto);
                    tabla.AddCell(empleado.Telefono);
                    tabla.AddCell(empleado.Correo);
                    tabla.AddCell(empleado.oCargo.Descripcion);
                }
                document.Add(tabla);

                document.Close();

                var fileContents = memoryStream.ToArray();
                return File(fileContents, "application/pdf", "empleados.pdf");
            }
        }
    }
}
    
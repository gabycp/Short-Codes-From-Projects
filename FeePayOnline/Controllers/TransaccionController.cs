using EaglePagoLinea.Helpers;
using EaglePagoLinea.Models;
using EaglePagoLinea.Models.DB;
using EaglePagoLinea.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PixelPaySDK.Base;
using PixelPaySDK.Entities;
using PixelPaySDK.Models;
using System.Diagnostics.Contracts;
using System.Text;

namespace EaglePagoLinea.Controllers
{
    public class TransaccionController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly HttpClient httpClient;
        private string url = "";

        public TransaccionController(ApplicationDbContext context, 
                                        HttpClient httpClient)
        {
            this.context = context;
            this.httpClient = httpClient;
        }

        public async Task<ActionResult> Index(string codigo, float montototal)
        {           
            
            var aviso = JsonConvert.DeserializeObject<Avisos>(HttpContext.Session.GetString("avisoActual"));

            if(codigo != aviso.Codigo || montototal != aviso.Valor) 
            {
                return BadRequest();
            }

            ViewBag.monto = montototal;
            ViewBag.codigo = codigo;
            ViewBag.mensualidad = aviso;

            CardModel card = new CardModel();
            return View(card);
        }


        public async Task<ActionResult> ComprobantePago(int IdPT)
        {
            Models.TransaccionPagoPP tp = new Models.TransaccionPagoPP();

            tp = context.PagosTemporales.Where(x => x.IdPagoTemporal == IdPT)
                                        .Select(x => new Models.TransaccionPagoPP
                                        {
                                            IdPagoTemporalPP = x.IdPagoTemporal,    
                                            PaymentUuid = x.PaymentUuid,
                                            Monto = x.Monto,
                                            NumeroDocumento = x.NumeroDocumento,
                                            PagadoPor = x.PagadoPor
                                            
                                        }).First();

            tp.TransactionId = await context.TransaccionPagosPP.Where(x => x.PaymentUuid == tp.PaymentUuid)
                                                        .Select(x => x.TransactionId)
                                                        .FirstOrDefaultAsync();

            tp.FechaPago = await context.TransaccionPagosPP.Where(x => x.PaymentUuid == tp.PaymentUuid)
                                                        .Select(x => x.FechaCreacion)
                                                        .FirstOrDefaultAsync();

            string avisoCadena = await context.TransaccionPagoAvisos.Where(x => x.TransactionId == tp.PaymentUuid)
                                                        .Select(x => x.MensajeRepuesta)
                                                        .FirstOrDefaultAsync();

            var aviso = JsonConvert.DeserializeObject<Avisos>(avisoCadena);
            var dni = HttpContext.Session.GetString("dni");

            ViewBag.CodigoMensualidad = "";
            ViewBag.dni = dni;

            if (aviso != null) 
            {
                tp.NombreEstudiante = aviso.Cliente;
                ViewBag.CodigoMensualidad = aviso.Periodo;
            }

            return View(tp);
        }



        bool IsJson(string str)
        {
            try
            {
                // Comprobacion de tipo de dato al deserializar
                JsonConvert.DeserializeObject(str);
                return true; 
            }
            catch (JsonException)
            {
                return false; 
            }
        }

    }
}

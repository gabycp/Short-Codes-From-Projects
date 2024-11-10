using AutoMapper;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Helpers;
using PeliculasAPI.Servicios;
using System.Linq.Dynamic.Core;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/peliculas")]
    public class PeliculasController: CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly ILogger logger;
        private readonly string contenedor = "peliculas";

        public PeliculasController(ApplicationDbContext context,
                                    IMapper mapper,
                                    IAlmacenadorArchivos almacenadorArchivos,
                                    ILogger<PeliculasController> logger)
            :base(context,mapper)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PeliculasIndexDTO>> Get() 
        {
            var top = 5;
            var hoy = DateTime.Now;

            var proximosEstrenos = await context.Peliculas
                .Where( x=> x.FechaEstreno >  hoy )
                .OrderBy( x => x.FechaEstreno )
                .Take(top)
                .ToListAsync();

            var enCines = await context.Peliculas
                .Where(x => x.EnCines)
                .Take(top)
                .ToListAsync();

            var resultado = new PeliculasIndexDTO();

            resultado.FuturosEstrenos = mapper.Map<List<PeliculaDTO>>(proximosEstrenos);
            resultado.EnCines = mapper.Map<List<PeliculaDTO>>(enCines);

            return resultado;

        }

        [HttpGet("filtro")]
        public async Task<ActionResult<List<PeliculaDTO>>> Filtrar([FromQuery] FiltroPeliculasDTO filtroPeliculasDTO) 
        {
            var peliculasQuerably = context.Peliculas.AsQueryable();

            if (!string.IsNullOrEmpty(filtroPeliculasDTO.Titulo)) 
            {
                peliculasQuerably = peliculasQuerably.Where(x => x.Titulo.Contains(filtroPeliculasDTO.Titulo)); 
            }

            if (filtroPeliculasDTO.EnCines) 
            {
                peliculasQuerably = peliculasQuerably.Where(x => x.EnCines);
            }

            if (filtroPeliculasDTO.ProximosEstrenos) 
            {
                var hoy = DateTime.Now;
                peliculasQuerably = peliculasQuerably.Where(x => x.FechaEstreno > hoy);
            }

            if(filtroPeliculasDTO.GeneroId != 0) 
            {
                peliculasQuerably = peliculasQuerably
                    .Where(x => x.peliculasGeneros.Select(y => y.GeneroId)
                    .Contains(filtroPeliculasDTO.GeneroId));
            }

            if(!string.IsNullOrEmpty(filtroPeliculasDTO.CampoOrdenar))
            {
                var tipoOrden = filtroPeliculasDTO.OrdenAscendente ? "ascending" : "descending";

                try
                {
                    peliculasQuerably = peliculasQuerably.OrderBy($"{filtroPeliculasDTO.CampoOrdenar} {tipoOrden}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message, ex);
                }

                
            }

            await HttpContext.InsertarParametrosPaginacion(peliculasQuerably, filtroPeliculasDTO.CantidadRegistroPorPagina);

            var peliculas = await peliculasQuerably.Paginar(filtroPeliculasDTO.paginacionDTO).ToListAsync();

            return mapper.Map<List<PeliculaDTO>>(peliculas);
        }

        [HttpGet("{id}", Name = "obtenerPelicula")]
        public async Task<ActionResult<PeliculasDetalleDTO>> Get(int id) 
        {
            var pelicula = await context.Peliculas
                .Include( x=> x.peliculasActores).ThenInclude( x=> x.Actor)
                .Include(x => x.peliculasGeneros).ThenInclude(x => x.Genero)
                .FirstOrDefaultAsync(x => x.Id == id);

            if(pelicula == null) return NotFound();

            pelicula.peliculasActores = pelicula.peliculasActores.OrderBy(x => x.Orden).ToList();

            return mapper.Map<PeliculasDetalleDTO>(pelicula);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm]PeliculasCreacionDTO peliculasCreacionDTO) 
        {
            var pelicula = mapper.Map<Peliculas>(peliculasCreacionDTO);


            if (peliculasCreacionDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await peliculasCreacionDTO.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(peliculasCreacionDTO.Poster.FileName);
                    pelicula.Poster = await almacenadorArchivos.GuardarArchivo(contenido, extension, contenedor,
                                           peliculasCreacionDTO.Poster.ContentType);
                }
            }

            AsignarOrdenActores(pelicula);
            context.Add(pelicula);
            await context.SaveChangesAsync();

            var peliculaDTO = mapper.Map<PeliculaDTO>(pelicula);
            return new CreatedAtRouteResult("obtenerPelicula", new { id = pelicula.Id }, peliculaDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] PeliculasCreacionDTO peliculasCreacionDTO) 
        {
            var peliculaDB = await context.Peliculas
                .Include( x => x.peliculasGeneros)
                .Include( x=> x.peliculasActores)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (peliculaDB == null)
            {
                return NotFound();
            }

            peliculaDB = mapper.Map(peliculasCreacionDTO, peliculaDB);

            if (peliculasCreacionDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await peliculasCreacionDTO.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(peliculasCreacionDTO.Poster.FileName);
                    peliculaDB.Poster = await almacenadorArchivos.EditarArchivo(contenido, extension, contenedor,
                                            peliculaDB.Poster,
                                           peliculasCreacionDTO.Poster.ContentType);
                }
            }

            AsignarOrdenActores(peliculaDB);
            await context.SaveChangesAsync();
            return NoContent();
        }

        private void AsignarOrdenActores(Peliculas peliculas) 
        {
            if(peliculas.peliculasActores != null) 
            {
                for (int i = 0; i < peliculas.peliculasActores.Count; i++) 
                {
                    peliculas.peliculasActores[i].Orden = i;
                }
            }
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<PeliculaPatchDTO> patchDocument) 
        {
            return await Patch<Peliculas, PeliculaPatchDTO>(id, patchDocument);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id) 
        {
            return await Delete<Actor>(id);
        }

    }
}

﻿using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.Entidades
{
    public class Peliculas:IId
    {
        public int Id { get; set; }
        [Required]
        [StringLength(300)]
        public string Titulo { get; set; }
        public bool EnCines { get; set; }
        public DateTime FechaEstreno { get; set; }
        public string Poster { get; set; }

        public List<PeliculasActores> peliculasActores { get; set; }
        public List<PeliculasGeneros> peliculasGeneros { get; set; }
        public List<PeliculasSalaDeCines> peliculasSalaDeCines { get; set; }
    }
}

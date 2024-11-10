namespace PeliculasAPI.Entidades
{
    public class PeliculasGeneros
    {
        public int GeneroId { get; set; }
        public int PeliculaId { get; set; }
        public Genero Genero { get; set; }
        public Peliculas Pelicula { get; set; }
    }
}

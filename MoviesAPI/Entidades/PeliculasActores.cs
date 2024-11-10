namespace PeliculasAPI.Entidades
{
    public class PeliculasActores
    {
        public int PeliculaId { get; set; }
        public int ActorId { get; set; }
        public string Personaje { get; set; }
        public int Orden { get; set; }
        public Peliculas Pelicula { get; set; }
        public Actor Actor { get; set; }
    }
}

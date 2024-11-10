using EaglePagoLinea.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace EaglePagoLinea.Helpers
{
    public class DatabaseConnection
    {
        private readonly ApplicationDbContext context;

        public DatabaseConnection(ApplicationDbContext context)
        {
            this.context = context;
        }

        public bool CheckConnection()
        {
            try
            {
                // Devuelve true si la base de datos está accesible
                return context.Database.CanConnect();
            }
            catch (Exception ex)
            {
                // Captura cualquier excepción relacionada con la conexión
                Console.WriteLine($"Error al conectar a la base de datos: {ex.Message}");
                return false;
            }
        }
    }
}

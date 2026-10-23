using System.Security.Claims;

namespace ApiTransporteLweb.Helpers
{
    public static class ReglasFecha
    {
        private const string ROL_ADMIN = "Admin";

        // Fecha de hoy en hora de Lima, sin depender de la zona horaria del servidor
        public static DateTime HoyLima()
        {
            TimeZoneInfo zona;
            try
            {
                zona = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"); // Windows
            }
            catch (TimeZoneNotFoundException)
            {
                zona = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");              // Linux
            }
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, zona).Date;
        }

        // Admin: siempre. Supervisor y operador: solo si la fecha es hoy o ayer.
        public static bool PuedeModificar(ClaimsPrincipal user, DateTime? fecha)
        {
            if (user.IsInRole(ROL_ADMIN))
                return true;

            if (fecha == null)
                return false;

            var hoy = HoyLima();
            var f = fecha.Value.Date;
            return f == hoy || f == hoy.AddDays(-1);
        }
    }
}
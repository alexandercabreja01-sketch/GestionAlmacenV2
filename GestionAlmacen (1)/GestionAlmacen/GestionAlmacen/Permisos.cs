using System;

namespace GestionAlmacen
{
    public static class Permisos
    {
        public static void ExigirAdministrador()
        {
            if (!Sesion.EsAdministrador)
                throw new UnauthorizedAccessException("Solo el administrador puede realizar esta acción.");
        }

        public static void ExigirCajero()
        {
            if (Sesion.Rol != "Cajero")
                throw new UnauthorizedAccessException("Solo un cajero puede realizar ventas.");
        }
    }
}

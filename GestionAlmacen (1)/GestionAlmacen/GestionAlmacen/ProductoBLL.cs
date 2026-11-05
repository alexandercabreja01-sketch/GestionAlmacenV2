using System;
using System.Data;

namespace GestionAlmacen
{
    public class ProductoBLL
    {
        private readonly ProductoDAL dal = new ProductoDAL();

        private void ExigirAdministrador()
        {
            if (!Sesion.EsAdministrador)
                throw new UnauthorizedAccessException(
                    "No tiene permisos para modificar productos.");
        }

        private void Validar(Producto p)
        {
            if (string.IsNullOrWhiteSpace(p.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(p.Marca))
                throw new ArgumentException("La marca es obligatoria.");
            if (p.Precio < 0)
                throw new ArgumentException("El precio no puede ser negativo.");
            if (p.Stock < 0)
                throw new ArgumentException("El stock no puede ser negativo.");
        }

        public int Insertar(Producto p)
        {
            ExigirAdministrador();
            Validar(p);
            return dal.Insertar(p);
        }

        public bool Actualizar(Producto p)
        {
            ExigirAdministrador();
            if (p.Id <= 0) throw new ArgumentException("Seleccione un producto.");
            Validar(p);
            return dal.Actualizar(p);
        }

        public bool Eliminar(int id)
        {
            ExigirAdministrador();
            if (id <= 0) throw new ArgumentException("Seleccione un producto.");
            return dal.Eliminar(id);
        }

        public DataTable Buscar(int? codigo, string nombre, string marca)
        {
            return dal.Buscar(codigo, nombre, marca);
        }

        public DataTable Listar()
        {
            return dal.Buscar(null, null, null);
        }
    }
}

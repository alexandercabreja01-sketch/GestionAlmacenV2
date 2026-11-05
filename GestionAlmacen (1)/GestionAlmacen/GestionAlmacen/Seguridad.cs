using System.Security.Cryptography;
using System.Text;

namespace GestionAlmacen
{
    public static class Seguridad
    {
        public static string HashSha256(string texto)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }
    }
}

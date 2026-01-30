using System;
using System.Data;
using System.Data.SqlClient;

namespace RcursosHumanoss.DALRRHH
{
    internal static class ConexionDB
    {
        private static readonly string _connectionString =
            "Server=WIKILIXS\\SQLEXPRESS;Database=RecursosHumanos;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection ObtenerConexion()
        {
            var conexion = new SqlConnection(_connectionString);
            conexion.Open();
            return conexion;
        }

        public static SqlCommand CrearComando(string sql, CommandType tipo = CommandType.Text)
        {
            var conexion = ObtenerConexion();
            return new SqlCommand(sql, conexion) { CommandType = tipo };
        }

        public static bool ProbarConexion(out string mensaje)
        {
            try
            {
                using (var conexion = new SqlConnection(_connectionString))
                {
                    conexion.Open();
                    mensaje = "Conexión exitosa con SQL Server.";
                    return true;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error de conexión: " + ex.Message;
                return false;
            }
        }
    }
}

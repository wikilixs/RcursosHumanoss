using System.Data;
using System.Data.SqlClient;

namespace RcursosHumanoss.DALRRHH
{
    internal static class CatalogoDAL
    {
        public static DataTable ListarDepartamentos()
        {
            const string sql = @"SELECT IdDepartamento, Nombre FROM Departamento ORDER BY Nombre;";
            var dt = new DataTable();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }

            return dt;
        }

        public static DataTable ListarCargos()
        {
            const string sql = @"SELECT IdCargo, Nombre FROM Cargo ORDER BY Nombre;";
            var dt = new DataTable();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }

            return dt;
        }
    }
}

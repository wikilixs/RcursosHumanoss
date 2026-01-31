using System;
using System.Data;
using System.Data.SqlClient;

namespace RcursosHumanoss.DALRRHH
{
    internal static class VacacionInfoDAL
    {
        public static DataTable ObtenerMisVacaciones(int idEmpleado)
        {
            const string sql = @"
SELECT
    v.IdVacaciones,
    v.FechaInicio,
    v.FechaFin,
    DATEDIFF(DAY, v.FechaInicio, v.FechaFin) + 1 AS Dias
FROM Vacaciones v
WHERE v.IdEmpleado = @IdEmpleado
ORDER BY v.IdVacaciones DESC;";

            var dt = new DataTable();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@IdEmpleado", SqlDbType.Int).Value = idEmpleado;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }
    }
}

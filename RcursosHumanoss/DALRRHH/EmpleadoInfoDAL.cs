using System;
using System.Data;
using System.Data.SqlClient;

namespace RcursosHumanoss.DALRRHH
{
    internal static class EmpleadoInfoDAL
    {
        public static DataTable ObtenerMiInformacion(int idEmpleado)
        {
            const string sql = @"
SELECT
    e.CI,
    e.Nombres,
    e.PrimerApellido,
    ISNULL(e.SegundoApellido,'') AS SegundoApellido,
    e.FechaNacimiento,
    e.Telefono,
    d.Nombre AS Departamento,
    c.Nombre AS Cargo,
    ISNULL(est.Nombre, 'Sin Estado') AS EstadoActual
FROM Empleado e
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
OUTER APPLY (
    SELECT TOP 1 es.Nombre
    FROM EstadoHistorial eh
    INNER JOIN Estado es ON es.IdEstado = eh.IdEstado
    WHERE eh.IdEmpleado = e.IdEmpleado
    ORDER BY eh.IdHistorial DESC
) est
WHERE e.IdEmpleado = @IdEmpleado;";

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

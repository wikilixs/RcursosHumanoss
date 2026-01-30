using System;
using System.Data;
using System.Data.SqlClient;
using RcursosHumanoss.EntidadesRRHH;

namespace RcursosHumanoss.DALRRHH
{
    internal static class EmpleadoInfoDAL
    {
        // =========================================================
        //  DataTable (ideal para DataGridView directo)
        // =========================================================
        public static DataTable ObtenerMiInformacion(int idEmpleado)
        {
            const string sql = @"
SELECT
    e.IdEmpleado,
    e.Nombres,
    e.PrimerApellido,
    ISNULL(e.SegundoApellido, '') AS SegundoApellido,
    e.CI,
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
    ORDER BY eh.FechaRegitro DESC, eh.IdHistorial DESC
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

        // =========================================================
        //  Opcional: devuelve EntidadEmpleado (más limpio para lógica)
        // =========================================================
        public static EntidadEmpleado? ObtenerMiInformacionEntidad(int idEmpleado)
        {
            const string sql = @"
SELECT
    e.IdEmpleado,
    e.Nombres,
    e.PrimerApellido,
    ISNULL(e.SegundoApellido, '') AS SegundoApellido,
    e.CI,
    e.FechaNacimiento,
    e.Telefono,
    e.IdDepartamento,
    e.IdCargo,
    d.Nombre AS DepartamentoNombre,
    c.Nombre AS CargoNombre,
    ISNULL(est.Nombre, 'Sin Estado') AS EstadoActual
FROM Empleado e
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
OUTER APPLY (
    SELECT TOP 1 es.Nombre
    FROM EstadoHistorial eh
    INNER JOIN Estado es ON es.IdEstado = eh.IdEstado
    WHERE eh.IdEmpleado = e.IdEmpleado
    ORDER BY eh.FechaRegitro DESC, eh.IdHistorial DESC
) est
WHERE e.IdEmpleado = @IdEmpleado;";

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@IdEmpleado", SqlDbType.Int).Value = idEmpleado;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;

                    return new EntidadEmpleado
                    {
                        IdEmpleado = Convert.ToInt32(rd["IdEmpleado"]),
                        Nombres = rd["Nombres"].ToString(),
                        PrimerApellido = rd["PrimerApellido"].ToString(),
                        SegundoApellido = rd["SegundoApellido"].ToString(),
                        CI = rd["CI"].ToString(),
                        FechaNacimiento = Convert.ToDateTime(rd["FechaNacimiento"]),
                        Telefono = rd["Telefono"].ToString(),
                        IdDepartamento = Convert.ToInt32(rd["IdDepartamento"]),
                        IdCargo = Convert.ToInt32(rd["IdCargo"]),
                        DepartamentoNombre = rd["DepartamentoNombre"].ToString(),
                        CargoNombre = rd["CargoNombre"].ToString(),
                        EstadoActual = rd["EstadoActual"].ToString()
                    };
                }
            }
        }
    }
}

using System;
using System.Data;
using System.Data.SqlClient;

namespace RcursosHumanoss.DALRRHH
{
    internal static class EliminarEmpleadoDAL
    {
        // ✅ Según tu BD
        public const int ESTADO_ACTIVO = 5;
        public const int ESTADO_INACTIVO = 6;

        // =========================================================
        // LISTAR ÚLTIMOS POR ESTADO
        // =========================================================
        public static DataTable ListarUltimos(int top, int idEstado)
        {
            const string sql = @"
SELECT TOP (@Top)
    e.IdEmpleado,
    e.CI,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    e.Telefono,
    d.Nombre AS Departamento,
    c.Nombre AS Cargo,
    est.Nombre AS Estado
FROM Empleado e
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
OUTER APPLY (
    SELECT TOP 1 eh.IdEstado
    FROM EstadoHistorial eh
    WHERE eh.IdEmpleado = e.IdEmpleado
    ORDER BY eh.FechaRegitro DESC, eh.IdHistorial DESC
) ult
LEFT JOIN Estado est ON est.IdEstado = ISNULL(ult.IdEstado, @EstadoActivo)
WHERE ISNULL(ult.IdEstado, @EstadoActivo) = @IdEstado
ORDER BY e.IdEmpleado DESC;";

            DataTable dt = new DataTable();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
                cmd.Parameters.Add("@IdEstado", SqlDbType.Int).Value = idEstado;

                // ✅ default activo = 5
                cmd.Parameters.Add("@EstadoActivo", SqlDbType.Int).Value = ESTADO_ACTIVO;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            return dt;
        }

        // =========================================================
        // BUSCAR POR CAMPO + ESTADO
        // campo: "CI" / "Nombres" / "Apellidos"
        // =========================================================
        public static DataTable Buscar(string campo, string valor, int idEstado)
        {
            campo = (campo ?? "").Trim();
            valor = (valor ?? "").Trim();

            if (string.IsNullOrWhiteSpace(campo))
                campo = "CI";

            string where;

            if (campo.Equals("CI", StringComparison.OrdinalIgnoreCase))
            {
                // ✅ flexible (soporta espacios/guiones y letras tipo "LP")
                where = @"
REPLACE(REPLACE(e.CI,'-',''),' ','') LIKE '%' + REPLACE(REPLACE(@Valor,'-',''),' ','') + '%'";
            }
            else if (campo.Equals("Nombres", StringComparison.OrdinalIgnoreCase))
            {
                where = "e.Nombres LIKE '%' + @Valor + '%'";
            }
            else
            {
                where = "(e.PrimerApellido LIKE '%' + @Valor + '%' OR ISNULL(e.SegundoApellido,'') LIKE '%' + @Valor + '%')";
            }

            string sql = $@"
SELECT
    e.IdEmpleado,
    e.CI,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    e.Telefono,
    d.Nombre AS Departamento,
    c.Nombre AS Cargo,
    est.Nombre AS Estado
FROM Empleado e
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
OUTER APPLY (
    SELECT TOP 1 eh.IdEstado
    FROM EstadoHistorial eh
    WHERE eh.IdEmpleado = e.IdEmpleado
    ORDER BY eh.FechaRegitro DESC, eh.IdHistorial DESC
) ult
LEFT JOIN Estado est ON est.IdEstado = ISNULL(ult.IdEstado, @EstadoActivo)
WHERE ISNULL(ult.IdEstado, @EstadoActivo) = @IdEstado
  AND {where}
ORDER BY e.IdEmpleado DESC;";

            DataTable dt = new DataTable();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@IdEstado", SqlDbType.Int).Value = idEstado;
                cmd.Parameters.Add("@Valor", SqlDbType.NVarChar, 200).Value = valor;

                // ✅ default activo = 5
                cmd.Parameters.Add("@EstadoActivo", SqlDbType.Int).Value = ESTADO_ACTIVO;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            return dt;
        }

        // =========================================================
        // INACTIVAR (insertar historial con IdEstado = 6)
        // =========================================================
        public static bool Inactivar(int idEmpleado)
        {
            const string sql = @"
INSERT INTO EstadoHistorial (IdEmpleado, IdEstado, FechaRegitro)
VALUES (@IdEmpleado, @IdEstado, GETDATE());";

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@IdEmpleado", SqlDbType.Int).Value = idEmpleado;
                cmd.Parameters.Add("@IdEstado", SqlDbType.Int).Value = ESTADO_INACTIVO;

                int filas = cmd.ExecuteNonQuery();
                return filas > 0;
            }
        }
    }
}

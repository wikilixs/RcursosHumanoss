using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RcursosHumanoss.EntidadesRRHH;

namespace RcursosHumanoss.DALRRHH
{
    internal static class GerenteEmpleadoDAL
    {
        // =========================================================
        //  LISTAR ÚLTIMOS (con filtro opcional por estado)
        //  estado: "Activo" / "Inactivo" / "Todos"
        // =========================================================
        public static List<EntidadEmpleado> ListarUltimos(int top, string estado = "Todos")
        {
            int? idEstadoFiltro = NormalizarEstado(estado); // 1/2/null

            string whereEstado = idEstadoFiltro.HasValue
                ? "WHERE ISNULL(est.IdEstado, 1) = @IdEstado"
                : "";

            string sql = $@"
SELECT TOP (@Top)
    e.IdEmpleado,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    e.CI,
    e.FechaNacimiento,
    e.Telefono,
    e.IdDepartamento,
    d.Nombre AS DepartamentoNombre,
    e.IdCargo,
    c.Nombre AS CargoNombre,
    ISNULL(est.Nombre, 'Sin Estado') AS EstadoActual,
    u.Email
FROM Empleado e
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
LEFT JOIN Usuario u ON u.IdEmpleado = e.IdEmpleado
OUTER APPLY (
    SELECT TOP 1 es.IdEstado, es.Nombre
    FROM EstadoHistorial eh
    INNER JOIN Estado es ON es.IdEstado = eh.IdEstado
    WHERE eh.IdEmpleado = e.IdEmpleado
    ORDER BY eh.IdHistorial DESC
) est
{whereEstado}
ORDER BY e.IdEmpleado DESC;";

            var lista = new List<EntidadEmpleado>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;

                if (idEstadoFiltro.HasValue)
                    cmd.Parameters.Add("@IdEstado", SqlDbType.Int).Value = idEstadoFiltro.Value;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapEmpleado(rd));
                }
            }

            return lista;
        }

        // =========================================================
        //  BUSCAR (Gerente) en TODO el sistema
        //  criterio: "CI" / "Nombres" / "Apellidos" / "TODOS"
        //  estado: "Activo" / "Inactivo" / "Todos"
        // =========================================================
        public static List<EntidadEmpleado> Buscar(string criterio, string valor, string estado = "Todos")
        {
            criterio = (criterio ?? "").Trim().ToUpperInvariant();
            valor = (valor ?? "").Trim();

            // Limpieza útil (CI con espacios o guiones)
            valor = valor.Replace(" ", "").Replace("-", "");

            int? idEstadoFiltro = NormalizarEstado(estado); // 1/2/null

            bool sinFiltroTexto = (criterio == "TODOS" || criterio == "") && string.IsNullOrWhiteSpace(valor);

            string whereCriterio;
            if (criterio == "CI")
                whereCriterio = "REPLACE(REPLACE(e.CI,'-',''),' ','') LIKE @Valor";
            else if (criterio == "NOMBRE" || criterio == "NOMBRES")
                whereCriterio = "e.Nombres LIKE @Valor";
            else if (criterio == "APELLIDOS" || criterio == "APELLIDO")
                whereCriterio = "(e.PrimerApellido LIKE @Valor OR e.SegundoApellido LIKE @Valor)";
            else
                whereCriterio = "(REPLACE(REPLACE(e.CI,'-',''),' ','') LIKE @Valor OR e.Nombres LIKE @Valor OR e.PrimerApellido LIKE @Valor OR e.SegundoApellido LIKE @Valor)";

            string whereFinal = sinFiltroTexto ? "1=1" : whereCriterio;

            string whereEstado = idEstadoFiltro.HasValue
                ? "AND ISNULL(est.IdEstado, 1) = @IdEstado"
                : "";

            string sql = $@"
SELECT
    e.IdEmpleado,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    e.CI,
    e.FechaNacimiento,
    e.Telefono,
    e.IdDepartamento,
    d.Nombre AS DepartamentoNombre,
    e.IdCargo,
    c.Nombre AS CargoNombre,
    ISNULL(est.Nombre, 'Sin Estado') AS EstadoActual,
    u.Email
FROM Empleado e
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
LEFT JOIN Usuario u ON u.IdEmpleado = e.IdEmpleado
OUTER APPLY (
    SELECT TOP 1 es.IdEstado, es.Nombre
    FROM EstadoHistorial eh
    INNER JOIN Estado es ON es.IdEstado = eh.IdEstado
    WHERE eh.IdEmpleado = e.IdEmpleado
    ORDER BY eh.IdHistorial DESC
) est
WHERE {whereFinal}
{whereEstado}
ORDER BY e.IdEmpleado DESC;";

            var lista = new List<EntidadEmpleado>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                if (!sinFiltroTexto)
                    cmd.Parameters.Add("@Valor", SqlDbType.NVarChar, 200).Value = "%" + valor + "%";

                if (idEstadoFiltro.HasValue)
                    cmd.Parameters.Add("@IdEstado", SqlDbType.Int).Value = idEstadoFiltro.Value;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapEmpleado(rd));
                }
            }

            return lista;
        }

        // =========================================================
        //  Helpers
        // =========================================================
        private static EntidadEmpleado MapEmpleado(SqlDataReader rd)
        {
            string Safe(string col) => rd[col] == DBNull.Value ? "" : rd[col].ToString();

            return new EntidadEmpleado
            {
                IdEmpleado = Convert.ToInt32(rd["IdEmpleado"]),
                Nombres = Safe("Nombres"),
                PrimerApellido = Safe("PrimerApellido"),
                SegundoApellido = Safe("SegundoApellido"),
                CI = Safe("CI"),
                FechaNacimiento = Convert.ToDateTime(rd["FechaNacimiento"]),
                Telefono = Safe("Telefono"),
                IdDepartamento = Convert.ToInt32(rd["IdDepartamento"]),
                DepartamentoNombre = Safe("DepartamentoNombre"),
                IdCargo = Convert.ToInt32(rd["IdCargo"]),
                CargoNombre = Safe("CargoNombre"),
                EstadoActual = Safe("EstadoActual"),
                Email = Safe("Email")
            };
        }

        // Activo=1, Inactivo=2, Todos=null
        private static int? NormalizarEstado(string estado)
        {
            string s = (estado ?? "").Trim().ToLowerInvariant();

            if (s.Contains("activo")) return 1;
            if (s.Contains("inactivo")) return 2;
            if (s.Contains("todos")) return null;

            // default: no filtrar
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RcursosHumanoss.EntidadesRRHH;

namespace RcursosHumanoss.DALRRHH
{
    internal static class VacacionDAL
    {
        // =========================================
        // LISTAR (GENERAL)
        // =========================================
        public static List<EntidadVacacion> ListarUltimosConVacaciones(int top)
        {
            const string sql = @"
SELECT TOP (@Top)
    v.IdVacaciones,
    v.IdEmpleado,
    v.FechaInicio,
    v.FechaFin,
    e.CI,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    d.Nombre AS DepartamentoNombre,
    c.Nombre AS CargoNombre
FROM Vacaciones v
INNER JOIN Empleado e ON e.IdEmpleado = v.IdEmpleado
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
ORDER BY v.IdVacaciones DESC;";

            var lista = new List<EntidadVacacion>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapVacacion(rd));
                }
            }

            return lista;
        }

        // =========================================
        // LISTAR (POR DEPARTAMENTO) ✅ NUEVO
        // =========================================
        public static List<EntidadVacacion> ListarUltimasPorDepartamento(int top, int idDepartamento)
        {
            const string sql = @"
SELECT TOP (@Top)
    v.IdVacaciones,
    v.IdEmpleado,
    v.FechaInicio,
    v.FechaFin,
    e.CI,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    d.Nombre AS DepartamentoNombre,
    c.Nombre AS CargoNombre
FROM Vacaciones v
INNER JOIN Empleado e ON e.IdEmpleado = v.IdEmpleado
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
WHERE e.IdDepartamento = @IdDepartamento
ORDER BY v.IdVacaciones DESC;";

            var lista = new List<EntidadVacacion>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
                cmd.Parameters.Add("@IdDepartamento", SqlDbType.Int).Value = idDepartamento;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapVacacion(rd));
                }
            }

            return lista;
        }

        // =========================================
        // BUSCAR (GENERAL)
        // =========================================
        public static List<EntidadVacacion> Buscar(string criterio, string valor)
        {
            criterio = (criterio ?? "").Trim().ToUpperInvariant();
            valor = (valor ?? "").Trim();

            string where;
            if (criterio == "CI")
                where = "e.CI LIKE @Valor";
            else if (criterio == "NOMBRE" || criterio == "NOMBRES")
                where = "e.Nombres LIKE @Valor";
            else if (criterio == "APELLIDOS" || criterio == "APELLIDO")
                where = "(e.PrimerApellido LIKE @Valor OR e.SegundoApellido LIKE @Valor)";
            else
                where = "(e.CI LIKE @Valor OR e.Nombres LIKE @Valor OR e.PrimerApellido LIKE @Valor OR e.SegundoApellido LIKE @Valor)";

            string sql = $@"
SELECT
    v.IdVacaciones,
    v.IdEmpleado,
    v.FechaInicio,
    v.FechaFin,
    e.CI,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    d.Nombre AS DepartamentoNombre,
    c.Nombre AS CargoNombre
FROM Vacaciones v
INNER JOIN Empleado e ON e.IdEmpleado = v.IdEmpleado
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
WHERE {where}
ORDER BY v.IdVacaciones DESC;";

            var lista = new List<EntidadVacacion>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Valor", SqlDbType.NVarChar, 200).Value = "%" + valor + "%";

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapVacacion(rd));
                }
            }

            return lista;
        }

        // =========================================
        // BUSCAR (POR DEPARTAMENTO) ✅ NUEVO
        // =========================================
        public static List<EntidadVacacion> BuscarEnDepartamento(string criterio, string valor, int idDepartamento)
        {
            criterio = (criterio ?? "").Trim().ToUpperInvariant();
            valor = (valor ?? "").Trim();

            string where;
            if (criterio == "CI")
                where = "e.CI LIKE @Valor";
            else if (criterio == "NOMBRE" || criterio == "NOMBRES")
                where = "e.Nombres LIKE @Valor";
            else if (criterio == "APELLIDOS" || criterio == "APELLIDO")
                where = "(e.PrimerApellido LIKE @Valor OR e.SegundoApellido LIKE @Valor)";
            else
                where = "(e.CI LIKE @Valor OR e.Nombres LIKE @Valor OR e.PrimerApellido LIKE @Valor OR e.SegundoApellido LIKE @Valor)";

            string sql = $@"
SELECT
    v.IdVacaciones,
    v.IdEmpleado,
    v.FechaInicio,
    v.FechaFin,
    e.CI,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    d.Nombre AS DepartamentoNombre,
    c.Nombre AS CargoNombre
FROM Vacaciones v
INNER JOIN Empleado e ON e.IdEmpleado = v.IdEmpleado
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
WHERE e.IdDepartamento = @IdDepartamento
  AND {where}
ORDER BY v.IdVacaciones DESC;";

            var lista = new List<EntidadVacacion>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@IdDepartamento", SqlDbType.Int).Value = idDepartamento;
                cmd.Parameters.Add("@Valor", SqlDbType.NVarChar, 200).Value = "%" + valor + "%";

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapVacacion(rd));
                }
            }

            return lista;
        }

        // =========================================
        // INSERTAR
        // =========================================
        public static int Insertar(int idEmpleado, DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaFin.Date < fechaInicio.Date)
                throw new ArgumentException("La FechaFin no puede ser menor a la FechaInicio.");

            const string sql = @"
INSERT INTO Vacaciones (IdEmpleado, FechaInicio, FechaFin)
VALUES (@IdEmpleado, @Inicio, @Fin);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@IdEmpleado", SqlDbType.Int).Value = idEmpleado;
                cmd.Parameters.Add("@Inicio", SqlDbType.Date).Value = fechaInicio.Date;
                cmd.Parameters.Add("@Fin", SqlDbType.Date).Value = fechaFin.Date;

                object r = cmd.ExecuteScalar();
                return (r == null || r == DBNull.Value) ? 0 : Convert.ToInt32(r);
            }
        }

        // =========================================
        // ACTUALIZAR
        // =========================================
        public static bool Actualizar(int idVacaciones, DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaFin.Date < fechaInicio.Date)
                throw new ArgumentException("La FechaFin no puede ser menor a la FechaInicio.");

            const string sql = @"
UPDATE Vacaciones
SET FechaInicio = @Inicio,
    FechaFin = @Fin
WHERE IdVacaciones = @Id;";

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = idVacaciones;
                cmd.Parameters.Add("@Inicio", SqlDbType.Date).Value = fechaInicio.Date;
                cmd.Parameters.Add("@Fin", SqlDbType.Date).Value = fechaFin.Date;

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================
        // ELIMINAR (BORRA)
        // =========================================
        public static bool EliminarPorId(int idVacaciones)
        {
            const string sql = @"DELETE FROM Vacaciones WHERE IdVacaciones = @Id;";

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = idVacaciones;
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================
        // SOLAPAMIENTO
        // =========================================
        public static bool ExisteSolapamiento(int idEmpleado, DateTime inicio, DateTime fin)
        {
            const string sql = @"
SELECT COUNT(1)
FROM Vacaciones
WHERE IdEmpleado = @IdEmpleado
  AND NOT (FechaFin < @Inicio OR FechaInicio > @Fin);";

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@IdEmpleado", SqlDbType.Int).Value = idEmpleado;
                cmd.Parameters.Add("@Inicio", SqlDbType.Date).Value = inicio.Date;
                cmd.Parameters.Add("@Fin", SqlDbType.Date).Value = fin.Date;

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static bool ExisteSolapamientoExcepto(int idEmpleado, int idVacaciones, DateTime inicio, DateTime fin)
        {
            const string sql = @"
SELECT COUNT(1)
FROM Vacaciones
WHERE IdEmpleado = @IdEmpleado
  AND IdVacaciones <> @IdVacaciones
  AND NOT (FechaFin < @Inicio OR FechaInicio > @Fin);";

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@IdEmpleado", SqlDbType.Int).Value = idEmpleado;
                cmd.Parameters.Add("@IdVacaciones", SqlDbType.Int).Value = idVacaciones;
                cmd.Parameters.Add("@Inicio", SqlDbType.Date).Value = inicio.Date;
                cmd.Parameters.Add("@Fin", SqlDbType.Date).Value = fin.Date;

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // =========================================
        // MAP
        // =========================================
        private static EntidadVacacion MapVacacion(SqlDataReader rd)
        {
            string SafeString(string col) => rd[col] == DBNull.Value ? "" : rd[col].ToString();

            return new EntidadVacacion
            {
                IdVacaciones = Convert.ToInt32(rd["IdVacaciones"]),
                IdEmpleado = Convert.ToInt32(rd["IdEmpleado"]),
                FechaInicio = Convert.ToDateTime(rd["FechaInicio"]),
                FechaFin = Convert.ToDateTime(rd["FechaFin"]),
                CI = SafeString("CI"),
                Nombres = SafeString("Nombres"),
                PrimerApellido = SafeString("PrimerApellido"),
                SegundoApellido = SafeString("SegundoApellido"),
                DepartamentoNombre = SafeString("DepartamentoNombre"),
                CargoNombre = SafeString("CargoNombre")
            };
        }
    }
}

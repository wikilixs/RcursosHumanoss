// EmpleadoDAL.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RcursosHumanoss.EntidadesRRHH;
using RcursosHumanoss.SesionRRHH; // SesionActual

namespace RcursosHumanoss.DALRRHH
{
    internal static class EmpleadoDAL
    {
        // =========================================================
        //  LISTAR: últimos empleados ACTIVOS (para Empleado.cs)
        // =========================================================
        public static List<EntidadEmpleado> ListarUltimosActivos(int top)
        {
            const string sql = @"
SELECT TOP (@Top)
    e.IdEmpleado,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    e.CI,
    e.FechaNacimiento,
    e.Telefono,
    e.IdDepartamento,
    e.IdCargo,
    d.Nombre AS DepartamentoNombre,
    c.Nombre AS CargoNombre,
    ISNULL(est.Nombre, 'Activo') AS EstadoActual
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
WHERE ISNULL(est.Nombre, 'Activo') = 'Activo'
ORDER BY e.IdEmpleado DESC;";

            var lista = new List<EntidadEmpleado>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;

                using (SqlDataReader rd = cmd.ExecuteReader())
                    while (rd.Read())
                        lista.Add(MapEmpleado(rd));
            }

            return lista;
        }

        // =========================================================
        //  BUSCAR + ESTADO (para Empleado.cs)
        //  criterio: "CI" | "Nombres" | "Apellidos" | "TODOS"
        //  estado: "Activo" | "Inactivo" | "Todos"
        // =========================================================
        public static List<EntidadEmpleado> BuscarConEstado(string criterio, string valor, string estado)
        {
            criterio = (criterio ?? "").Trim().ToUpperInvariant();
            valor = (valor ?? "").Trim();
            estado = (estado ?? "").Trim().ToUpperInvariant();

            string whereCriterio;
            if (criterio == "CI")
                whereCriterio = "e.CI LIKE @Valor";
            else if (criterio == "NOMBRES" || criterio == "NOMBRE")
                whereCriterio = "e.Nombres LIKE @Valor";
            else if (criterio == "APELLIDOS" || criterio == "APELLIDO")
                whereCriterio = "(e.PrimerApellido LIKE @Valor OR e.SegundoApellido LIKE @Valor)";
            else // "TODOS"
                whereCriterio = "(e.CI LIKE @Valor OR e.Nombres LIKE @Valor OR e.PrimerApellido LIKE @Valor OR e.SegundoApellido LIKE @Valor)";

            string whereEstado;
            if (estado == "ACTIVO")
                whereEstado = "ISNULL(est.Nombre, 'Activo') = 'Activo'";
            else if (estado == "INACTIVO")
                whereEstado = "ISNULL(est.Nombre, 'Activo') = 'Inactivo'";
            else
                whereEstado = "1=1"; // Todos

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
    e.IdCargo,
    d.Nombre AS DepartamentoNombre,
    c.Nombre AS CargoNombre,
    ISNULL(est.Nombre, 'Activo') AS EstadoActual
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
WHERE {whereCriterio}
  AND {whereEstado}
ORDER BY e.IdEmpleado DESC;";

            var lista = new List<EntidadEmpleado>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Valor", SqlDbType.NVarChar, 200).Value = "%" + valor + "%";

                using (SqlDataReader rd = cmd.ExecuteReader())
                    while (rd.Read())
                        lista.Add(MapEmpleado(rd));
            }

            return lista;
        }

        // =========================================================
        //  INSERTAR: Empleado + Usuario automático (para EmpleadoAgregar.cs)
        //  Email: nombre.apellido@tiendaboli.com
        //  PasswordHash: BCrypt(CI)
        //  Estado inicial: Activo (IdEstado=1) en EstadoHistorial
        // =========================================================
        public static int InsertarUsuarioAuto(EntidadEmpleado emp)
        {
            if (emp == null) throw new ArgumentNullException(nameof(emp));

            // Validaciones mínimas (puedes ampliar)
            if (string.IsNullOrWhiteSpace(emp.Nombres)) throw new ArgumentException("Nombres es obligatorio.");
            if (string.IsNullOrWhiteSpace(emp.PrimerApellido)) throw new ArgumentException("PrimerApellido es obligatorio.");
            if (string.IsNullOrWhiteSpace(emp.CI)) throw new ArgumentException("CI es obligatorio.");
            if (string.IsNullOrWhiteSpace(emp.Telefono)) throw new ArgumentException("Telefono es obligatorio.");

            const string sqlEmpleado = @"
INSERT INTO Empleado
(Nombres, PrimerApellido, SegundoApellido, CI, FechaNacimiento, Telefono, IdDepartamento, IdCargo)
VALUES
(@Nombres, @PrimerApellido, @SegundoApellido, @CI, @FechaNacimiento, @Telefono, @IdDepartamento, @IdCargo);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            const string sqlUsuario = @"
INSERT INTO Usuario (Email, PasswordHash, IdEmpleado)
VALUES (@Email, @PasswordHash, @IdEmpleado);";

            // Activo=1 / Inactivo=2 (según tu modelo)
            const string sqlEstado = @"
INSERT INTO EstadoHistorial (IdEmpleado, IdEstado, Fecha)
VALUES (@IdEmpleado, @IdEstado, GETDATE());";

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    // 1) Insert Empleado
                    int idEmpleado;
                    using (SqlCommand cmd = new SqlCommand(sqlEmpleado, cn, tx))
                    {
                        cmd.Parameters.Add("@Nombres", SqlDbType.NVarChar, 100).Value = emp.Nombres.Trim();
                        cmd.Parameters.Add("@PrimerApellido", SqlDbType.NVarChar, 100).Value = emp.PrimerApellido.Trim();
                        cmd.Parameters.Add("@SegundoApellido", SqlDbType.NVarChar, 100).Value =
                            string.IsNullOrWhiteSpace(emp.SegundoApellido) ? (object)DBNull.Value : emp.SegundoApellido.Trim();
                        cmd.Parameters.Add("@CI", SqlDbType.NVarChar, 50).Value = emp.CI.Trim();
                        cmd.Parameters.Add("@FechaNacimiento", SqlDbType.Date).Value = emp.FechaNacimiento.Date;
                        cmd.Parameters.Add("@Telefono", SqlDbType.NVarChar, 50).Value = emp.Telefono.Trim();
                        cmd.Parameters.Add("@IdDepartamento", SqlDbType.Int).Value = emp.IdDepartamento;
                        cmd.Parameters.Add("@IdCargo", SqlDbType.Int).Value = emp.IdCargo;

                        object r = cmd.ExecuteScalar();
                        idEmpleado = (r == null || r == DBNull.Value) ? 0 : Convert.ToInt32(r);
                        if (idEmpleado <= 0) throw new Exception("No se pudo insertar el empleado.");
                    }

                    // 2) Estado inicial (Activo=1)
                    using (SqlCommand cmd = new SqlCommand(sqlEstado, cn, tx))
                    {
                        cmd.Parameters.Add("@IdEmpleado", SqlDbType.Int).Value = idEmpleado;
                        cmd.Parameters.Add("@IdEstado", SqlDbType.Int).Value = 1; // Activo
                        cmd.ExecuteNonQuery();
                    }

                    // 3) Insert Usuario (email + bcrypt(CI))
                    string email = GenerarEmail(emp.Nombres, emp.PrimerApellido);
                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(emp.CI.Trim());

                    using (SqlCommand cmd = new SqlCommand(sqlUsuario, cn, tx))
                    {
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = email;
                        cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 300).Value = passwordHash;
                        cmd.Parameters.Add("@IdEmpleado", SqlDbType.Int).Value = idEmpleado;
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return idEmpleado;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        // =========================================================
        //  ACTUALIZAR datos del empleado (para EmpleadoActualizar)
        //  Nota: el cambio Activo/Inactivo se hace con CambiarEstado(...)
        // =========================================================
        public static bool Actualizar(EntidadEmpleado emp)
        {
            if (emp == null) throw new ArgumentNullException(nameof(emp));
            if (emp.IdEmpleado <= 0) throw new ArgumentException("IdEmpleado inválido.");

            const string sql = @"
UPDATE Empleado
SET Nombres = @Nombres,
    PrimerApellido = @PrimerApellido,
    SegundoApellido = @SegundoApellido,
    CI = @CI,
    FechaNacimiento = @FechaNacimiento,
    Telefono = @Telefono,
    IdDepartamento = @IdDepartamento,
    IdCargo = @IdCargo
WHERE IdEmpleado = @IdEmpleado;";

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@IdEmpleado", SqlDbType.Int).Value = emp.IdEmpleado;
                cmd.Parameters.Add("@Nombres", SqlDbType.NVarChar, 100).Value = emp.Nombres?.Trim() ?? "";
                cmd.Parameters.Add("@PrimerApellido", SqlDbType.NVarChar, 100).Value = emp.PrimerApellido?.Trim() ?? "";
                cmd.Parameters.Add("@SegundoApellido", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(emp.SegundoApellido) ? (object)DBNull.Value : emp.SegundoApellido.Trim();
                cmd.Parameters.Add("@CI", SqlDbType.NVarChar, 50).Value = emp.CI?.Trim() ?? "";
                cmd.Parameters.Add("@FechaNacimiento", SqlDbType.Date).Value = emp.FechaNacimiento.Date;
                cmd.Parameters.Add("@Telefono", SqlDbType.NVarChar, 50).Value = emp.Telefono?.Trim() ?? "";
                cmd.Parameters.Add("@IdDepartamento", SqlDbType.Int).Value = emp.IdDepartamento;
                cmd.Parameters.Add("@IdCargo", SqlDbType.Int).Value = emp.IdCargo;

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        //  CAMBIAR ESTADO: Activo=1 / Inactivo=2 (inserta historial)
        // =========================================================
        public static bool CambiarEstado(int idEmpleado, int idEstado) // 1=Activo, 2=Inactivo
        {
            if (idEmpleado <= 0) throw new ArgumentException("IdEmpleado inválido.");
            if (idEstado != 1 && idEstado != 2) throw new ArgumentException("IdEstado debe ser 1 (Activo) o 2 (Inactivo).");

            const string sql = @"
INSERT INTO EstadoHistorial (IdEmpleado, IdEstado, Fecha)
VALUES (@IdEmpleado, @IdEstado, GETDATE());";

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@IdEmpleado", SqlDbType.Int).Value = idEmpleado;
                cmd.Parameters.Add("@IdEstado", SqlDbType.Int).Value = idEstado;
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        //  BUSCAR POR CI (útil en Actualizar / buscar exacto)
        // =========================================================
        public static EntidadEmpleado? BuscarPorCIExacto(string ci)
        {
            ci = (ci ?? "").Trim();
            if (ci.Length == 0) return null;

            const string sql = @"
SELECT TOP 1
    e.IdEmpleado,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    e.CI,
    e.FechaNacimiento,
    e.Telefono,
    e.IdDepartamento,
    e.IdCargo,
    d.Nombre AS DepartamentoNombre,
    c.Nombre AS CargoNombre,
    ISNULL(est.Nombre, 'Activo') AS EstadoActual
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
WHERE e.CI = @CI
ORDER BY e.IdEmpleado DESC;";

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@CI", SqlDbType.NVarChar, 50).Value = ci;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    return MapEmpleado(rd);
                }
            }
        }

        // =========================================================
        //  MI ÁREA: últimos activos (para EmpleadoArea.cs)
        //  Usa SesionActual.IdDepartamento internamente
        // =========================================================
        public static List<EntidadEmpleado> ListarUltimosActivosPorDepartamento(int top)
        {
            SesionActual.AsegurarSesion();
            if (SesionActual.IdDepartamento <= 0) throw new InvalidOperationException("Sesión sin IdDepartamento.");

            const string sql = @"
SELECT TOP (@Top)
    e.IdEmpleado,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    e.CI,
    e.FechaNacimiento,
    e.Telefono,
    e.IdDepartamento,
    e.IdCargo,
    d.Nombre AS DepartamentoNombre,
    c.Nombre AS CargoNombre,
    ISNULL(est.Nombre, 'Activo') AS EstadoActual
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
WHERE e.IdDepartamento = @IdDep
  AND ISNULL(est.Nombre, 'Activo') = 'Activo'
ORDER BY e.IdEmpleado DESC;";

            var lista = new List<EntidadEmpleado>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
                cmd.Parameters.Add("@IdDep", SqlDbType.Int).Value = SesionActual.IdDepartamento;

                using (SqlDataReader rd = cmd.ExecuteReader())
                    while (rd.Read())
                        lista.Add(MapEmpleado(rd));
            }

            return lista;
        }

        // =========================================================
        //  MI ÁREA: buscar dentro del departamento (para EmpleadoArea.cs)
        // =========================================================
        public static List<EntidadEmpleado> BuscarEnDepartamento(string criterio, string valor)
        {
            SesionActual.AsegurarSesion();
            if (SesionActual.IdDepartamento <= 0) throw new InvalidOperationException("Sesión sin IdDepartamento.");

            criterio = (criterio ?? "").Trim().ToUpperInvariant();
            valor = (valor ?? "").Trim();

            string whereCriterio;
            if (criterio == "CI")
                whereCriterio = "e.CI LIKE @Valor";
            else if (criterio == "NOMBRES" || criterio == "NOMBRE")
                whereCriterio = "e.Nombres LIKE @Valor";
            else if (criterio == "APELLIDOS" || criterio == "APELLIDO")
                whereCriterio = "(e.PrimerApellido LIKE @Valor OR e.SegundoApellido LIKE @Valor)";
            else
                whereCriterio = "(e.CI LIKE @Valor OR e.Nombres LIKE @Valor OR e.PrimerApellido LIKE @Valor OR e.SegundoApellido LIKE @Valor)";

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
    e.IdCargo,
    d.Nombre AS DepartamentoNombre,
    c.Nombre AS CargoNombre,
    ISNULL(est.Nombre, 'Activo') AS EstadoActual
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
WHERE e.IdDepartamento = @IdDep
  AND {whereCriterio}
ORDER BY e.IdEmpleado DESC;";

            var lista = new List<EntidadEmpleado>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@IdDep", SqlDbType.Int).Value = SesionActual.IdDepartamento;
                cmd.Parameters.Add("@Valor", SqlDbType.NVarChar, 200).Value = "%" + valor + "%";

                using (SqlDataReader rd = cmd.ExecuteReader())
                    while (rd.Read())
                        lista.Add(MapEmpleado(rd));
            }

            return lista;
        }

        // =========================================================
        //  HELPERS
        // =========================================================
        private static EntidadEmpleado MapEmpleado(SqlDataReader rd)
        {
            string SafeString(string col) => rd[col] == DBNull.Value ? "" : rd[col].ToString();

            return new EntidadEmpleado
            {
                IdEmpleado = Convert.ToInt32(rd["IdEmpleado"]),
                Nombres = SafeString("Nombres"),
                PrimerApellido = SafeString("PrimerApellido"),
                SegundoApellido = SafeString("SegundoApellido"),
                CI = SafeString("CI"),
                FechaNacimiento = Convert.ToDateTime(rd["FechaNacimiento"]),
                Telefono = SafeString("Telefono"),
                IdDepartamento = Convert.ToInt32(rd["IdDepartamento"]),
                IdCargo = Convert.ToInt32(rd["IdCargo"]),
                DepartamentoNombre = SafeString("DepartamentoNombre"),
                CargoNombre = SafeString("CargoNombre"),
                EstadoActual = SafeString("EstadoActual"),
                Email = "" // si luego haces JOIN con Usuario, aquí lo mapeas
            };
        }

        private static string GenerarEmail(string nombres, string primerApellido)
        {
            string nom = (nombres ?? "").Trim().Split(' ')[0].ToLowerInvariant();
            string ape = (primerApellido ?? "").Trim().ToLowerInvariant();
            return $"{nom}.{ape}@tiendaboli.com";
        }
    }
}

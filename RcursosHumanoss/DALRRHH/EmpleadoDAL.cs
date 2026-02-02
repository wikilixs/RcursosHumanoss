using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using RcursosHumanoss.EntidadesRRHH;

namespace RcursosHumanoss.DALRRHH
{
    internal static class EmpleadoDAL
    {
        // ===========================
        //  IDs DE ESTADO (según tu DB)
        // ===========================
        private const int ESTADO_ACTIVO = 5;
        private const int ESTADO_INACTIVO = 6;

        // ===========================
        //  PUBLIC: LISTAR (ACTIVOS)
        // ===========================
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
    ORDER BY eh.FechaRegitro DESC, eh.IdHistorial DESC
) est
WHERE ISNULL(est.IdEstado, @Activo) = @Activo
ORDER BY e.IdEmpleado DESC;";

            var lista = new List<EntidadEmpleado>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
                cmd.Parameters.Add("@Activo", SqlDbType.Int).Value = ESTADO_ACTIVO;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapEmpleado(rd));
                }
            }

            return lista;
        }

        // ==========================================
        //  PUBLIC: BUSCAR (POR ESTADO)
        //  criterio: CI / Nombres / Apellidos / TODOS
        //  estado: Activo / Inactivo / Todos
        // ==========================================
        public static List<EntidadEmpleado> BuscarConEstado(string criterio, string valor, string estado)
        {
            criterio = (criterio ?? "").Trim().ToUpperInvariant();
            valor = (valor ?? "").Trim();

            int? idEstadoFiltro = NormalizarEstado(estado); // 5/6/null

            string whereCriterio;
            if (criterio == "CI")
                whereCriterio = "e.CI LIKE @Valor";
            else if (criterio == "NOMBRE" || criterio == "NOMBRES")
                whereCriterio = "e.Nombres LIKE @Valor";
            else if (criterio == "APELLIDOS" || criterio == "APELLIDO")
                whereCriterio = "(e.PrimerApellido LIKE @Valor OR e.SegundoApellido LIKE @Valor)";
            else
                whereCriterio = "(e.CI LIKE @Valor OR e.Nombres LIKE @Valor OR e.PrimerApellido LIKE @Valor OR e.SegundoApellido LIKE @Valor)";

            bool sinFiltroTexto = (criterio == "TODOS" || criterio == "") && string.IsNullOrWhiteSpace(valor);

            string whereEstado = idEstadoFiltro.HasValue
                ? "AND ISNULL(est.IdEstado, @Activo) = @IdEstado"
                : "";

            string whereFinal = sinFiltroTexto ? "1=1" : whereCriterio;

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
    ORDER BY eh.FechaRegitro DESC, eh.IdHistorial DESC
) est
WHERE {whereFinal}
{whereEstado}
ORDER BY e.IdEmpleado DESC;";

            var lista = new List<EntidadEmpleado>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Activo", SqlDbType.Int).Value = ESTADO_ACTIVO;

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

        // ===========================
        //  INSERTAR + CREAR USUARIO
        //  Email estilo: juan.mamani@tiendaboli.com
        //  Estado inicial: Activo (5)
        // ===========================
        public static int InsertarUsuarioAuto(EntidadEmpleado emp)
        {
            if (emp == null) throw new ArgumentNullException(nameof(emp));

            if (string.IsNullOrWhiteSpace(emp.Nombres)) throw new ArgumentException("Nombres es obligatorio.");
            if (string.IsNullOrWhiteSpace(emp.PrimerApellido)) throw new ArgumentException("PrimerApellido es obligatorio.");
            if (string.IsNullOrWhiteSpace(emp.CI)) throw new ArgumentException("CI es obligatorio.");
            if (string.IsNullOrWhiteSpace(emp.Telefono)) throw new ArgumentException("Telefono es obligatorio.");
            if (emp.IdDepartamento <= 0) throw new ArgumentException("IdDepartamento inválido.");
            if (emp.IdCargo <= 0) throw new ArgumentException("IdCargo inválido.");

            // Genera email base
            string email = GenerarEmail(emp.Nombres, emp.PrimerApellido);

            // Password por defecto (hash)
            string passwordPlano = "123456";
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(passwordPlano);

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    // 1) Insert Empleado
                    const string sqlEmp = @"
INSERT INTO Empleado (Nombres, PrimerApellido, SegundoApellido, CI, FechaNacimiento, Telefono, IdDepartamento, IdCargo)
VALUES (@Nombres, @PA, @SA, @CI, @FN, @Tel, @IdDep, @IdCargo);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int idEmpleado;

                    using (SqlCommand cmdEmp = new SqlCommand(sqlEmp, cn, tx))
                    {
                        cmdEmp.Parameters.Add("@Nombres", SqlDbType.NVarChar, 200).Value = emp.Nombres.Trim();
                        cmdEmp.Parameters.Add("@PA", SqlDbType.NVarChar, 120).Value = emp.PrimerApellido.Trim();
                        cmdEmp.Parameters.Add("@SA", SqlDbType.NVarChar, 120).Value =
                            string.IsNullOrWhiteSpace(emp.SegundoApellido) ? (object)DBNull.Value : emp.SegundoApellido.Trim();

                        cmdEmp.Parameters.Add("@CI", SqlDbType.NVarChar, 50).Value = emp.CI.Trim();
                        cmdEmp.Parameters.Add("@FN", SqlDbType.Date).Value = emp.FechaNacimiento.Date;
                        cmdEmp.Parameters.Add("@Tel", SqlDbType.NVarChar, 50).Value = emp.Telefono.Trim();
                        cmdEmp.Parameters.Add("@IdDep", SqlDbType.Int).Value = emp.IdDepartamento;
                        cmdEmp.Parameters.Add("@IdCargo", SqlDbType.Int).Value = emp.IdCargo;

                        object r = cmdEmp.ExecuteScalar();
                        idEmpleado = (r == null || r == DBNull.Value) ? 0 : Convert.ToInt32(r);
                    }

                    if (idEmpleado <= 0)
                        throw new Exception("No se pudo insertar el empleado.");

                    // 2) Insert EstadoHistorial -> Activo(5) + FechaRegitro ✅
                    const string sqlEstado = @"
INSERT INTO EstadoHistorial (IdEmpleado, IdEstado, FechaRegitro)
VALUES (@IdEmpleado, @IdEstado, GETDATE());";

                    using (SqlCommand cmdEst = new SqlCommand(sqlEstado, cn, tx))
                    {
                        cmdEst.Parameters.Add("@IdEmpleado", SqlDbType.Int).Value = idEmpleado;
                        cmdEst.Parameters.Add("@IdEstado", SqlDbType.Int).Value = ESTADO_ACTIVO;
                        cmdEst.ExecuteNonQuery();
                    }

                    // 3) Insert Usuario
                    const string sqlUser = @"
INSERT INTO Usuario (Email, [Password], IdEmpleado)
VALUES (@Email, @Pass, @IdEmpleado);";

                    using (SqlCommand cmdUser = new SqlCommand(sqlUser, cn, tx))
                    {
                        cmdUser.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = email;
                        cmdUser.Parameters.Add("@Pass", SqlDbType.NVarChar, -1).Value = passwordHash;
                        cmdUser.Parameters.Add("@IdEmpleado", SqlDbType.Int).Value = idEmpleado;
                        cmdUser.ExecuteNonQuery();
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
        //  SUPERVISOR: LISTAR POR DEPARTAMENTO
        // =========================================================
        public static List<EntidadEmpleado> ListarUltimosActivosPorDepartamento(int top)
        {
            int idDep = ObtenerIdDepartamentoSesion();
            if (idDep <= 0)
                throw new InvalidOperationException("No se pudo obtener IdDepartamento desde la sesión.");

            return ListarUltimosActivosPorDepartamento(top, idDep);
        }

        public static List<EntidadEmpleado> ListarUltimosActivosPorDepartamento(int top, int idDepartamento)
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
    ORDER BY eh.FechaRegitro DESC, eh.IdHistorial DESC
) est
WHERE e.IdDepartamento = @IdDep
  AND ISNULL(est.IdEstado, @Activo) = @Activo
ORDER BY e.IdEmpleado DESC;";

            var lista = new List<EntidadEmpleado>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
                cmd.Parameters.Add("@IdDep", SqlDbType.Int).Value = idDepartamento;
                cmd.Parameters.Add("@Activo", SqlDbType.Int).Value = ESTADO_ACTIVO;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapEmpleado(rd));
                }
            }

            return lista;
        }

        // =========================================================
        //  SUPERVISOR: BUSCAR EN SU DEPARTAMENTO
        // =========================================================
        public static List<EntidadEmpleado> BuscarEnDepartamento(string criterio, string valor)
        {
            int idDep = ObtenerIdDepartamentoSesion();
            if (idDep <= 0)
                throw new InvalidOperationException("No se pudo obtener IdDepartamento desde la sesión.");

            return BuscarEnDepartamento(criterio, valor, idDep);
        }

        public static List<EntidadEmpleado> BuscarEnDepartamento(string criterio, string valor, int idDepartamento)
        {
            criterio = (criterio ?? "").Trim().ToUpperInvariant();
            valor = (valor ?? "").Trim();

            string whereCriterio;
            if (criterio == "CI")
                whereCriterio = "e.CI LIKE @Valor";
            else if (criterio == "NOMBRE" || criterio == "NOMBRES")
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
    ORDER BY eh.FechaRegitro DESC, eh.IdHistorial DESC
) est
WHERE e.IdDepartamento = @IdDep
  AND {whereCriterio}
ORDER BY e.IdEmpleado DESC;";

            var lista = new List<EntidadEmpleado>();

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@IdDep", SqlDbType.Int).Value = idDepartamento;
                cmd.Parameters.Add("@Valor", SqlDbType.NVarChar, 200).Value = "%" + valor + "%";

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        lista.Add(MapEmpleado(rd));
                }
            }

            return lista;
        }

        // =========================================================
        //  COMPATIBILIDAD (si algún form llama ListarUltimos/Buscar)
        // =========================================================
        public static List<EntidadEmpleado> ListarUltimos(int top, int? idEstado)
        {
            // tu DB: 5 Activo / 6 Inactivo
            if (idEstado == ESTADO_ACTIVO)
                return BuscarConEstado("TODOS", "", "Activo");

            if (idEstado == ESTADO_INACTIVO)
                return BuscarConEstado("TODOS", "", "Inactivo");

            return BuscarConEstado("TODOS", "", "Todos");
        }

        public static List<EntidadEmpleado> Buscar(int? idEstado, string criterio, string valor)
        {
            string estado;
            if (idEstado == ESTADO_ACTIVO) estado = "Activo";
            else if (idEstado == ESTADO_INACTIVO) estado = "Inactivo";
            else estado = "Todos";

            return BuscarConEstado(criterio, valor, estado);
        }

        // =========================================================
        //  HELPERS
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
                IdCargo = Convert.ToInt32(rd["IdCargo"]),

                DepartamentoNombre = Safe("DepartamentoNombre"),
                CargoNombre = Safe("CargoNombre"),
                EstadoActual = Safe("EstadoActual"),
                Email = Safe("Email")
            };
        }

        // Activo=5, Inactivo=6, Todos=null
        private static int? NormalizarEstado(string estado)
        {
            string s = (estado ?? "").Trim().ToLowerInvariant();

            if (s.Contains("activo")) return ESTADO_ACTIVO;
            if (s.Contains("inactivo")) return ESTADO_INACTIVO;
            if (s.Contains("todos")) return null;

            // si viene vacío -> por defecto Activo (como suelen hacer tus pantallas)
            if (string.IsNullOrWhiteSpace(s)) return ESTADO_ACTIVO;

            return null;
        }

        private static string GenerarEmail(string nombres, string primerApellido)
        {
            string primerNombre = ExtraerPrimerToken(nombres);
            string apellido = ExtraerPrimerToken(primerApellido);

            string user = $"{primerNombre}.{apellido}".ToLowerInvariant();
            user = QuitarAcentos(user);
            user = user.Replace(" ", "").Replace("'", "").Replace("\"", "");

            return $"{user}@tiendaboli.com";
        }

        private static string ExtraerPrimerToken(string s)
        {
            s = (s ?? "").Trim();
            if (string.IsNullOrWhiteSpace(s)) return "user";
            string[] parts = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : s;
        }

        private static string QuitarAcentos(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            string normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // Obtiene IdDepartamento desde sesión sin depender del nombre exacto de la clase.
        // Busca en este orden:
        // - RcursosHumanoss.SesionRRHH.EnridadSesion
        // - RcursosHumanoss.SesionRRHH.EntidadSesion
        // - RcursosHumanoss.SesionRRHH.SesionActual
        private static int ObtenerIdDepartamentoSesion()
        {
            int id = TryGetStaticInt("RcursosHumanoss.SesionRRHH.EnridadSesion", "IdDepartamento");
            if (id > 0) return id;

            id = TryGetStaticInt("RcursosHumanoss.SesionRRHH.EntidadSesion", "IdDepartamento");
            if (id > 0) return id;

            id = TryGetStaticInt("RcursosHumanoss.SesionRRHH.SesionActual", "IdDepartamento");
            return id;
        }

        private static int TryGetStaticInt(string fullTypeName, string propName)
        {
            try
            {
                Type t = Type.GetType(fullTypeName);
                if (t == null) return 0;

                var p = t.GetProperty(propName,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Static);

                if (p == null) return 0;

                object v = p.GetValue(null);
                if (v == null) return 0;

                return Convert.ToInt32(v);
            }
            catch
            {
                return 0;
            }
        }
    }
}

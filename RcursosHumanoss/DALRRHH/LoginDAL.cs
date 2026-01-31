using System;
using System.Data;
using System.Data.SqlClient;
using RcursosHumanoss.EntidadesRRHH;

namespace RcursosHumanoss.DALRRHH
{
    internal static class LoginDAL
    {
        public static EntidadLogin? ObtenerPorEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            const string sql = @"
SELECT TOP 1
    u.Email,
    u.[Password] AS PasswordHash,   -- tu columna real
    e.IdEmpleado,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    e.IdDepartamento,
    d.Nombre AS DepartamentoNombre,
    e.IdCargo,
    c.Nombre AS CargoNombre
FROM Usuario u
INNER JOIN Empleado e ON e.IdEmpleado = u.IdEmpleado
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
WHERE u.Email = @Email;";

            using (SqlConnection cn = ConexionDB.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = email.Trim();

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (!rd.Read())
                        return null;

                    string nombres = rd["Nombres"]?.ToString() ?? "";
                    string pa = rd["PrimerApellido"]?.ToString() ?? "";
                    string sa = (rd["SegundoApellido"] == DBNull.Value) ? "" : (rd["SegundoApellido"]?.ToString() ?? "");
                    string nombreCompleto = $"{nombres} {pa} {sa}".Trim();

                    return new EntidadLogin
                    {
                        Email = rd["Email"]?.ToString() ?? "",
                        PasswordHash = rd["PasswordHash"]?.ToString() ?? "",

                        IdEmpleado = Convert.ToInt32(rd["IdEmpleado"]),

                        IdDepartamento = Convert.ToInt32(rd["IdDepartamento"]),
                        Departamento = rd["DepartamentoNombre"]?.ToString() ?? "",

                        IdCargo = Convert.ToInt32(rd["IdCargo"]),
                        Cargo = rd["CargoNombre"]?.ToString() ?? "",

                        NombreCompleto = nombreCompleto
                    };
                }
            }
        }
    }
}

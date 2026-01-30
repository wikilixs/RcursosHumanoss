using System;

namespace RcursosHumanoss.SesionRRHH
{
    internal static class SesionActual
    {
        // ==========================
        //  Datos base de sesión
        // ==========================
        public static int IdEmpleado { get; private set; }
        public static string Email { get; private set; } = "";

        // Para “mi área”
        public static int IdDepartamento { get; private set; }
        public static string Departamento { get; private set; } = "";

        // Rol / cargo
        public static int IdCargo { get; private set; }  // útil si necesitas validar permisos por Id
        public static string Cargo { get; private set; } = "";

        // Opcional: mostrar en UI
        public static string NombreCompleto { get; private set; } = "";

        // Bandera rápida
        public static bool HaySesion => IdEmpleado > 0;

        // ==========================
        //  Iniciar sesión (mínimo)
        // ==========================
        public static void Iniciar(int idEmpleado, string email)
        {
            IdEmpleado = idEmpleado;
            Email = email ?? "";

            // Si llamas a este Iniciar “mínimo”, lo demás queda como esté.
            // Recomendado: usar el Iniciar(...) completo de abajo.
        }

        // ==========================
        //  Iniciar sesión (completo)
        // ==========================
        public static void Iniciar(
            int idEmpleado,
            string email,
            int idDepartamento,
            string departamento,
            int idCargo,
            string cargo,
            string nombreCompleto = ""
        )
        {
            IdEmpleado = idEmpleado;
            Email = email ?? "";

            IdDepartamento = idDepartamento;
            Departamento = departamento ?? "";

            IdCargo = idCargo;
            Cargo = cargo ?? "";

            NombreCompleto = nombreCompleto ?? "";
        }

        // ==========================
        //  Cargar desde un objeto (por si tu LoginDAL devuelve info)
        // ==========================
        public static void CargarDesdeLoginInfo(
            int idEmpleado,
            string email,
            int idDepartamento,
            string departamento,
            int idCargo,
            string cargo,
            string nombreCompleto = ""
        )
        {
            Iniciar(idEmpleado, email, idDepartamento, departamento, idCargo, cargo, nombreCompleto);
        }

        // ==========================
        //  Validación de sesión
        // ==========================
        public static void AsegurarSesion()
        {
            if (!HaySesion)
                throw new InvalidOperationException("No hay sesión iniciada. Debe iniciar sesión primero.");
        }

        // ==========================
        //  Cerrar sesión
        // ==========================
        public static void Cerrar()
        {
            IdEmpleado = 0;
            Email = "";

            IdDepartamento = 0;
            Departamento = "";

            IdCargo = 0;
            Cargo = "";

            NombreCompleto = "";
        }
    }
}

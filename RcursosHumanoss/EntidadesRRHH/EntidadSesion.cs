using System;

namespace RcursosHumanoss.SesionRRHH
{
    internal static class EntidadSesion
    {
        // ==========================
        //  Datos de sesión
        // ==========================
        public static int IdEmpleado { get; private set; }
        public static string Email { get; private set; } = "";

        public static int IdDepartamento { get; private set; }
        public static int IdCargo { get; private set; }

        public static string Departamento { get; private set; } = "";
        public static string Cargo { get; private set; } = "";

        public static string NombreCompleto { get; private set; } = "";

        public static bool HaySesion => IdEmpleado > 0;

        // ==========================
        //  Iniciar sesión
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
        //  Validación
        // ==========================
        public static void AsegurarSesion()
        {
            if (!HaySesion)
                throw new InvalidOperationException("No hay sesión iniciada. Inicie sesión primero.");
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

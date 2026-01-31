using System;

namespace RcursosHumanoss.EntidadesRRHH
{
    internal sealed class EntidadLogin
    {
        public int IdEmpleado { get; set; }
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";

        public int IdDepartamento { get; set; }
        public string Departamento { get; set; } = "";

        public int IdCargo { get; set; }
        public string Cargo { get; set; } = "";

        public string NombreCompleto { get; set; } = "";
    }
}

namespace RcursosHumanoss.EntidadesRRHH
{
    internal class EntidadLoginInfo
    {
        public int IdEmpleado { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string Cargo { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;

        public int Estado { get; set; } // 1 = Activo, 2 = Inactivo
    }
}

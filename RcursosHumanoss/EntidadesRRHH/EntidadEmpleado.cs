using System;

namespace RcursosHumanoss.EntidadesRRHH
{
    internal class EntidadEmpleado
    {
        // ====== Campos de la tabla Empleado ======

        public int IdEmpleado { get; set; }

        // Obligatorios (NOT NULL en DB)
        public string Nombres { get; set; } = string.Empty;

        public string PrimerApellido { get; set; } = string.Empty;

        public string CI { get; set; } = string.Empty;

        public DateTime FechaNacimiento { get; set; }

        public string Telefono { get; set; } = string.Empty;

        public int IdDepartamento { get; set; }

        public int IdCargo { get; set; }

        // Opcional (NULL permitido en DB)
        public string? SegundoApellido { get; set; }

        // ====== Campos de apoyo (JOIN / UI) ======
        // NO existen físicamente en la tabla Empleado
        public string? DepartamentoNombre { get; set; }

        public string? CargoNombre { get; set; }

        public string? EstadoActual { get; set; }

        public string? Email { get; set; }
    }
}

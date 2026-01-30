using System;

namespace RcursosHumanoss.EntidadesRRHH
{
    internal class EntidadVacacion
    {
        // Tabla Vacaciones
        public int IdVacaciones { get; set; }
        public int IdEmpleado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // Campos de apoyo (JOIN)
        public string CI { get; set; }
        public string Nombres { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string DepartamentoNombre { get; set; }
        public string CargoNombre { get; set; }

        public string EmpleadoNombreCompleto
        {
            get
            {
                string sa = string.IsNullOrWhiteSpace(SegundoApellido) ? "" : " " + SegundoApellido;
                return $"{Nombres} {PrimerApellido}{sa}".Trim();
            }
        }
    }
}

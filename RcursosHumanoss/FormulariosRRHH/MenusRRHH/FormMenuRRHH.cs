using System;
using System.Windows.Forms;

// Empleados
using RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.EmpleadoCRUD;
using RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.DetalleEmpleado;

// Vacaciones
using RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.VacionesCRUD;
using RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.DetalleVacacion;

namespace RcursosHumanoss.FormulariosRRHH.MenusRRHH
{
    public partial class FormMenuRRHH : Form
    {
        public FormMenuRRHH()
        {
            InitializeComponent();

            // Textos de botones
            button1.Text = "Empleados";
            button2.Text = "Vacaciones";
            button3.Text = "Mi Información";
            button4.Text = "Mis Vacaciones";

            // Eventos
            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
            button3.Click += Button3_Click;
            button4.Click += Button4_Click;
        }

        private void FormMenuRRHH_Load(object sender, EventArgs e)
        {
        }

        // =========================
        // Navegación
        // =========================

        // Empleado.cs
        private void Button1_Click(object sender, EventArgs e)
        {
            var frm = new Empleado();
            AbrirFormulario(frm);
        }

        // Vacacion.cs
        private void Button2_Click(object sender, EventArgs e)
        {
            var frm = new Vacacion();
            AbrirFormulario(frm);
        }

        // EmpleadoInformación.cs
        private void Button3_Click(object sender, EventArgs e)
        {
            var frm = new EmpleadoInformacion();
            this.Hide();
            frm.FormClosed += (s, args) => this.Show(); // cuando cierre, vuelve al menú
            frm.Show();
        }

        // VacacionInformacion.cs
        private void Button4_Click(object sender, EventArgs e)
        {
            var frm = new VacacionInformacion();
            this.Hide();
            frm.FormClosed += (s, args) => this.Show(); // cuando cierre, vuelve al menú
            frm.Show();
        }

        // =========================
        // Helper común
        // =========================
        private void AbrirFormulario(Form frm)
        {
            Hide();
            frm.FormClosed += (_, __) => Show();
            frm.Show();
        }

    }
}

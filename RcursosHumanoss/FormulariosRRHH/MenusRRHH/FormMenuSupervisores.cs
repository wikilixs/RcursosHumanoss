using RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.DetalleEmpleado;
using RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.EmpleadoCRUD;
using RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.DetalleVacacion;
using RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.VacacionCRUD;
using System;
using System.Windows.Forms;

namespace RcursosHumanoss.FormulariosRRHH.MenusRRHH
{
    public partial class FormMenuSupervisores : Form
    {
        public FormMenuSupervisores()
        {
            InitializeComponent();
            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
            button3.Click += Button3_Click;
            button4.Click += Button4_Click;
        }

        private void Button4_Click(object? sender, EventArgs e)
        {
            var frm = new VacacionInformacion();
            this.Hide();
            frm.FormClosed += (s, args) => this.Show(); // cuando cierre, vuelve al menú
            frm.Show();
        }

        private void Button3_Click(object? sender, EventArgs e)
        {
            var frm = new EmpleadoInformacion();
            this.Hide();
            frm.FormClosed += (s, args) => this.Show(); // cuando cierre, vuelve al menú
            frm.Show();
        }

        private void Button2_Click(object? sender, EventArgs e)
        {
            var frm = new VacacionArea();
            this.Hide();
            frm.FormClosed += (s, args) => this.Show(); // cuando cierre, vuelve al menú
            frm.Show();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var frm = new EmpleadoArea();

            this.Hide();
            frm.FormClosed += (s, args) => this.Show(); // cuando cierre, vuelve al menú
            frm.Show();
        }

    }
}

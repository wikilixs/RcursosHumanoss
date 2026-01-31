using System;
using System.Data;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.DetalleEmpleado;
using RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.DetalleVacacion;


namespace RcursosHumanoss.FormulariosRRHH.MenusRRHH
{
    public partial class FormMenuEmpleado : Form
    {
        public FormMenuEmpleado()
        {
            InitializeComponent();
            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
        }

        private void Button2_Click(object? sender, EventArgs e)
        {
            var frm = new VacacionInformacion();
            this.Hide();
            frm.FormClosed += (s, args) => this.Show(); // cuando cierre, vuelve al menú
            frm.Show();
        }

        private void Button1_Click(object? sender, EventArgs e)
        {
            var frm = new EmpleadoInformacion();
            this.Hide();
            frm.FormClosed += (s, args) => this.Show(); // cuando cierre, vuelve al menú
            frm.Show();
        }

    }
}

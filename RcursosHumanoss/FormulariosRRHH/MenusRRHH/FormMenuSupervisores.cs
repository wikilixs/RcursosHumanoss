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
            button1.Click += button1_Click;
            button2.Click += Button2_Click;
        }

        private void Button2_Click(object? sender, EventArgs e)
        {
            var frm = new VacacionInfromacion();
            this.Hide();
            frm.FormClosed += (s, args) => this.Show(); // cuando cierre, vuelve al menú
            frm.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var frm = new EmpleadoInformacion();

            this.Hide();
            frm.FormClosed += (s, args) => this.Show(); // cuando cierre, vuelve al menú
            frm.Show();
        }
    }
}

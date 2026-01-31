using System;
using System.Data;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.SesionRRHH;

namespace RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.DetalleVacacion
{
    public partial class VacacionInformacion : Form
    {
        public VacacionInformacion()
        {
            InitializeComponent();

            Load += VacacionInformacion_Load;
            button1.Click += Button1_Click;
        }

        private void Button1_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void VacacionInformacion_Load(object sender, EventArgs e)
        {
            try
            {
                EntidadSesion.AsegurarSesion();

                button1.Text = "Salir";

                dataGridView1.AutoGenerateColumns = true;
                dataGridView1.ReadOnly = true;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                CargarMisVacaciones();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando mis vacaciones:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void CargarMisVacaciones()
        {
            if (EntidadSesion.IdEmpleado <= 0)
                throw new InvalidOperationException("IdEmpleado de sesión inválido.");

            DataTable dt = VacacionInfoDAL.ObtenerMisVacaciones(EntidadSesion.IdEmpleado);

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = dt;

            // Opcional: ocultar IdVacaciones si no quieres verlo
            if (dataGridView1.Columns["IdVacaciones"] != null)
                dataGridView1.Columns["IdVacaciones"].Visible = false;
        }
    }
}

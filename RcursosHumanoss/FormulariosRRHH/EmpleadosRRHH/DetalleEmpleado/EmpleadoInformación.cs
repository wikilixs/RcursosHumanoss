using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.EntidadesRRHH;
using RcursosHumanoss.FormulariosRRHH.MenusRRHH;
using RcursosHumanoss.SesionRRHH;
using System;
using System.Data;
using System.Windows.Forms;

namespace RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.DetalleEmpleado
{
    public partial class EmpleadoInformación : Form
    {
        public EmpleadoInformación()
        {
            InitializeComponent();

            // Sin tocar el Designer
            Load += EmpleadoInformación_Load;
            button3.Click += Button3_Click;
        }

        private void EmpleadoInformación_Load(object? sender, EventArgs e)
        {
            ConfigurarUI();
            CargarMiInformacion();
        }

        private void ConfigurarUI()
        {
            button3.Text = "Volver";

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
        }

        private void CargarMiInformacion()
        {
            if (!SesionActual.HaySesion)
            {
                MessageBox.Show("No hay sesión iniciada. Vuelva a iniciar sesión.", "Sesión",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            try
            {
                DataTable dt = EmpleadoInfoDAL.ObtenerMiInformacion(SesionActual.IdEmpleado);

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dt;

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontró información del empleado.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando información:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Button3_Click(object? sender, EventArgs e)
        {
            // Volver a menú RRHH (ajusta si quieres volver a Supervisor/Empleado según sesión)
            var menu = new FormMenuRRHH();
            Hide();
            menu.FormClosed += (s, args) => Close();
            menu.Show();
        }
    }
}

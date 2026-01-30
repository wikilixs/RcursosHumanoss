// Si usas SesionActual:
using RcursosHumanoss;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.EntidadesRRHH;
using RcursosHumanoss.SesionRRHH;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.DetalleVacacion
{
    public partial class VacacionInformacion : Form
    {
        public VacacionInformacion()
        {
            InitializeComponent();

            // Eventos sin tocar Designer
            Load += VacacionInformacion_Load;
            button5.Click += Button5_Click;
        }

        private void VacacionInformacion_Load(object? sender, EventArgs e)
        {
            ConfigurarUI();
            CargarVacacionesDelEmpleadoLogueado();
        }

        private void ConfigurarUI()
        {
            button5.Text = "Volver";

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
        }

        private void CargarVacacionesDelEmpleadoLogueado()
        {
            try
            {
                int idEmpleado = SesionActual.IdEmpleado;

                if (idEmpleado <= 0)
                {
                    MessageBox.Show("No se encontró el empleado en sesión. Vuelva a iniciar sesión.",
                        "Sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                List<EntidadVacacion> lista = VacacionDAL.ListarPorEmpleado(idEmpleado);

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = lista;

                // Opcional: ocultar ids
                if (dataGridView1.Columns["IdVacaciones"] != null)
                    dataGridView1.Columns["IdVacaciones"].Visible = false;

                if (dataGridView1.Columns["IdEmpleado"] != null)
                    dataGridView1.Columns["IdEmpleado"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando vacaciones:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Button5_Click(object? sender, EventArgs e)
        {
            // Si lo abres con ShowDialog() esto basta:
            Close();
        }
    }
}

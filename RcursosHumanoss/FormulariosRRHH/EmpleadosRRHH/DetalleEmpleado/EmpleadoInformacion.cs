using System;
using System.Data;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.SesionRRHH;

namespace RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.DetalleEmpleado
{
    public partial class EmpleadoInformacion : Form
    {
        public EmpleadoInformacion()
        {
            InitializeComponent();

            // NO tocar Designer: solo eventos aquí
            Load += EmpleadoInformación_Load;
            button1.Click += Button1_Click; // Refrescar
            button2.Click += Button2_Click; // Salir
        }

        private void EmpleadoInformación_Load(object sender, EventArgs e)
        {
            try
            {
                // Validar sesión
                EntidadSesion.AsegurarSesion();

                // Textos UI
                button1.Text = "Refrescar";
                button2.Text = "Salir";

                // Grid config
                dataGridView1.AutoGenerateColumns = true;
                dataGridView1.ReadOnly = true;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;

                CargarMiInformacion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando la información:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                EntidadSesion.AsegurarSesion();
                CargarMiInformacion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al refrescar:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            // ✅ Para que NO abra otra pantalla: solo cerrar
            Close();
        }

        private void CargarMiInformacion()
        {
            if (EntidadSesion.IdEmpleado <= 0)
                throw new InvalidOperationException("IdEmpleado de sesión inválido.");

            DataTable dt = EmpleadoInfoDAL.ObtenerMiInformacion(EntidadSesion.IdEmpleado);

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = dt;
        }
    }
}

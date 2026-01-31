using System;
using System.Collections.Generic;
using System.Windows.Forms;

using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.EntidadesRRHH;
using RcursosHumanoss.SesionRRHH;
using RcursosHumanoss.FormulariosRRHH.MenusRRHH;

namespace RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.EmpleadoCRUD
{
    public partial class EmpleadoArea : Form
    {
        public EmpleadoArea()
        {
            InitializeComponent();

            // ✅ Enganchar eventos SIN tocar Designer
            Load += EmpleadoArea_Load;

            button1.Click += Button1_Buscar_Click;   // Buscar
            button2.Click += Button2_Limpiar_Click;  // Limpiar
            button3.Click += Button3_Volver_Click;   // Volver
            button4.Click += Button4_Salir_Click;    // Salir
        }

        private void EmpleadoArea_Load(object sender, EventArgs e)
        {
            ConfigurarUI();
            CargarCriterios();
            CargarUltimosActivosMiDepartamento();
        }

        private void ConfigurarUI()
        {
            button1.Text = "Buscar";
            button2.Text = "Limpiar";
            button3.Text = "Volver";
            button4.Text = "Salir";

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
        }

        private void CargarCriterios()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("CI");
            comboBox1.Items.Add("Nombres");
            comboBox1.Items.Add("Apellidos");
            comboBox1.SelectedIndex = 0;
        }

        // ============================
        // CARGA INICIAL
        // ============================
        private void CargarUltimosActivosMiDepartamento()
        {
            try
            {
                int idDep = EntidadSesion.IdDepartamento;

                if (idDep <= 0)
                {
                    MessageBox.Show("No se encontró el departamento del usuario en sesión.\n" +
                                    "Verifica que el Login esté cargando EnridadSesion.IdDepartamento.",
                        "Sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ✅ Estos métodos deben existir en tu EmpleadoDAL
                List<EntidadEmpleado> lista = EmpleadoDAL.ListarUltimosActivosPorDepartamento(20, idDep);
                PintarGrid(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando empleados del área:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================
        // BUSCAR
        // ============================
        private void Button1_Buscar_Click(object sender, EventArgs e)
        {
            string criterio = comboBox1.SelectedItem?.ToString() ?? "";
            string valor = (textBox1.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(valor))
            {
                MessageBox.Show("Ingrese un valor para buscar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int idDep = EntidadSesion.IdDepartamento;

                if (idDep <= 0)
                {
                    MessageBox.Show("No se encontró el departamento del usuario en sesión.",
                        "Sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ✅ Solo busca dentro del departamento del supervisor
                List<EntidadEmpleado> lista = EmpleadoDAL.BuscarEnDepartamento(criterio, valor, idDep);
                PintarGrid(lista);

                if (lista.Count == 0)
                {
                    MessageBox.Show("No se encontraron empleados en tu área con ese criterio.",
                        "Búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la búsqueda:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================
        // LIMPIAR
        // ============================
        private void Button2_Limpiar_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            comboBox1.SelectedIndex = 0;
            CargarUltimosActivosMiDepartamento();
        }

        // ============================
        // VOLVER (NO abre otra pantalla)
        // ============================
        private void Button3_Volver_Click(object sender, EventArgs e)
        {
            // ✅ Solución al problema típico: "volver me abre otra pantalla"
            // Aquí SOLO cerramos este form.
            Close();
        }

        // ============================
        // SALIR
        // ============================
        private void Button4_Salir_Click(object sender, EventArgs e)
        {
            Close();
        }

        // ============================
        // GRID
        // ============================
        private void PintarGrid(List<EntidadEmpleado> lista)
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = lista;

            if (dataGridView1.Columns["IdEmpleado"] != null)
                dataGridView1.Columns["IdEmpleado"].Visible = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.EntidadesRRHH;
using RcursosHumanoss.FormulariosRRHH.MenusRRHH;

namespace RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.EmpleadoCRUD
{
    public partial class Empleado : Form
    {
        public Empleado()
        {
            InitializeComponent();

            button1.Click += button1_Click; // Buscar
            button2.Click += button2_Click; // Limpiar

            button4.Click += button4_Click; // Agregar
            button5.Click += button5_Click; // Actualizar
            button3.Click += button3_Click; // Eliminar

            button6.Click += button6_Click; // Volver
        }

        private void Empleado_Load(object sender, EventArgs e)
        {
            ConfigurarUI();
            CargarCriterios();
            CargarInicial();
        }

        // =========================================================
        // CONFIGURACIÓN UI
        // =========================================================
        private void ConfigurarUI()
        {
            button1.Text = "Buscar";
            button2.Text = "Limpiar";

            button4.Text = "Agregar";
            button5.Text = "Actualizar";
            button3.Text = "Eliminar";

            button6.Text = "Volver";

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

            comboBox1.Items.Add("Ver Activos");
            comboBox1.Items.Add("Ver Inactivos");
            comboBox1.Items.Add("Ver Todos");

            comboBox1.SelectedIndex = 0;
        }

        // =========================================================
        // CARGA INICIAL
        // =========================================================
        private void CargarInicial()
        {
            try
            {
                var lista = GerenteEmpleadoDAL.ListarUltimos(20, "Todos");
                PintarGrid(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando empleados:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // BOTÓN BUSCAR
        // =========================================================
        private void button1_Click(object sender, EventArgs e)
        {
            string criterio = comboBox1.SelectedItem?.ToString() ?? "";
            string valor = (textBox1.Text ?? "").Trim();

            try
            {
                // =============================
                // FILTROS DE ESTADO
                // =============================
                if (criterio.Equals("Ver Activos", StringComparison.OrdinalIgnoreCase))
                {
                    PintarGrid(GerenteEmpleadoDAL.ListarUltimos(200, "Activo"));
                    return;
                }

                if (criterio.Equals("Ver Inactivos", StringComparison.OrdinalIgnoreCase))
                {
                    PintarGrid(GerenteEmpleadoDAL.ListarUltimos(200, "Inactivo"));
                    return;
                }

                if (criterio.Equals("Ver Todos", StringComparison.OrdinalIgnoreCase))
                {
                    PintarGrid(GerenteEmpleadoDAL.ListarUltimos(200, "Todos"));
                    return;
                }

                // =============================
                // BÚSQUEDA NORMAL
                // =============================
                if (string.IsNullOrWhiteSpace(valor))
                {
                    MessageBox.Show("Ingrese un valor para buscar.",
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var resultado = GerenteEmpleadoDAL.Buscar(criterio, valor, "Todos");

                if (resultado.Count == 0)
                {
                    MessageBox.Show("No se encontraron coincidencias.",
                        "Búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                PintarGrid(resultado);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en búsqueda:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // BOTÓN LIMPIAR
        // =========================================================
        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            comboBox1.SelectedIndex = 0;
            CargarInicial();
        }

        // =========================================================
        // CRUD
        // =========================================================
        private void button4_Click(object sender, EventArgs e)
        {
            using (var frm = new EmpleadoAgregar())
            {
                frm.ShowDialog();
            }
            CargarInicial();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (var frm = new EmpleadoActualizar())
            {
                frm.ShowDialog();
            }
            CargarInicial();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var frm = new EmpleadoEliminar())
            {
                frm.ShowDialog();
            }
            CargarInicial();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var menu = new FormMenuRRHH();
            Hide();
            menu.FormClosed += (s, args) => Close();
            menu.Show();
        }

        // =========================================================
        // GRID
        // =========================================================
        private void PintarGrid(List<EntidadEmpleado> lista)
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = lista;

            if (dataGridView1.Columns["IdEmpleado"] != null)
                dataGridView1.Columns["IdEmpleado"].Visible = false;
        }
    }
}

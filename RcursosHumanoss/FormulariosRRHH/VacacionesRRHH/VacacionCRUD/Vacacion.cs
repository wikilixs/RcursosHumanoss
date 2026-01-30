using System;
using System.Collections.Generic;
using System.Windows.Forms;

using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.EntidadesRRHH;
using RcursosHumanoss.FormulariosRRHH.MenusRRHH;

// Ajusta si tus forms están en otros namespaces:
using RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.VacacionCRUD; // VacacionAgregar / VacacionActualizar

namespace RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.VacionesCRUD
{
    public partial class Vacacion : Form
    {
        public Vacacion()
        {
            InitializeComponent();

            // No tocar Designer: asigno eventos aquí
            button1.Click += Button1_Click; // Buscar
            button2.Click += Button2_Click; // Limpiar
            button3.Click += Button3_Click; // Agregar
            button4.Click += Button4_Click; // Actualizar
            button5.Click += Button5_Click; // Volver
        }

        private void Vacaciones_Load(object sender, EventArgs e)
        {
            ConfigurarUI();
            CargarCriterios();
            CargarUltimasVacaciones();
        }

        // ==========================================
        // UI
        // ==========================================
        private void ConfigurarUI()
        {
            button1.Text = "Buscar";
            button2.Text = "Limpiar";
            button3.Text = "Agregar";
            button4.Text = "Actualizar";
            button5.Text = "Volver";

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

        // ==========================================
        // Carga inicial
        // ==========================================
        private void CargarUltimasVacaciones()
        {
            try
            {
                var lista = VacacionDAL.ListarUltimosConVacaciones(20);
                PintarGrid(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando vacaciones:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // Buscar / Limpiar
        // ==========================================
        private void Button1_Click(object sender, EventArgs e)
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
                var lista = VacacionDAL.Buscar(criterio, valor);
                PintarGrid(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en búsqueda:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            comboBox1.SelectedIndex = 0;
            CargarUltimasVacaciones();
        }

        // ==========================================
        // Navegación
        // ==========================================
        private void Button3_Click(object sender, EventArgs e)
        {
            // Abrir Agregar Vacación
            var frm = new VacacionAgregar();
            frm.ShowDialog();
            CargarUltimasVacaciones();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            // Abrir Actualizar Vacación
            var frm = new VacacionActualizar();
            frm.ShowDialog();
            CargarUltimasVacaciones();
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            // Volver al menú RRHH
            var menu = new FormMenuRRHH();
            Hide();
            menu.FormClosed += (_, __) => Close();
            menu.Show();
        }

        // ==========================================
        // Grid
        // ==========================================
        private void PintarGrid(List<EntidadVacacion> lista)
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = lista;

            // Ocultar IDs si no quieres mostrar
            if (dataGridView1.Columns["IdVacaciones"] != null)
                dataGridView1.Columns["IdVacaciones"].Visible = false;

            if (dataGridView1.Columns["IdEmpleado"] != null)
                dataGridView1.Columns["IdEmpleado"].Visible = false;
        }
    }
}

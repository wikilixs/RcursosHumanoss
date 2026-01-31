using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.EntidadesRRHH;
using RcursosHumanoss.SesionRRHH;

namespace RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.VacacionCRUD
{
    public partial class VacacionInfromacion : Form
    {
        public VacacionInfromacion()
        {
            InitializeComponent();

            // Eventos sin tocar el Designer
            Load += VacacionInfromacion_Load;

            button1.Click += Button1_Click; // Buscar
            button2.Click += Button2_Click; // Limpiar
            button4.Click += Button4_Click; // Salir

            // button3 no hace nada (lo dejamos quieto)
        }

        private void VacacionInfromacion_Load(object sender, EventArgs e)
        {
            ConfigurarUI();
            CargarCriterios();
            CargarUltimosMiDepartamento();
        }

        private void ConfigurarUI()
        {
            button1.Text = "Buscar";
            button2.Text = "Limpiar";
            button3.Text = "";     // no hace nada
            button3.Enabled = false; // opcional, para que no moleste
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

        // ============================================
        //  CARGA INICIAL: últimos con vacaciones
        // ============================================
        private void CargarUltimosMiDepartamento()
        {
            try
            {
                int idDep = EntidadSesion.IdDepartamento;

                if (idDep <= 0)
                {
                    MessageBox.Show("No se detectó el IdDepartamento en sesión.",
                        "Sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dataGridView1.DataSource = null;
                    return;
                }

                var lista = VacacionDAL.ListarUltimosConVacacionesPorDepartamento(20, idDep);
                PintarGrid(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando vacaciones:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        //  BUTTON 1: BUSCAR
        // ============================================
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
                int idDep = EntidadSesion.IdDepartamento;

                if (idDep <= 0)
                {
                    MessageBox.Show("No se detectó el IdDepartamento en sesión.",
                        "Sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var lista = VacacionDAL.BuscarPorDepartamento(criterio, valor, idDep);
                PintarGrid(lista);

                if (lista.Count == 0)
                {
                    MessageBox.Show("No se encontraron registros.", "Búsqueda",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en búsqueda:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        //  BUTTON 2: LIMPIAR
        // ============================================
        private void Button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            comboBox1.SelectedIndex = 0;
            CargarUltimosMiDepartamento();
        }

        // ============================================
        //  BUTTON 4: SALIR
        // ============================================
        private void Button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PintarGrid(List<EntidadVacacion> lista)
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = lista;

            // Ocultar IDs si no quieres mostrarlos
            if (dataGridView1.Columns["IdVacaciones"] != null)
                dataGridView1.Columns["IdVacaciones"].Visible = false;

            if (dataGridView1.Columns["IdEmpleado"] != null)
                dataGridView1.Columns["IdEmpleado"].Visible = false;
        }
    }
}

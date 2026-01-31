using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.EntidadesRRHH;

namespace RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.EmpleadoCRUD
{
    public partial class EmpleadoInformacion : Form
    {
        public EmpleadoInformacion()
        {
            InitializeComponent();

            // Eventos SIN tocar Designer
            Load += EmpleadoInformacion_Load;

            button1.Click += Button1_Click; // Buscar
            button2.Click += Button2_Click; // Limpiar
            button4.Click += Button4_Click; // Salir

            // Button3 no hace nada (NO asigno evento)
        }

        private void EmpleadoInformacion_Load(object sender, EventArgs e)
        {
            ConfigurarUI();
            CargarCriterios();
            CargarUltimosMiDepartamento();
        }

        // =========================
        // UI
        // =========================
        private void ConfigurarUI()
        {
            button1.Text = "Buscar";
            button2.Text = "Limpiar";
            button3.Text = "(Sin acción)";
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

        // =========================
        // Carga inicial (mi depto)
        // =========================
        private void CargarUltimosMiDepartamento()
        {
            try
            {
                int idDep = SesionHelper.ObtenerIdDepartamento();

                if (idDep <= 0)
                {
                    MessageBox.Show(
                        "No se encontró el IdDepartamento en la sesión.\n" +
                        "Asegúrate de cargar IdDepartamento cuando el usuario hace login.",
                        "Sesión",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    dataGridView1.DataSource = null;
                    return;
                }

                var lista = EmpleadoDAL.ListarUltimosActivosPorDepartamento(20); // toma depto desde sesión
                PintarGrid(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando empleados:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // Buscar
        // =========================
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
                int idDep = SesionHelper.ObtenerIdDepartamento();
                if (idDep <= 0)
                {
                    MessageBox.Show("No se encontró el IdDepartamento en la sesión.", "Sesión",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Solo dentro del departamento del supervisor
                var lista = EmpleadoDAL.BuscarEnDepartamento(criterio, valor); // filtra con sesión
                PintarGrid(lista);

                if (lista.Count == 0)
                {
                    MessageBox.Show("No se encontraron empleados con ese criterio.", "Búsqueda",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la búsqueda:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // Limpiar
        // =========================
        private void Button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            comboBox1.SelectedIndex = 0;
            CargarUltimosMiDepartamento();
        }

        // =========================
        // Salir
        // =========================
        private void Button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        // =========================
        // Grid
        // =========================
        private void PintarGrid(List<EntidadEmpleado> lista)
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = lista;

            // Ocultar Id si no quieres mostrarlo
            if (dataGridView1.Columns["IdEmpleado"] != null)
                dataGridView1.Columns["IdEmpleado"].Visible = false;
        }

        // (Tu handler del designer, lo dejo vacío para no romper nada)
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        // =========================
        // Helper de sesión (no depende del nombre exacto)
        // =========================
        private static class SesionHelper
        {
            public static int ObtenerIdDepartamento()
            {
                // Orden: EnridadSesion -> EntidadSesion -> SesionActual
                int id = TryGetStaticInt("RcursosHumanoss.SesionRRHH.EnridadSesion", "IdDepartamento");
                if (id > 0) return id;

                id = TryGetStaticInt("RcursosHumanoss.SesionRRHH.EntidadSesion", "IdDepartamento");
                if (id > 0) return id;

                id = TryGetStaticInt("RcursosHumanoss.SesionRRHH.SesionActual", "IdDepartamento");
                return id;
            }

            private static int TryGetStaticInt(string fullTypeName, string propName)
            {
                try
                {
                    Type t = Type.GetType(fullTypeName);
                    if (t == null) return 0;

                    var p = t.GetProperty(propName,
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Static);

                    if (p == null) return 0;

                    object v = p.GetValue(null);
                    if (v == null) return 0;

                    return Convert.ToInt32(v);
                }
                catch
                {
                    return 0;
                }
            }
        }
    }
}

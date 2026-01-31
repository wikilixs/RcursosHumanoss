using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.EntidadesRRHH;

// Ajusta si tu listado principal está en otro namespace
using RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.VacionesCRUD;

namespace RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.VacacionCRUD
{
    public partial class VacacionActualizar : Form
    {
        private int _idVacacionesSeleccionada = 0;
        private int _idEmpleadoSeleccionado = 0;

        public VacacionActualizar()
        {
            InitializeComponent();

            // Eventos sin tocar Designer
            button1.Click += Button1_Click; // Buscar
            button2.Click += Button2_Click; // Limpiar
            button4.Click += Button4_Click; // Actualizar
            button3.Click += button3_Click; // Volver (ya está en Designer)

            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ConfigurarUI();
            CargarCriterios();
            PrepararFechas();
            CargarUltimosConVacaciones();
        }

        private void ConfigurarUI()
        {
            button1.Text = "Buscar";
            button2.Text = "Limpiar";
            button4.Text = "Actualizar";
            button3.Text = "Volver";

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

        private void PrepararFechas()
        {
            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker2.Value = DateTime.Today;
        }

        private void CargarUltimosConVacaciones()
        {
            try
            {
                var lista = VacacionDAL.ListarUltimosConVacaciones(20);
                PintarGrid(lista);
                LimpiarSeleccion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando vacaciones:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // Buscar
        // =========================
        private void Button1_Click(object? sender, EventArgs e)
        {
            string criterio = comboBox1.SelectedItem?.ToString() ?? "";
            string valor = (textBox1.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(valor))
            {
                MessageBox.Show("Ingrese un valor para buscar (CI/Nombres/Apellidos).", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var lista = VacacionDAL.Buscar(criterio, valor);
                PintarGrid(lista);
                LimpiarSeleccion();

                if (lista.Count == 0)
                {
                    MessageBox.Show("No se encontraron vacaciones con ese criterio.", "Búsqueda",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en búsqueda:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // Limpiar
        // =========================
        private void Button2_Click(object? sender, EventArgs e)
        {
            textBox1.Clear();
            comboBox1.SelectedIndex = 0;
            PrepararFechas();
            CargarUltimosConVacaciones();
        }

        // =========================
        // Selección en grid
        // =========================
        private void DataGridView1_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                var row = dataGridView1.Rows[e.RowIndex];

                _idVacacionesSeleccionada = Convert.ToInt32(row.Cells["IdVacaciones"].Value);
                _idEmpleadoSeleccionado = Convert.ToInt32(row.Cells["IdEmpleado"].Value);

                dateTimePicker1.Value = Convert.ToDateTime(row.Cells["FechaInicio"].Value);
                dateTimePicker2.Value = Convert.ToDateTime(row.Cells["FechaFin"].Value);
            }
            catch
            {
                LimpiarSeleccion();
            }
        }

        private void LimpiarSeleccion()
        {
            _idVacacionesSeleccionada = 0;
            _idEmpleadoSeleccionado = 0;
        }

        // =========================
        // Actualizar
        // =========================
        private void Button4_Click(object? sender, EventArgs e)
        {
            if (_idVacacionesSeleccionada <= 0)
            {
                MessageBox.Show("Seleccione una vacación del listado para actualizar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime inicio = dateTimePicker1.Value.Date;
            DateTime fin = dateTimePicker2.Value.Date;

            if (fin < inicio)
            {
                MessageBox.Show("La fecha fin no puede ser menor que la fecha inicio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Evita solapamiento con otras vacaciones del mismo empleado (excluyendo la actual)
                if (VacacionDAL.ExisteSolapamientoExcepto(_idEmpleadoSeleccionado, _idVacacionesSeleccionada, inicio, fin))
                {
                    MessageBox.Show("Las fechas se solapan con otra vacación del empleado.", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool ok = VacacionDAL.Actualizar(_idVacacionesSeleccionada, inicio, fin);

                if (ok)
                {
                    MessageBox.Show("Vacación actualizada correctamente.", "OK",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    CargarUltimosConVacaciones();
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar la vacación.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // Volver (Designer ya tiene button3_Click asignado)
        // =========================
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
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

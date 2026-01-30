using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.EntidadesRRHH;

// Ajusta si tu form de lista se llama diferente o está en otro namespace
using RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.VacionesCRUD;

namespace RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.VacacionCRUD
{
    public partial class VacacionAgregar : Form
    {
        private int _idEmpleadoSeleccionado = 0;

        public VacacionAgregar()
        {
            InitializeComponent();

            // Eventos (sin tocar Designer)
            button1.Click += button1_Click; // Agregar
            button2.Click += button2_Click; // Limpiar
            button3.Click += button3_Click; // Volver
            button4.Click += button4_Click; // (opcional) Limpiar o Volver

            dataGridView1.CellClick += dataGridView1_CellClick;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ConfigurarUI();
            CargarCriterios();
            PrepararFechas();
            LimpiarSeleccion();
        }

        private void ConfigurarUI()
        {
            // Textos
            button1.Text = "Agregar";
            button2.Text = "Limpiar";
            button3.Text = "Volver";
            button4.Text = "Limpiar selección";

            // Grid
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
            dateTimePicker1.MinDate = new DateTime(1900, 1, 1);
            dateTimePicker2.MinDate = new DateTime(1900, 1, 1);
        }

        // =========================================
        // BUSCAR (usando el textbox + comboBox)
        // =========================================
        private void button4_Click(object sender, EventArgs e)
        {
            // "Limpiar selección" (puedes cambiarlo a "Buscar" si deseas)
            LimpiarSeleccion();
        }

        // Si quieres que button4 sea BUSCAR y no limpiar:
        // cambia arriba: button4.Text = "Buscar";
        // y en este método pega el contenido de BuscarEmpleado()

        private void BuscarEmpleado()
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
                // VacacionDAL.Buscar devuelve lista de vacaciones + datos de empleado.
                // Pero para seleccionar empleado, nos sirve porque trae IdEmpleado.
                List<EntidadVacacion> lista = VacacionDAL.Buscar(criterio, valor);

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = lista;

                // Ocultar columnas si quieres
                OcultarColumnasNoNecesarias();

                if (lista.Count == 0)
                {
                    MessageBox.Show("No se encontraron registros con ese criterio.", "Búsqueda",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en búsqueda:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OcultarColumnasNoNecesarias()
        {
            // Si no quieres ver IDs en UI
            if (dataGridView1.Columns["IdVacaciones"] != null)
                dataGridView1.Columns["IdVacaciones"].Visible = false;

            // El IdEmpleado puedes mostrarlo o no; igual se usa internamente
            // if (dataGridView1.Columns["IdEmpleado"] != null)
            //     dataGridView1.Columns["IdEmpleado"].Visible = false;
        }

        // =========================================
        // Seleccionar empleado desde el grid
        // =========================================
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                var row = dataGridView1.Rows[e.RowIndex];
                if (row.Cells["IdEmpleado"]?.Value == null) return;

                _idEmpleadoSeleccionado = Convert.ToInt32(row.Cells["IdEmpleado"].Value);

                // Si quieres mostrar algo en el textbox:
                // textBox1.Text = row.Cells["CI"]?.Value?.ToString();
            }
            catch
            {
                _idEmpleadoSeleccionado = 0;
            }
        }

        private void LimpiarSeleccion()
        {
            _idEmpleadoSeleccionado = 0;
            dataGridView1.DataSource = null;
        }

        // =========================================
        // AGREGAR VACACIÓN
        // =========================================
        private void button1_Click(object sender, EventArgs e)
        {
            // Si tu button2 es limpiar y button1 agregar, ok.
            // Si quieres que button1 busque y button4 agregue, dímelo y lo ajusto.

            if (_idEmpleadoSeleccionado <= 0)
            {
                // Si no seleccionó empleado, intentamos buscar por lo escrito
                // (puedes quitar esto si quieres obligar selección manual)
                BuscarEmpleado();

                MessageBox.Show("Seleccione un empleado del listado antes de agregar la vacación.", "Validación",
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
                // Validar solapamiento
                if (VacacionDAL.ExisteSolapamiento(_idEmpleadoSeleccionado, inicio, fin))
                {
                    MessageBox.Show("El empleado ya tiene vacaciones registradas que se solapan con esas fechas.",
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int id = VacacionDAL.Insertar(_idEmpleadoSeleccionado, inicio, fin);

                if (id > 0)
                {
                    MessageBox.Show("Vacación registrada correctamente.", "OK",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Recargar grid mostrando las del mismo empleado (opcional)
                    // BuscarEmpleado();
                    LimpiarCampos();
                }
                else
                {
                    MessageBox.Show("No se pudo registrar la vacación.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            textBox1.Clear();
            comboBox1.SelectedIndex = 0;
            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker2.Value = DateTime.Today;
            LimpiarSeleccion();
        }

        // =========================================
        // LIMPIAR
        // =========================================
        private void button2_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }

        // =========================================
        // VOLVER
        // =========================================
        private void button3_Click(object sender, EventArgs e)
        {
            // Vuelve al listado principal de vacaciones
            var frm = new Vacacion();
            Hide();
            frm.FormClosed += (s, args) => Close();
            frm.Show();
        }
    }
}

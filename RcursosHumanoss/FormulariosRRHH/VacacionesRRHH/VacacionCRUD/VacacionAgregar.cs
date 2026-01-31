using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.EntidadesRRHH;

namespace RcursosHumanoss.FormulariosRRHH.VacacionesRRHH.VacacionCRUD
{
    public partial class VacacionAgregar : Form
    {
        private int _idEmpleadoSeleccionado = 0;

        public VacacionAgregar()
        {
            InitializeComponent();

            // Eventos (sin tocar Designer)
            button1.Click += Button1_Click; // Buscar
            button2.Click += Button2_Click; // Limpiar
            button3.Click += Button3_Click; // Agregar
            button4.Click += Button4_Click; // Salir

            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ConfigurarUI();
            CargarCriterios();
            PrepararFechas();
            LimpiarSeleccion();
        }

        // =========================
        // UI
        // =========================
        private void ConfigurarUI()
        {
            button1.Text = "Buscar";
            button2.Text = "Limpiar";
            button3.Text = "Agregar";
            button4.Text = "Salir";

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;

            AcceptButton = button1; // Enter = Buscar
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

            // Evitar MinDate raro
            dateTimePicker1.MinDate = new DateTime(1900, 1, 1);
            dateTimePicker2.MinDate = new DateTime(1900, 1, 1);
        }

        // =========================
        // BUTTON 1: BUSCAR
        // =========================
        private void Button1_Click(object sender, EventArgs e)
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
                List<EntidadVacacion> lista = VacacionDAL.Buscar(criterio, valor);

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = lista;

                OcultarColumnasNoNecesarias();
                LimpiarSeleccion();

                if (lista.Count == 0)
                {
                    MessageBox.Show("No se encontraron registros.", "Búsqueda",
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
            // Ocultar IdVacaciones (puede venir vacío si el empleado no tiene vacaciones,
            // pero tu DAL busca en Vacaciones, así que normalmente viene)
            if (dataGridView1.Columns["IdVacaciones"] != null)
                dataGridView1.Columns["IdVacaciones"].Visible = false;

            // Si tampoco quieres ver IdEmpleado:
            // if (dataGridView1.Columns["IdEmpleado"] != null)
            //     dataGridView1.Columns["IdEmpleado"].Visible = false;
        }

        // =========================
        // CLICK EN GRID: selecciona empleado
        // =========================
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                var row = dataGridView1.Rows[e.RowIndex];

                // Necesario para registrar la vacación
                if (row.Cells["IdEmpleado"]?.Value == null)
                {
                    _idEmpleadoSeleccionado = 0;
                    return;
                }

                _idEmpleadoSeleccionado = Convert.ToInt32(row.Cells["IdEmpleado"].Value);

                // (Opcional) Puedes cargar en textbox1 el CI, si existe:
                // if (row.Cells["CI"]?.Value != null)
                //     textBox1.Text = row.Cells["CI"].Value.ToString();
            }
            catch
            {
                _idEmpleadoSeleccionado = 0;
            }
        }

        private void LimpiarSeleccion()
        {
            _idEmpleadoSeleccionado = 0;
        }

        // =========================
        // BUTTON 2: LIMPIAR
        // =========================
        private void Button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            comboBox1.SelectedIndex = 0;

            PrepararFechas();

            dataGridView1.DataSource = null;
            LimpiarSeleccion();
        }

        // =========================
        // BUTTON 3: AGREGAR
        // =========================
        private void Button3_Click(object sender, EventArgs e)
        {
            if (_idEmpleadoSeleccionado <= 0)
            {
                MessageBox.Show("Seleccione un empleado del listado (haga click en una fila).", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime inicio = dateTimePicker1.Value.Date;
            DateTime fin = dateTimePicker2.Value.Date;

            if (fin < inicio)
            {
                MessageBox.Show("La Fecha Fin no puede ser menor que la Fecha Inicio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Validar solapamiento antes de insertar
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

                    // Limpio después de agregar
                    Button2_Click(sender, e);
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

        // =========================
        // BUTTON 4: SALIR
        // =========================
        private void Button4_Click(object sender, EventArgs e)
        {
            // No abre otra pantalla: solo cierra este form
            this.Close();
        }
    }
}

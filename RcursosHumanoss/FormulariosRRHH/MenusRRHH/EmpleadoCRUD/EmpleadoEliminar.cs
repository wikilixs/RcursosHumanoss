using System;
using System.Data;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;

namespace RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.EmpleadoCRUD
{
    public partial class EmpleadoEliminar : Form
    {
        public EmpleadoEliminar()
        {
            InitializeComponent();
            WireUp();
        }

        private void WireUp()
        {
            Load += EmpleadoEliminar_Load;

            button1.Click += button1_Click; // Buscar
            button2.Click += button2_Click; // Limpiar
            button4.Click += button4_Click; // Inactivar
            button3.Click += button3_Click; // Volver
        }

        private void EmpleadoEliminar_Load(object sender, EventArgs e)
        {
            // Textos
            button1.Text = "Buscar";
            button2.Text = "Limpiar";
            button4.Text = "Inactivar";
            button3.Text = "Volver";

            // Combo
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Items.Clear();

            comboBox1.Items.Add("CI | Activos");
            comboBox1.Items.Add("Nombres | Activos");
            comboBox1.Items.Add("Apellidos | Activos");

            comboBox1.Items.Add("CI | Inactivos");
            comboBox1.Items.Add("Nombres | Inactivos");
            comboBox1.Items.Add("Apellidos | Inactivos");

            comboBox1.SelectedIndex = 0;

            ConfigurarGrid();

            // Carga inicial: Activos
            CargarUltimos(EliminarEmpleadoDAL.ESTADO_ACTIVO);
        }

        private void ConfigurarGrid()
        {
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
        }

        private void CargarUltimos(int idEstado)
        {
            DataTable dt = EliminarEmpleadoDAL.ListarUltimos(20, idEstado);
            dataGridView1.DataSource = dt;

            if (dataGridView1.Columns.Contains("IdEmpleado"))
                dataGridView1.Columns["IdEmpleado"].Visible = false;
        }

        // ==========================
        // BUSCAR
        // ==========================
        private void button1_Click(object sender, EventArgs e)
        {
            string item = comboBox1.SelectedItem?.ToString() ?? "CI | Activos";
            string texto = (textBox1.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                MessageBox.Show("Ingrese un valor para buscar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var parsed = ParseCombo(item);

            DataTable dt = EliminarEmpleadoDAL.Buscar(parsed.Campo, texto, parsed.IdEstado);
            dataGridView1.DataSource = dt;

            if (dataGridView1.Columns.Contains("IdEmpleado"))
                dataGridView1.Columns["IdEmpleado"].Visible = false;

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("No se encontraron resultados.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private (string Campo, int IdEstado) ParseCombo(string item)
        {
            // "CI | Activos"
            string[] partes = item.Split('|');
            string campo = partes.Length > 0 ? partes[0].Trim() : "CI";
            string estadoTxt = partes.Length > 1 ? partes[1].Trim().ToLowerInvariant() : "activos";

            int idEstado = estadoTxt.Contains("inact")
                ? EliminarEmpleadoDAL.ESTADO_INACTIVO
                : EliminarEmpleadoDAL.ESTADO_ACTIVO;

            if (campo.Equals("CI", StringComparison.OrdinalIgnoreCase)) campo = "CI";
            else if (campo.Equals("Nombres", StringComparison.OrdinalIgnoreCase)) campo = "Nombres";
            else campo = "Apellidos";

            return (campo, idEstado);
        }

        // ==========================
        // LIMPIAR
        // ==========================
        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            comboBox1.SelectedIndex = 0;
            CargarUltimos(EliminarEmpleadoDAL.ESTADO_ACTIVO);
            textBox1.Focus();
        }

        // ==========================
        // INACTIVAR
        // ==========================
        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un empleado de la lista.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            object idObj = dataGridView1.CurrentRow.Cells["IdEmpleado"]?.Value;
            if (idObj == null || !int.TryParse(idObj.ToString(), out int idEmpleado))
            {
                MessageBox.Show("No se pudo obtener el Id del empleado.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string nombre = dataGridView1.CurrentRow.Cells["Nombres"]?.Value?.ToString() ?? "";
            string pa = dataGridView1.CurrentRow.Cells["PrimerApellido"]?.Value?.ToString() ?? "";
            string sa = dataGridView1.CurrentRow.Cells["SegundoApellido"]?.Value?.ToString() ?? "";
            string ci = dataGridView1.CurrentRow.Cells["CI"]?.Value?.ToString() ?? "";

            var confirm = MessageBox.Show(
                $"¿Desea INACTIVAR a:\n{nombre} {pa} {sa}\nCI: {ci} ?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            bool ok = EliminarEmpleadoDAL.Inactivar(idEmpleado);

            if (!ok)
            {
                MessageBox.Show("No se pudo inactivar el empleado.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Empleado inactivado correctamente.", "OK",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // refrescar según combo actual
            var parsed = ParseCombo(comboBox1.SelectedItem?.ToString() ?? "CI | Activos");

            if (string.IsNullOrWhiteSpace(textBox1.Text.Trim()))
                CargarUltimos(parsed.IdEstado);
            else
                dataGridView1.DataSource = EliminarEmpleadoDAL.Buscar(parsed.Campo, textBox1.Text.Trim(), parsed.IdEstado);
        }

        // ==========================
        // VOLVER
        // ==========================
        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

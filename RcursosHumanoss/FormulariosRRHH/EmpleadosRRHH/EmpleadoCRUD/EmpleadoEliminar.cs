using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;

namespace RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.EmpleadoCRUD
{
    public partial class EmpleadoEliminar : Form
    {
        private const int ESTADO_ACTIVO = 1;
        private const int ESTADO_INACTIVO = 2;

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
            button4.Click += button4_Click; // Eliminar (Inactivar)
            button3.Click += button3_Click; // Volver

            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }

        private void EmpleadoEliminar_Load(object sender, EventArgs e)
        {
            try
            {
                // Textos
                button1.Text = "Buscar";
                button2.Text = "Limpiar";
                button4.Text = "Inactivar";
                button3.Text = "Volver";

                // Combo de búsqueda (criterio + estado a mostrar)
                // No agrego controles nuevos, lo resuelvo desde comboBox1
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox1.Items.Clear();

                // Formato: "CRITERIO | ESTADO"
                comboBox1.Items.Add("CI | Activos");
                comboBox1.Items.Add("Nombres | Activos");
                comboBox1.Items.Add("Apellidos | Activos");

                comboBox1.Items.Add("CI | Inactivos");
                comboBox1.Items.Add("Nombres | Inactivos");
                comboBox1.Items.Add("Apellidos | Inactivos");

                comboBox1.SelectedIndex = 0;

                ConfigurarGrid();

                // Al inicio mostrar últimos activos
                CargarUltimos(ESTADO_ACTIVO);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        // ==========================
        // 1) Cargar últimos
        // ==========================
        private void CargarUltimos(int idEstado)
        {
            const string sql = @"
SELECT TOP 20
    e.IdEmpleado,
    e.CI,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    e.Telefono,
    d.Nombre AS Departamento,
    c.Nombre AS Cargo,
    est.Nombre AS Estado
FROM Empleado e
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
OUTER APPLY (
    SELECT TOP 1 eh.IdEstado
    FROM EstadoHistorial eh
    WHERE eh.IdEmpleado = e.IdEmpleado
    ORDER BY eh.FechaRegitro DESC, eh.IdHistorial DESC
) ult
INNER JOIN Estado est ON est.IdEstado = ISNULL(ult.IdEstado, 1)
WHERE ISNULL(ult.IdEstado, 1) = @IdEstado
ORDER BY e.IdEmpleado DESC;";

            using SqlConnection cn = ConexionDB.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@IdEstado", idEstado);

            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dataGridView1.DataSource = dt;

            // Opcional: esconder IdEmpleado para “no mostrar ids”
            if (dataGridView1.Columns.Contains("IdEmpleado"))
                dataGridView1.Columns["IdEmpleado"].Visible = false;
        }

        // ==========================
        // 2) Buscar (button1)
        // ==========================
        private void button1_Click(object sender, EventArgs e)
        {
            string criterio = comboBox1.SelectedItem?.ToString() ?? "";
            string texto = textBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                MessageBox.Show("Ingrese un valor para buscar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var parsed = ParseCombo(criterio);
                Buscar(parsed.Campo, texto, parsed.IdEstado);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Buscar(string campo, string valor, int idEstado)
        {
            // Búsqueda flexible (LIKE para nombres/apellidos, exacta para CI)
            string where;
            bool exacto = campo == "CI";

            if (exacto)
                where = "e.CI = @Valor";
            else if (campo == "Nombres")
                where = "e.Nombres LIKE '%' + @Valor + '%'";
            else // Apellidos -> PrimerApellido o SegundoApellido
                where = "(e.PrimerApellido LIKE '%' + @Valor + '%' OR ISNULL(e.SegundoApellido,'') LIKE '%' + @Valor + '%')";

            string sql = $@"
SELECT
    e.IdEmpleado,
    e.CI,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    e.Telefono,
    d.Nombre AS Departamento,
    c.Nombre AS Cargo,
    est.Nombre AS Estado
FROM Empleado e
INNER JOIN Departamento d ON d.IdDepartamento = e.IdDepartamento
INNER JOIN Cargo c ON c.IdCargo = e.IdCargo
OUTER APPLY (
    SELECT TOP 1 eh.IdEstado
    FROM EstadoHistorial eh
    WHERE eh.IdEmpleado = e.IdEmpleado
    ORDER BY eh.FechaRegitro DESC, eh.IdHistorial DESC
) ult
INNER JOIN Estado est ON est.IdEstado = ISNULL(ult.IdEstado, 1)
WHERE ISNULL(ult.IdEstado, 1) = @IdEstado
  AND {where}
ORDER BY e.IdEmpleado DESC;";

            using SqlConnection cn = ConexionDB.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@IdEstado", idEstado);
            cmd.Parameters.AddWithValue("@Valor", valor);

            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

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
            // "Apellidos | Inactivos"
            // (por seguridad si viene mal)
            string[] partes = item.Split('|');
            string campo = partes.Length > 0 ? partes[0].Trim() : "CI";
            string estadoTxt = partes.Length > 1 ? partes[1].Trim().ToLower() : "activos";

            int idEstado = estadoTxt.Contains("inact") ? ESTADO_INACTIVO : ESTADO_ACTIVO;

            // normalización de campo
            if (campo.Equals("CI", StringComparison.OrdinalIgnoreCase)) campo = "CI";
            else if (campo.Equals("Nombres", StringComparison.OrdinalIgnoreCase)) campo = "Nombres";
            else campo = "Apellidos";

            return (campo, idEstado);
        }

        // ==========================
        // 3) Limpiar (button2)
        // ==========================
        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            comboBox1.SelectedIndex = 0;
            CargarUltimos(ESTADO_ACTIVO);
            textBox1.Focus();
        }

        // ==========================
        // 4) Inactivar (button4)
        // ==========================
        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un empleado de la lista.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Recuperar IdEmpleado aunque la columna esté oculta
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

            try
            {
                // Insertar historial con IdEstado = 2 (Inactivo)
                const string sql = @"
INSERT INTO EstadoHistorial (IdEmpleado, IdEstado, FechaRegitro)
VALUES (@IdEmpleado, @IdEstado, @Fecha);";

                using SqlConnection cn = ConexionDB.ObtenerConexion();
                using SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                cmd.Parameters.AddWithValue("@IdEstado", ESTADO_INACTIVO);
                cmd.Parameters.AddWithValue("@Fecha", DateTime.Today);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Empleado inactivado correctamente.", "OK",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refrescar la vista según el combo actual (activos/inactivos)
                var parsed = ParseCombo(comboBox1.SelectedItem?.ToString() ?? "CI | Activos");

                // Si estaba viendo Activos, el empleado ya no debe aparecer
                // Si estaba viendo Inactivos, debe aparecer (si coincide búsqueda)
                if (string.IsNullOrWhiteSpace(textBox1.Text.Trim()))
                {
                    CargarUltimos(parsed.IdEstado);
                }
                else
                {
                    Buscar(parsed.Campo, textBox1.Text.Trim(), parsed.IdEstado);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al inactivar:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Doble click también inactiva (opcional)
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Puedes comentar esto si no quieres doble click
            if (e.RowIndex >= 0) button4_Click(sender, EventArgs.Empty);
        }

        // ==========================
        // 5) Volver (button3)
        // ==========================
        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

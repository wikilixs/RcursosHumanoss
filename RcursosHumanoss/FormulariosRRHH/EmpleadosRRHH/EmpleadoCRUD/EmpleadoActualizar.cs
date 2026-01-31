using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;

namespace RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.EmpleadoCRUD
{
    public partial class EmpleadoActualizar : Form
    {
        private int _idEmpleado = 0;
        private int _idEstadoActual = 1; // por defecto Activo

        public EmpleadoActualizar()
        {
            InitializeComponent();
            WireUp();
        }

        private void WireUp()
        {
            Load += EmpleadoActualizar_Load;

            button3.Click += button3_Click; // Buscar
            button4.Click += button4_Click; // Limpiar
            button1.Click += button1_Click; // Actualizar
            button2.Click += button2_Click; // Regresar
        }

        private void EmpleadoActualizar_Load(object sender, EventArgs e)
        {
            try
            {
                // Textos
                label1.Text = "Nombres";
                label2.Text = "Primer Apellido";
                label3.Text = "Segundo Apellido";
                label4.Text = "CI";
                label5.Text = "Teléfono";
                label6.Text = "Fecha Nacimiento";
                label7.Text = "Departamento";
                label8.Text = "Cargo";
                label9.Text = "Estado";

                button3.Text = "Buscar";
                button4.Text = "Limpiar";
                button1.Text = "Actualizar";
                button2.Text = "Volver";

                // Combo criterio (solo CI)
                comboBox4.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox4.Items.Clear();
                comboBox4.Items.Add("CI");
                comboBox4.SelectedIndex = 0;

                // Cargar combos de edición
                CargarCombosEdicion();

                // Al inicio: bloquear edición hasta buscar
                BloquearEdicion(true);
                LimpiarEdicion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================
        // 1) Combos de edición
        // ==========================
        private void CargarCombosEdicion()
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;

            comboBox1.DataSource = ObtenerDepartamentos();
            comboBox1.DisplayMember = "Nombre";
            comboBox1.ValueMember = "IdDepartamento";

            comboBox2.DataSource = ObtenerCargos();
            comboBox2.DisplayMember = "Nombre";
            comboBox2.ValueMember = "IdCargo";

            comboBox3.DataSource = ObtenerEstados();
            comboBox3.DisplayMember = "Nombre";
            comboBox3.ValueMember = "IdEstado";
        }

        private static DataTable ObtenerDepartamentos()
        {
            const string sql = "SELECT IdDepartamento, Nombre FROM Departamento ORDER BY Nombre;";
            using SqlConnection cn = ConexionDB.ObtenerConexion();
            using SqlDataAdapter da = new SqlDataAdapter(sql, cn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        private static DataTable ObtenerCargos()
        {
            const string sql = "SELECT IdCargo, Nombre FROM Cargo ORDER BY Nombre;";
            using SqlConnection cn = ConexionDB.ObtenerConexion();
            using SqlDataAdapter da = new SqlDataAdapter(sql, cn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        private static DataTable ObtenerEstados()
        {
            const string sql = "SELECT IdEstado, Nombre FROM Estado ORDER BY IdEstado;";
            using SqlConnection cn = ConexionDB.ObtenerConexion();
            using SqlDataAdapter da = new SqlDataAdapter(sql, cn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        // ==========================
        // 2) Buscar por CI (button3)
        // ==========================
        private void button3_Click(object sender, EventArgs e)
        {
            string ci = textBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(ci))
            {
                MessageBox.Show("Ingrese el CI para buscar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var encontrado = BuscarEmpleadoPorCI(ci);

                if (!encontrado)
                {
                    MessageBox.Show("No se encontró un empleado con ese CI.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _idEmpleado = 0;
                    BloquearEdicion(true);
                    LimpiarEdicion();
                    return;
                }

                // ✅ Si encontró: habilitar edición
                BloquearEdicion(false);

                // ⚠️ CI no se debe cambiar
                maskedTextBox4.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool BuscarEmpleadoPorCI(string ci)
        {
            const string sql = @"
SELECT
    e.IdEmpleado,
    e.Nombres,
    e.PrimerApellido,
    e.SegundoApellido,
    e.CI,
    e.FechaNacimiento,
    e.Telefono,
    e.IdDepartamento,
    e.IdCargo,
    ISNULL((
        SELECT TOP 1 eh.IdEstado
        FROM EstadoHistorial eh
        WHERE eh.IdEmpleado = e.IdEmpleado
        ORDER BY eh.FechaRegitro DESC, eh.IdHistorial DESC
    ), 1) AS IdEstadoActual
FROM Empleado e
WHERE e.CI = @CI;";

            using SqlConnection cn = ConexionDB.ObtenerConexion();
            using SqlCommand cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@CI", ci);

            using SqlDataReader dr = cmd.ExecuteReader();

            if (!dr.Read())
                return false;

            _idEmpleado = Convert.ToInt32(dr["IdEmpleado"]);

            maskedTextBox1.Text = dr["Nombres"].ToString();
            maskedTextBox2.Text = dr["PrimerApellido"].ToString();
            maskedTextBox3.Text = dr["SegundoApellido"] == DBNull.Value ? "" : dr["SegundoApellido"].ToString();
            maskedTextBox4.Text = dr["CI"].ToString();
            maskedTextBox5.Text = dr["Telefono"].ToString();
            dateTimePicker1.Value = Convert.ToDateTime(dr["FechaNacimiento"]);

            comboBox1.SelectedValue = Convert.ToInt32(dr["IdDepartamento"]);
            comboBox2.SelectedValue = Convert.ToInt32(dr["IdCargo"]);

            _idEstadoActual = Convert.ToInt32(dr["IdEstadoActual"]);
            comboBox3.SelectedValue = _idEstadoActual;

            return true;
        }

        // ==========================
        // 3) Limpiar (button4)
        // ==========================
        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            _idEmpleado = 0;

            LimpiarEdicion();
            BloquearEdicion(true);

            textBox1.Focus();
        }

        private void LimpiarEdicion()
        {
            maskedTextBox1.Clear();
            maskedTextBox2.Clear();
            maskedTextBox3.Clear();
            maskedTextBox4.Clear();
            maskedTextBox5.Clear();

            dateTimePicker1.Value = DateTime.Today;

            if (comboBox1.DataSource != null) comboBox1.SelectedIndex = -1;
            if (comboBox2.DataSource != null) comboBox2.SelectedIndex = -1;
            if (comboBox3.DataSource != null) comboBox3.SelectedIndex = -1;
        }

        private void BloquearEdicion(bool bloquear)
        {
            maskedTextBox1.Enabled = !bloquear;
            maskedTextBox2.Enabled = !bloquear;
            maskedTextBox3.Enabled = !bloquear;
            maskedTextBox4.Enabled = !bloquear; // luego lo bloqueo fijo al cargar
            maskedTextBox5.Enabled = !bloquear;
            dateTimePicker1.Enabled = !bloquear;

            comboBox1.Enabled = !bloquear;
            comboBox2.Enabled = !bloquear;
            comboBox3.Enabled = !bloquear;

            button1.Enabled = !bloquear; // Actualizar
        }

        // ==========================
        // 4) Actualizar (button1)
        // ==========================
        private void button1_Click(object sender, EventArgs e)
        {
            if (_idEmpleado <= 0)
            {
                MessageBox.Show("Primero debe buscar un empleado por CI.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nombres = maskedTextBox1.Text.Trim();
            string primerApellido = maskedTextBox2.Text.Trim();
            string segundoApellido = maskedTextBox3.Text.Trim();
            string ci = maskedTextBox4.Text.Trim(); // no se cambia
            string telefono = maskedTextBox5.Text.Trim();
            DateTime fechaNacimiento = dateTimePicker1.Value.Date;

            if (string.IsNullOrWhiteSpace(nombres) ||
                string.IsNullOrWhiteSpace(primerApellido) ||
                string.IsNullOrWhiteSpace(ci) ||
                string.IsNullOrWhiteSpace(telefono))
            {
                MessageBox.Show("Nombres, Primer Apellido, CI y Teléfono son obligatorios.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox1.SelectedValue == null || comboBox2.SelectedValue == null || comboBox3.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar Departamento, Cargo y Estado.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idDepartamento = Convert.ToInt32(comboBox1.SelectedValue);
            int idCargo = Convert.ToInt32(comboBox2.SelectedValue);
            int idEstadoNuevo = Convert.ToInt32(comboBox3.SelectedValue);

            try
            {
                using SqlConnection cn = ConexionDB.ObtenerConexion();
                using SqlTransaction tx = cn.BeginTransaction();

                // 1) Actualizar Empleado
                const string sqlUpdate = @"
UPDATE Empleado
SET
    Nombres = @Nombres,
    PrimerApellido = @PrimerApellido,
    SegundoApellido = @SegundoApellido,
    FechaNacimiento = @FechaNacimiento,
    Telefono = @Telefono,
    IdDepartamento = @IdDepartamento,
    IdCargo = @IdCargo
WHERE IdEmpleado = @IdEmpleado;";

                using (SqlCommand cmd = new SqlCommand(sqlUpdate, cn, tx))
                {
                    cmd.Parameters.AddWithValue("@IdEmpleado", _idEmpleado);
                    cmd.Parameters.AddWithValue("@Nombres", nombres);
                    cmd.Parameters.AddWithValue("@PrimerApellido", primerApellido);
                    cmd.Parameters.AddWithValue("@SegundoApellido",
                        string.IsNullOrWhiteSpace(segundoApellido) ? (object)DBNull.Value : segundoApellido);
                    cmd.Parameters.AddWithValue("@FechaNacimiento", fechaNacimiento);
                    cmd.Parameters.AddWithValue("@Telefono", telefono);
                    cmd.Parameters.AddWithValue("@IdDepartamento", idDepartamento);
                    cmd.Parameters.AddWithValue("@IdCargo", idCargo);

                    int filas = cmd.ExecuteNonQuery();
                    if (filas == 0)
                        throw new Exception("No se actualizó ningún registro.");
                }

                // 2) Si cambió estado, insertar en historial
                if (idEstadoNuevo != _idEstadoActual)
                {
                    const string sqlHist = @"
INSERT INTO EstadoHistorial (IdEmpleado, IdEstado, FechaRegitro)
VALUES (@IdEmpleado, @IdEstado, @FechaRegitro);";

                    using SqlCommand cmdHist = new SqlCommand(sqlHist, cn, tx);
                    cmdHist.Parameters.AddWithValue("@IdEmpleado", _idEmpleado);
                    cmdHist.Parameters.AddWithValue("@IdEstado", idEstadoNuevo);
                    cmdHist.Parameters.AddWithValue("@FechaRegitro", DateTime.Today);

                    cmdHist.ExecuteNonQuery();

                    _idEstadoActual = idEstadoNuevo;
                }

                tx.Commit();

                MessageBox.Show("Empleado actualizado correctamente.", "OK",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Opcional: dejar el form listo para otra búsqueda
                // button4_Click(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================
        // 5) Volver (button2)
        // ==========================
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

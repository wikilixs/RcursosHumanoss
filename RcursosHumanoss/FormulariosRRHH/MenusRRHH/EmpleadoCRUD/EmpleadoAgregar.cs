using System;
using System.Data;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.EntidadesRRHH;

namespace RcursosHumanoss.FormulariosRRHH.EmpleadosRRHH.EmpleadoCRUD
{
    public partial class EmpleadoAgregar : Form
    {
        public EmpleadoAgregar()
        {
            InitializeComponent();

            // No toco el Designer, solo asigno lógica aquí
            button1.Text = "Agregar";
            button2.Text = "Volver";

            label1.Text = "Nombres";
            label2.Text = "Primer Apellido";
            label3.Text = "Segundo Apellido";
            label4.Text = "CI";
            label5.Text = "Teléfono";
            label6.Text = "Fecha Nacimiento";
            label7.Text = "Departamento";
            label8.Text = "Cargo";

            button1.Click += button1_Click;
            button2.Click += button2_Click;

            // Más cómodo
            AcceptButton = button1;

            // (Opcional) limitar entrada a números
            maskedTextBox4.KeyPress += SoloNumeros_KeyPress; // CI
            maskedTextBox5.KeyPress += SoloNumeros_KeyPress; // Teléfono

            // Si el designer no conectó el Load, aquí lo conectamos sin tocar el diseñador
            Load += EmpleadoAgregar_Load;
        }

        private void EmpleadoAgregar_Load(object sender, EventArgs e)
        {
            CargarCombos();
        }

        // Si tu designer NO conectó el evento Load, puedes forzarlo así:
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (comboBox1.DataSource == null || comboBox2.DataSource == null)
                CargarCombos();
        }

        // =========================================================
        //  Combos (desde BD: IDs reales autoincrementables)
        // =========================================================
        private void CargarCombos()
        {
            try
            {
                // Departamentos reales desde BD
                DataTable dtDep = CatalogoDAL.ListarDepartamentos();
                comboBox1.DataSource = dtDep;
                comboBox1.DisplayMember = "Nombre";
                comboBox1.ValueMember = "IdDepartamento";
                comboBox1.SelectedIndex = -1;

                // Cargos reales desde BD
                DataTable dtCar = CatalogoDAL.ListarCargos();
                comboBox2.DataSource = dtCar;
                comboBox2.DisplayMember = "Nombre";
                comboBox2.ValueMember = "IdCargo";
                comboBox2.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando Departamentos/Cargos:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        //  Botón Agregar
        // =========================================================
        private void button1_Click(object sender, EventArgs e)
        {
            // Lectura de inputs (sin asumir)
            string nombres = (maskedTextBox1.Text ?? "").Trim();
            string primerApellido = (maskedTextBox2.Text ?? "").Trim();
            string segundoApellido = (maskedTextBox3.Text ?? "").Trim();
            string ci = (maskedTextBox4.Text ?? "").Trim();
            string telefono = (maskedTextBox5.Text ?? "").Trim();
            DateTime fechaNac = dateTimePicker1.Value.Date;

            if (string.IsNullOrWhiteSpace(nombres))
            {
                MessageBox.Show("Nombres es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(primerApellido))
            {
                MessageBox.Show("Primer Apellido es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ci))
            {
                MessageBox.Show("CI es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(telefono))
            {
                MessageBox.Show("Teléfono es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar Departamento.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox2.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar Cargo.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idDepartamento = Convert.ToInt32(comboBox1.SelectedValue);
            int idCargo = Convert.ToInt32(comboBox2.SelectedValue);

            // Segundo apellido puede quedar null
            string? segundoApellidoNullable = string.IsNullOrWhiteSpace(segundoApellido) ? null : segundoApellido;

            try
            {
                // Creamos entidad
                EntidadEmpleado emp = new EntidadEmpleado
                {
                    Nombres = nombres,
                    PrimerApellido = primerApellido,
                    SegundoApellido = segundoApellidoNullable,
                    CI = ci,
                    FechaNacimiento = fechaNac,
                    Telefono = telefono,
                    IdDepartamento = idDepartamento,
                    IdCargo = idCargo
                };

                // Inserta empleado + crea usuario automático
                int idEmpleado = EmpleadoDAL.InsertarUsuarioAuto(emp);

                MessageBox.Show($"Empleado agregado correctamente. IdEmpleado: {idEmpleado}",
                    "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar empleado:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        //  Botón Volver
        // =========================================================
        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LimpiarFormulario()
        {
            maskedTextBox1.Clear();
            maskedTextBox2.Clear();
            maskedTextBox3.Clear();
            maskedTextBox4.Clear();
            maskedTextBox5.Clear();

            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;

            dateTimePicker1.Value = DateTime.Today;
        }

        private void SoloNumeros_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permitir control (backspace, etc.)
            if (char.IsControl(e.KeyChar))
                return;

            // Solo dígitos
            if (!char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        // Tu evento ya existe en el Designer, lo dejo (no lo elimino)
        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            // Opcional
        }
    }
}

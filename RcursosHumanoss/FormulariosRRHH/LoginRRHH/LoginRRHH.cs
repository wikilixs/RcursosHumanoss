using System;
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.EntidadesRRHH;
using RcursosHumanoss.SesionRRHH; // ✅ EntidadSesion
using RcursosHumanoss.FormulariosRRHH.MenusRRHH;

namespace RcursosHumanoss.FormulariosRRHH.LoginRRHH
{
    public partial class LoginRRHH : Form
    {
        public LoginRRHH()
        {
            InitializeComponent();

            textBox2.UseSystemPasswordChar = true;
            label1.Text = "Email";
            label2.Text = "Contraseña";
            button1.Text = "Iniciar sesión";

            button1.Click += button1_Click;
            AcceptButton = button1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string email = textBox1.Text.Trim();
            string password = textBox2.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Debe ingresar Email y Contraseña.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                EntidadLogin? info = LoginDAL.ObtenerPorEmail(email);

                if (info == null)
                {
                    MessageBox.Show("Credenciales incorrectas.", "Login",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool ok = BCrypt.Net.BCrypt.Verify(password, info.PasswordHash);

                if (!ok)
                {
                    MessageBox.Show("Credenciales incorrectas.", "Login",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ✅ Cargar sesión
                EntidadSesion.Iniciar(
                    info.IdEmpleado,
                    info.Email,
                    info.IdDepartamento,
                    info.Departamento,
                    info.IdCargo,
                    info.Cargo,
                    info.NombreCompleto
                );

                AbrirMenuSegunCargo(info.Cargo, info.Departamento);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al iniciar sesión:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirMenuSegunCargo(string cargo, string departamento)
        {
            string cargoLower = (cargo ?? string.Empty).Trim().ToLowerInvariant();
            string depLower = (departamento ?? string.Empty).Trim().ToLowerInvariant();

            Form menu;

            if ((cargoLower.Contains("gerente") || cargoLower.Contains("jefe") || cargoLower.Contains("director"))
                && (depLower.Contains("recursos humanos") || depLower.Contains("rrhh")))
            {
                menu = new FormMenuRRHH();
            }
            else if (cargoLower.Contains("supervisor") || cargoLower.Contains("encargado")
                     || cargoLower.Contains("coordinador") || cargoLower.Contains("jefe de área"))
            {
                menu = new FormMenuSupervisores();
            }
            else
            {
                menu = new FormMenuEmpleado();
            }

            Hide();
            menu.FormClosed += (_, __) => Close();
            menu.Show();
        }
    }
}

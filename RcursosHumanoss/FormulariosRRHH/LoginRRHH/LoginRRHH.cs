using System;
using System.ComponentModel; 
using System.Windows.Forms;
using RcursosHumanoss.DALRRHH;
using RcursosHumanoss.FormulariosRRHH.MenusRRHH;

namespace RcursosHumanoss.FormulariosRRHH.LoginRRHH
{
    public partial class LoginRRHH : Form
    {
        public LoginRRHH()
        {
            InitializeComponent();
            Load += LoginRRHH_Load;
        }

        private void LoginRRHH_Load(object? sender, EventArgs e)
        {
            // ✅ Evita que el diseñador ejecute lógica en tiempo de diseño
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            textBox2.UseSystemPasswordChar = true;
            label1.Text = "Email";
            label2.Text = "Contraseña";
            button1.Text = "Iniciar sesión";

            button1.Click -= button1_Click; // evita doble suscripción
            button1.Click += button1_Click;

            AcceptButton = button1;
        }

        private void button1_Click(object? sender, EventArgs e)
        {
            string email = textBox1.Text.Trim();
            string passwordPlano = textBox2.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(passwordPlano))
            {
                MessageBox.Show("Debe ingresar Email y Contraseña.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // ✅ Este método DEBE existir en LoginDAL
                var info = LoginDAL.ObtenerInfoPorEmail(email);

                if (info == null)
                {
                    MessageBox.Show("Credenciales incorrectas.", "Login",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ✅ BCrypt (el hash está en DB)
                bool ok = BCrypt.Net.BCrypt.Verify(passwordPlano, info.PasswordHash);

                if (!ok)
                {
                    MessageBox.Show("Credenciales incorrectas.", "Login",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

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
            string cargoLower = (cargo ?? "").Trim().ToLowerInvariant();
            string depLower = (departamento ?? "").Trim().ToLowerInvariant();

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

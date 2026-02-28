using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Code_Generator.Global_Classes;

namespace Code_Generator
{
    public partial class Login : Form
    {
        private string serverName = "DESKTOP-U71E8LN"; // أو من إعدادات
        private DataTable _dtAllDatabases;

        public Login()
        {
            InitializeComponent();
        }

      

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!this.ValidateChildren())
            {
                MessageBox.Show("Some fileds are not valide!, put the mouse over the red icons(s)", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;

                bool connectionSuccess = _ConnectToServer(username, password);

                if (connectionSuccess)
                {
                    if (tsRememberMe.Checked)
                    {
                        // Store username and password
                        clsGlobal.RememberUsernameAndPassword(txtUsername.Text.Trim(), txtPassword.Text.Trim());
                    }
                    else
                    {
                        // Store empty username and password
                        clsGlobal.RememberUsernameAndPassword("", "");
                    }
                    this.Hide();
                    frmMain frm = new frmMain(_dtAllDatabases);
                    frm.ShowDialog();

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Conect to Server:\n {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private bool _ConnectToServer(string username, string password)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = serverName,
                    UserID = username,
                    Password = password,
                    InitialCatalog = "master",
                    ConnectTimeout = 15
                };

                string query = @"
                SELECT name 
                FROM sys.databases 
                WHERE state = 0
                AND name NOT IN ('master', 'tempdb', 'model', 'msdb')
                ORDER BY name";

                using (SqlConnection conn = new SqlConnection(builder.ToString()))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            _dtAllDatabases = new DataTable();
                            _dtAllDatabases.Load(reader);
                        }
                        reader.Close();
                    }

                    return true;
                }
            }
            catch (SqlException sqlEx)
            {
                return false;
            }
        }

        private void txtUsername_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                errorProvider1.SetError(txtUsername, "This Record Requerd");
                e.Cancel = false;
            }
            else
                errorProvider1.SetError(txtUsername, null);
        }

        private void txtPassword_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                errorProvider1.SetError(txtPassword, "This Record Requerd");
                e.Cancel = false;
            }
            else
                errorProvider1.SetError(txtPassword, null);
        }

        private void Login_Load(object sender, EventArgs e)
        {
            string UserName = "", Password = "";
            if (clsGlobal.GetStoredCredential(ref UserName, ref Password))
            {
                txtUsername.Text = UserName;
                txtPassword.Text = Password;
                tsRememberMe.Checked = true;
            }
            else
                tsRememberMe.Checked = false;
        }
    }
}

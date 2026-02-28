using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Code_Generator
{
    public partial class frmMain : Form
    {
        private string _DatabaseName;
        private string _FolderPath;
        private DataTable _dtAllDatabases;

        public frmMain(DataTable dtAllDatabases)
        {
            InitializeComponent();
            _dtAllDatabases = dtAllDatabases;
        }

        public void FullComboBoxDatabeases()
        {
            cbDatabases.DataSource = _dtAllDatabases;
            cbDatabases.DisplayMember = "name";

            
                cbDatabases.SelectedIndex = -1;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            FullComboBoxDatabeases();
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.Filter = "Database Files(*.db, *.bak, *.mdf)|*.db;*.bak;*.mdf";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fullPath = openFileDialog1.FileName;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullPath);
                _DatabaseName = fileNameWithoutExtension;
                bool isUploaded = await UploadDatabase(openFileDialog1.FileName, _DatabaseName);

                if (isUploaded)
                {
                    // تحديث قائمة قواعد البيانات بعد الإضافة
                    _RefreshDatabasesList();
                    MessageBox.Show("The Upload Is Successfully "+ openFileDialog1.SafeFileName, "Upload Done",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Error Upload Database", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }


        private async Task<bool> UploadDatabase(string PathDatabase, string DatabaseName)
        {
            bool isUploaded = false;

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = "DESKTOP-U71E8LN",
                    UserID = "sa",
                    Password = "sa123",
                    InitialCatalog = "master",
                    ConnectTimeout = 15
                };

                string query = @"
                RESTORE DATABASE @DatabaseName
                FROM DISK = @PathDatabase";

                using (SqlConnection conn = new SqlConnection(builder.ToString()))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DatabaseName", DatabaseName);
                        cmd.Parameters.AddWithValue("@PathDatabase", PathDatabase);
                        await cmd.ExecuteNonQueryAsync();
                        isUploaded = true;

                    }

                   
                }
            }
            catch (Exception ex)
            {
                isUploaded = false;
            }

            return isUploaded;
        }


        private void _RefreshDatabasesList()
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = "DESKTOP-U71E8LN",
                    UserID = "sa",
                    Password = "sa123",
                    InitialCatalog = "master",
                    ConnectTimeout = 15
                };

                string query = "SELECT name FROM sys.databases WHERE database_id > 4";

                using (SqlConnection conn = new SqlConnection(builder.ToString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            // تحديث DataTable و ComboBox
                            _dtAllDatabases.Clear();
                            _dtAllDatabases.Merge(dt);

                            cbDatabases.DataSource = _dtAllDatabases;
                            cbDatabases.DisplayMember = "name";
                            cbDatabases.ValueMember = "name";

                            if (_dtAllDatabases.Rows.Count > 0)
                                cbDatabases.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Refresh Database List: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                _FolderPath = folderBrowserDialog1.SelectedPath;
                txtFilePath.Text = _FolderPath;
            }
           
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if(cbDatabases.SelectedIndex > 0 && !string.IsNullOrEmpty(txtFilePath.Text.Trim()))
            {
                _DatabaseName = cbDatabases.GetItemText(cbDatabases.SelectedItem);

                frmChooesTables frm = new frmChooesTables(_DatabaseName, _FolderPath);
                frm.ShowDialog();

                this.Close();
            }
            else
            {
                MessageBox.Show("Shud be Select Database Name from List and Chooes Folder Path!!",
                    "Not Allowe", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

           
        }
    }
}

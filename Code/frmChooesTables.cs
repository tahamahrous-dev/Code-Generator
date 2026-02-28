using Code_Generator.Global_Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Diagnostics;
using Code_Generator.Generator_Classes;
using BusinessLayerGenerator;

namespace Code_Generator
{
    public partial class frmChooesTables : Form
    {
        private string _DatabaseName;
        private string _FolderPath;
        private string _FolderDataAccess;
        private string _FolderDataBusiness;
        private DataTable _dtAllTables;
        private List<string> _ChooseTables = new List<string>();
        private clsGeneratorSettings _SearchingFK =  new clsGeneratorSettings();
        private clsGeneratorSettings _AddingStaticMethods = new clsGeneratorSettings();

        bool _isLoading = false;

        private Stopwatch _sw;

        public frmChooesTables(string DatabaseName, string FolderPath)
        {
            InitializeComponent();
            _DatabaseName = DatabaseName;
            _FolderPath = FolderPath;
        }

        private void FullCheckedList()
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = "DESKTOP-U71E8LN",
                    UserID = "sa",
                    Password = "sa123",
                    InitialCatalog = _DatabaseName,
                    ConnectTimeout = 15
                };

                string query = "SELECT name AS TableName FROM sys.tables WHERE name != 'sysdiagrams'";

                using (SqlConnection conn = new SqlConnection(builder.ToString()))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            _dtAllTables = new DataTable();
                            _dtAllTables.Load(reader);
                        }
                        reader.Close();
                    }
                }
            }

            catch (Exception ex)
            {
            }

            foreach (DataRow row in _dtAllTables.Rows)
            {
                string itemString = row["TableName"].ToString();
                chkAllTables.Items.Add(itemString);
            }
        }

        private void chkChooesAllTables_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;

            _ChooseTables.Clear();

            for (int i = 0; i < chkAllTables.Items.Count; i++)
            {
                chkAllTables.SetItemChecked(i, chkChooesAllTables.Checked);

                if (chkChooesAllTables.Checked)
                    _ChooseTables.Add(chkAllTables.Items[i].ToString());
            }

        }

        private void frmChooesTables_Load(object sender, EventArgs e)
        {
            _isLoading = true;

            FullCheckedList();
            chkChooesAllTables.Checked = (chkAllTables.Items.Count > 0);

            rbAll.Checked = true;
            rbYes.Checked = true;

            _isLoading = false;
        }


        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _sw = Stopwatch.StartNew();
            btnGenerate.Enabled = false;

            _FolderDataAccess = string.Format(@"{0}\{1}_DataAccess\", _FolderPath, _DatabaseName);
            _FolderDataBusiness = string.Format(@"{0}\{1}_Business\", _FolderPath, _DatabaseName);

            if (!clsUtil.CreateFolderIfDoesNotExist(_FolderDataAccess))
            {
                return;
            }

            if (!clsUtil.CreateFolderIfDoesNotExist(_FolderDataBusiness))
            {
                return;
            }

            if (!clsUtil.CreateConnectionSettingFile(_FolderDataAccess, ".", _DatabaseName, "sa", "sa123"))
            {
                return;
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = "DESKTOP-U71E8LN",
                UserID = "sa",
                Password = "sa123",
                InitialCatalog = _DatabaseName,
                ConnectTimeout = 15
            };



            clsDataAccessLayerGenerator clsDataAccessLayer = new clsDataAccessLayerGenerator(builder.ToString(), _DatabaseName, _FolderDataAccess,
                _ChooseTables, _SearchingFK, _AddingStaticMethods);
            clsBusinessLayerGenerator clsBusinessLayer = new clsBusinessLayerGenerator(builder.ToString(),_DatabaseName+"_DataAccess", _DatabaseName,
                _FolderDataBusiness, _ChooseTables);

            if (clsDataAccessLayer.GenerateAllDataAccessClasses() && clsBusinessLayer.GenerateAllBusinessClasses())
            {
                _sw.Stop();
                MessageBox.Show("Created Success, In: " + _sw.ElapsedMilliseconds + "ms", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Error Create Files", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rbAll_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAll.Checked)
            {
                _SearchingFK.FKSearchMode = clsGeneratorSettings.enFKSearchMode.All;
            }
            else
            {
                _SearchingFK.FKSearchMode = clsGeneratorSettings.enFKSearchMode.JustThis;
            }
        }

        private void rbYes_CheckedChanged(object sender, EventArgs e)
        {
            if (rbYes.Checked)
            {
                _AddingStaticMethods.StaticMethodsMode = clsGeneratorSettings.enStaticMethodsMode.Yes;
            }
            else if (rbNo.Checked)
            {
                if (MessageBox.Show(@"If You Select This you Didn't Have All This  Methods\n In Code:\n
                                    \t\n
                                        1) Static Adding New Row\n
                                        2) Static Update Row\n
                                        3) Static Find\n
                                        4) Get All Rows\n
                                        5) Delete Row\n
                                        6) Search Data And and Return DataTable\n
                                Do you want to Let?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    _AddingStaticMethods.StaticMethodsMode = clsGeneratorSettings.enStaticMethodsMode.No;

                }
            }
        }

        private void chkAllTables_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string tableName = chkAllTables.Items[e.Index].ToString();

            if (e.NewValue == CheckState.Checked)
            {
                if (!_ChooseTables.Contains(tableName))
                    _ChooseTables.Add(tableName);
            }
            else
            {
                if (_ChooseTables.Contains(tableName))
                    _ChooseTables.Remove(tableName);
            }
        }

      
    }
}

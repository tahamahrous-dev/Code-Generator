using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace Code_Generator.Global_Classes
{
    public class clsGlobal
    {
        public struct stColumnInfo
        {
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public bool IsNullable { get; set; }
            public int? MaxLength { get; set; }
            public bool IsPrimaryKey { get; set; }
        }

        public static bool RememberUsernameAndPassword(string Username, string Password)
        {
            string FileName = "RememberMe.txt";
            bool IsRememberMe;

            if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password))
            {
                File.Delete(FileName);
                IsRememberMe = false;
            }
            else
            {
                try
                {
                    string LoginData = $"{Username}#{Password}";
                    File.WriteAllText(FileName, LoginData);
                    IsRememberMe = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving login info: " + ex.Message, "Error Saved", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    IsRememberMe = false;
                }
            }

            return IsRememberMe;
        }

        public static bool GetStoredCredential(ref string UserName, ref string Password)
        {
            string FileName = "RememberMe.txt";
            bool IsRememberMe;

            try
            {
                string[] DataParts = File.ReadAllText(FileName).Split('#');
                if (DataParts.Length == 2)
                {
                    UserName = DataParts[0];
                    Password = DataParts[1];
                }
                IsRememberMe = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving login info: " + ex.Message, "Error Saved", MessageBoxButtons.OK, MessageBoxIcon.Error);
                IsRememberMe = false;

            }

            return IsRememberMe;
        }

        public static string ConnectionString = "Server=.;Database=DVLD_DB;User Id=sa;Password=sa123;";

    }
}

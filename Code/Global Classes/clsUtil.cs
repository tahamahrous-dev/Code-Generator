using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Code_Generator.Global_Classes
{
    public class clsUtil
    {
        public static bool CreateFolderIfDoesNotExist(string FolderName)
        {
            if (!Directory.Exists(FolderName))
            {
                Directory.CreateDirectory(FolderName);
            }

            return true;
        }

        private static string _GenerateClassContent(string ServerName, string DatabaseName, string UserID, string Password)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine($"namespace {DatabaseName}_DataAccess");

            sb.AppendLine("{");
            sb.AppendLine();

            sb.AppendLine($"    static class clsDataAccessSettings");
            sb.AppendLine("    {");

            sb.AppendLine($"        public static string ConnectionString = \"Server={ServerName};Database={DatabaseName};User Id={UserID};Password={Password}\";");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("}");



            return sb.ToString();
        }

        public static bool CreateConnectionSettingFile(string FolderName,string ServerName, string DatabaseName, string UserID, string Password)
        {
            string classContent = _GenerateClassContent(ServerName, DatabaseName, UserID, Password);
            File.WriteAllText(FolderName + "clsDataAccessSettings.cs", classContent, Encoding.UTF8);

            return true;
        }


        
        

    }
}

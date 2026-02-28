using Code_Generator.Generator_Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using static Code_Generator.Global_Classes.clsGlobal;

namespace BusinessLayerGenerator
{
    public class clsBusinessLayerGenerator
    {
        private string _connectionString;
        private string _dataAccessNamespace;
        private string _businessNamespace;
        private string _outputPath;
        private List<string> _DatabaseTables;

        public clsBusinessLayerGenerator(string connectionString, string dataAccessNamespace,
                                     string businessNamespace, string outputPath, List<string> databaseTables)
        {
            _connectionString = connectionString;
            _dataAccessNamespace = dataAccessNamespace;
            _businessNamespace = businessNamespace;
            _outputPath = outputPath;
            _DatabaseTables = databaseTables;
        }

        // توليد كلاسات Business Layer لكل الجداول
        public bool GenerateAllBusinessClasses()
        {
            try
            {
                List<string> tables = _DatabaseTables;

                foreach (var table in tables)
                {
                    GenerateBusinessClassForTable(table);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in generate classes: {ex.Message}");
                return false;
            }
        }

        // الحصول على معلومات أعمدة الجدول
        private List<clsColumnInfo> GetTableColumns(string tableName)
        {
            var columns = new List<clsColumnInfo>();

            string query = @"
                SELECT 
                    COLUMN_NAME,
                    DATA_TYPE,
                    IS_NULLABLE,
                    CHARACTER_MAXIMUM_LENGTH,
                    ORDINAL_POSITION,
                    COLUMNPROPERTY(OBJECT_ID(@TableName), COLUMN_NAME, 'IsIdentity') AS IsIdentity
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @TableName
                ORDER BY ORDINAL_POSITION";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var column = new clsColumnInfo
                        {
                            ColumnName = reader["COLUMN_NAME"].ToString(),
                            DataType = reader["DATA_TYPE"].ToString(),
                            IsNullable = reader["IS_NULLABLE"].ToString().ToUpper() == "YES",
                            MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] as int?,
                            IsIdentity = reader["IsIdentity"] != DBNull.Value && Convert.ToInt32(reader["IsIdentity"]) == 1,
                            IsPrimaryKey = IsPrimaryKey(tableName, reader["COLUMN_NAME"].ToString()),
                            IsForeignKey = IsForeignKey(tableName, reader["COLUMN_NAME"].ToString())
                        };

                        if (column.IsForeignKey)
                        {
                            GetForeignKeyInfo(tableName, column.ColumnName, out string refTable, out string refColumn);
                            column.ReferencedTable = refTable;
                            column.ReferencedColumn = refColumn;
                        }

                        columns.Add(column);
                    }
                }
            }

            return columns;
        }

        // التحقق من Primary Key
        private bool IsPrimaryKey(string tableName, string columnName)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                WHERE TABLE_NAME = @TableName 
                    AND COLUMN_NAME = @ColumnName 
                    AND CONSTRAINT_NAME IN 
                    (SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                     WHERE CONSTRAINT_TYPE = 'PRIMARY KEY')";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName);
                cmd.Parameters.AddWithValue("@ColumnName", columnName);
                conn.Open();

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        // التحقق من Foreign Key
        private bool IsForeignKey(string tableName, string columnName)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                WHERE TABLE_NAME = @TableName 
                    AND COLUMN_NAME = @ColumnName 
                    AND CONSTRAINT_NAME IN 
                    (SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                     WHERE CONSTRAINT_TYPE = 'FOREIGN KEY')";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName);
                cmd.Parameters.AddWithValue("@ColumnName", columnName);
                conn.Open();

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        // الحصول على معلومات Foreign Key
        private void GetForeignKeyInfo(string tableName, string columnName,
                                      out string referencedTable, out string referencedColumn)
        {
            referencedTable = "";
            referencedColumn = "";

            string query = @"
                SELECT 
                    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
                    rc.name AS ReferencedColumn
                FROM sys.foreign_key_columns fkc
                INNER JOIN sys.foreign_keys fk ON fkc.constraint_object_id = fk.object_id
                INNER JOIN sys.columns rc ON fkc.referenced_column_id = rc.column_id 
                    AND fkc.referenced_object_id = rc.object_id
                WHERE fkc.parent_object_id = OBJECT_ID(@TableName)
                    AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = @ColumnName";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName);
                cmd.Parameters.AddWithValue("@ColumnName", columnName);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        referencedTable = reader["ReferencedTable"].ToString();
                        referencedColumn = reader["ReferencedColumn"].ToString();
                    }
                }
            }
        }

        // الحصول على اسم كلاس DataAccess
        private string GetDataAccessClassName(string tableName)
        {
            // People -> clsPersonData
            // Users -> clsUserData
            string singleName = tableName.TrimEnd('s');
            return $"cls{singleName}Data";
        }

        // الحصول على اسم كلاس Business
        private string GetBusinessClassName(string tableName)
        {
            // People -> clsPerson
            string singleName = tableName.TrimEnd('s');
            return $"cls{singleName}";
        }

        // توليد Business Class لجدول محدد
        public void GenerateBusinessClassForTable(string tableName)
        {
            var columns = GetTableColumns(tableName);
            var primaryKey = columns.Find(c => c.IsPrimaryKey);
            var identityColumn = columns.Find(c => c.IsIdentity);
            var dataAccessClass = GetDataAccessClassName(tableName);
            var businessClass = GetBusinessClassName(tableName);

            string fileName = $"{businessClass}.cs";
            string filePath = Path.Combine(_outputPath, fileName);

            string classContent = GenerateBusinessClassContent(
                tableName, businessClass, dataAccessClass, columns, primaryKey, identityColumn);

            Directory.CreateDirectory(_outputPath);
            File.WriteAllText(filePath, classContent, Encoding.UTF8);

            Console.WriteLine($"تم إنشاء Business Class: {fileName}");
        }

        // توليد محتوى Business Class
        private string GenerateBusinessClassContent(string tableName, string businessClass,
        string dataAccessClass, List<clsColumnInfo> columns,
                                                   clsColumnInfo primaryKey, clsColumnInfo identityColumn)
        {
            StringBuilder sb = new StringBuilder();

            // Using statements
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine($"using {_dataAccessNamespace};");

            // Add using for referenced tables
            foreach (var col in columns.Where(c => c.IsForeignKey))
            {
                string referencedBusinessClass = GetBusinessClassName(col.ReferencedTable);
                sb.AppendLine($"using {_businessNamespace};");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {_businessNamespace}");
            sb.AppendLine("{");

            // Class declaration
            sb.AppendLine($"    public class {businessClass}");
            sb.AppendLine("    {");

            // 1. enMode Enum
            sb.AppendLine("        public enum enMode { AddNew = 0, Update = 1 }");
            sb.AppendLine($"        public enMode Mode = enMode.AddNew;");
            sb.AppendLine();

            // 2. Properties
            foreach (var column in columns)
            {
                string csharpType = GetCSharpType(column.DataType, column.IsNullable);

                // Special handling for Identity/PrimaryKey
                if (column.IsIdentity || column.IsPrimaryKey)
                {
                    sb.AppendLine($"        public {csharpType} {column.ColumnName} {{ get; set; }}");
                }
                else
                {
                    sb.AppendLine($"        public {csharpType} {column.ColumnName} {{ get; set; }}");
                }

                // Add foreign key navigation property
                if (column.IsForeignKey && !string.IsNullOrEmpty(column.ReferencedTable))
                {
                    string referencedClass = GetBusinessClassName(column.ReferencedTable);
                    sb.AppendLine($"        public {referencedClass} {referencedClass}Info {{ get; set; }}");
                }
            }

            // Add FullName/Computed properties if table has name components
            if (HasNameComponents(columns))
            {
                sb.AppendLine();
                sb.AppendLine("        public string FullName");
                sb.AppendLine("        {");
                sb.AppendLine("            get");
                sb.AppendLine("            {");
                sb.AppendLine("                return FirstName + \" \" + SecondName + \" \" + ThirdName + \" \" + LastName;");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
            }

            // Add GenderCaption property if Gender exists
            if (columns.Any(c => c.ColumnName == "Gender" || c.ColumnName == "Gendor"))
            {
                sb.AppendLine();
                sb.AppendLine("        public string GenderCaption");
                sb.AppendLine("        {");
                sb.AppendLine("            get");
                sb.AppendLine("            {");
                sb.AppendLine("                return Gender == 0 ? \"Male\" : \"Female\";");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
            }

            sb.AppendLine();

            // 3. Default Constructor
            sb.AppendLine($"        public {businessClass}()");
            sb.AppendLine("        {");

            foreach (var column in columns)
            {
                if (column.IsIdentity || column.IsPrimaryKey)
                    sb.AppendLine($"            this.{column.ColumnName} = -1;");
                else if (GetCSharpType(column.DataType, column.IsNullable) == "string")
                    sb.AppendLine($"            this.{column.ColumnName} = \"\";");
                else if (GetCSharpType(column.DataType, column.IsNullable) == "DateTime")
                    sb.AppendLine($"            this.{column.ColumnName} = DateTime.Now;");
                else if (GetCSharpType(column.DataType, column.IsNullable) == "bool")
                    sb.AppendLine($"            this.{column.ColumnName} = false;");
                else if (!column.IsNullable && IsValueType(GetCSharpType(column.DataType, column.IsNullable)))
                    sb.AppendLine($"            this.{column.ColumnName} = default({GetCSharpType(column.DataType, column.IsNullable)});");
            }

            sb.AppendLine();
            sb.AppendLine("            Mode = enMode.AddNew;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // 4. Private Constructor (for Find)
            sb.AppendLine($"        private {businessClass}({GenerateConstructorParameters(columns, primaryKey)})");
            sb.AppendLine("        {");

            foreach (var column in columns)
            {
                sb.AppendLine($"            this.{column.ColumnName} = {column.ColumnName};");

                // Initialize navigation properties
                if (column.IsForeignKey && !string.IsNullOrEmpty(column.ReferencedTable))
                {
                    string referencedClass = GetBusinessClassName(column.ReferencedTable);
                    sb.AppendLine($"            this.{referencedClass}Info = {referencedClass}.Find({column.ColumnName});");
                }
            }

            sb.AppendLine("            Mode = enMode.Update;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // 5. AddNew Method
            sb.AppendLine($"        private bool _AddNew{businessClass.Replace("cls", "")}()");
            sb.AppendLine("        {");
            sb.AppendLine($"            // Call DataAccess Layer");
            sb.AppendLine();
            sb.Append($"            this.{primaryKey?.ColumnName ?? identityColumn?.ColumnName} = {dataAccessClass}.AddNew{businessClass.Replace("cls", "")}(");
            sb.Append(GenerateAddNewParameters(columns, primaryKey, identityColumn));
            sb.AppendLine(");");
            sb.AppendLine();
            sb.AppendLine($"            return (this.{primaryKey?.ColumnName ?? identityColumn?.ColumnName} != -1);");
            sb.AppendLine("        }");
            sb.AppendLine();

            // 6. Update Method
            sb.AppendLine($"        private bool _Update{businessClass.Replace("cls", "")}()");
            sb.AppendLine("        {");
            sb.AppendLine($"            // Call DataAccess Layer");
            sb.AppendLine();
            sb.Append($"            return {dataAccessClass}.Update{businessClass.Replace("cls", "")}(");
            sb.Append(GenerateUpdateParameters(columns, primaryKey));
            sb.AppendLine(");");
            sb.AppendLine("        }");
            sb.AppendLine();

            // 7. Find Method (by primary key)
            if (primaryKey != null)
            {
                string pkType = GetCSharpType(primaryKey.DataType, primaryKey.IsNullable);
                sb.AppendLine($"        public static {businessClass} Find({pkType} {primaryKey.ColumnName})");
                sb.AppendLine("        {");
                sb.AppendLine($"            // Initialize variables");

                foreach (var column in columns)
                {
                    string csharpType = GetCSharpType(column.DataType, column.IsNullable);
                    if (csharpType == "string")
                        sb.AppendLine($"            {csharpType} {column.ColumnName} = \"\";");
                    else if (csharpType == "DateTime")
                        sb.AppendLine($"            {csharpType} {column.ColumnName} = DateTime.Now;");
                    else if (csharpType == "bool")
                        sb.AppendLine($"            {csharpType} {column.ColumnName} = false;");
                    else if (IsValueType(csharpType) && !csharpType.Contains("?"))
                        sb.AppendLine($"            {csharpType} {column.ColumnName} = default({csharpType});");
                    else
                        sb.AppendLine($"            {csharpType} {column.ColumnName} = default({csharpType});");
                }
                sb.AppendLine();

                sb.AppendLine($"            bool IsFound = {dataAccessClass}.Get{businessClass.Replace("cls", "")}ByID(");
                sb.AppendLine($"                {primaryKey.ColumnName}, {GenerateFindRefParameters(columns, primaryKey)});");
                sb.AppendLine();
                sb.AppendLine("            if (IsFound)");
                sb.AppendLine($"                return new {businessClass}({GenerateConstructorCallParameters(columns)});");
                sb.AppendLine("            else");
                sb.AppendLine("                return null;");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            // 8. Find Method (by unique column like NationalNo)
            var uniqueStringColumn = columns.Find(c =>
                !c.IsPrimaryKey &&
                !c.IsForeignKey &&
                c.DataType.ToLower().Contains("char") &&
                c.ColumnName.ToLower().Contains("national") ||
                c.ColumnName.ToLower().Contains("code") ||
                c.ColumnName.ToLower().Contains("no"));

            if (uniqueStringColumn != null)
            {
                sb.AppendLine($"        public static {businessClass} Find({GetCSharpType(uniqueStringColumn.DataType, uniqueStringColumn.IsNullable)} {uniqueStringColumn.ColumnName})");
                sb.AppendLine("        {");
                sb.AppendLine($"            // Initialize variables");

                foreach (var column in columns)
                {
                    string csharpType = GetCSharpType(column.DataType, column.IsNullable);
                    if (csharpType == "string")
                        sb.AppendLine($"            {csharpType} {column.ColumnName} = \"\";");
                    else if (csharpType == "DateTime")
                        sb.AppendLine($"            {csharpType} {column.ColumnName} = DateTime.Now;");
                    else if (csharpType == "bool")
                        sb.AppendLine($"            {csharpType} {column.ColumnName} = false;");
                    else if (IsValueType(csharpType) && !csharpType.Contains("?"))
                        sb.AppendLine($"            {csharpType} {column.ColumnName} = default({csharpType});");
                    else
                        sb.AppendLine($"            {csharpType} {column.ColumnName} = default({csharpType});");
                }
                sb.AppendLine();

                sb.AppendLine($"            bool IsFound = {dataAccessClass}.Get{businessClass.Replace("cls", "")}InfoBy{uniqueStringColumn.ColumnName}(");
                sb.AppendLine($"                {uniqueStringColumn.ColumnName}, {GenerateFindByStringRefParameters(columns, primaryKey, uniqueStringColumn)});");
                sb.AppendLine();
                sb.AppendLine("            if (IsFound)");
                sb.AppendLine($"                return new {businessClass}({GenerateConstructorCallParameters(columns)});");
                sb.AppendLine("            else");
                sb.AppendLine("                return null;");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            // 9. Save Method
            sb.AppendLine("        public bool Save()");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (Mode)");
            sb.AppendLine("            {");
            sb.AppendLine("                case enMode.AddNew:");
            sb.AppendLine($"                    if (_AddNew{businessClass.Replace("cls", "")}())");
            sb.AppendLine("                    {");
            sb.AppendLine("                        Mode = enMode.Update;");
            sb.AppendLine("                        return true;");
            sb.AppendLine("                    }");
            sb.AppendLine("                    else");
            sb.AppendLine("                    {");
            sb.AppendLine("                        return false;");
            sb.AppendLine("                    }");
            sb.AppendLine();
            sb.AppendLine("                case enMode.Update:");
            sb.AppendLine($"                    return _Update{businessClass.Replace("cls", "")}();");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            return false;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // 10. GetAll Method
            sb.AppendLine($"        public static DataTable GetAll{businessClass.Replace("cls", "")}()");
            sb.AppendLine("        {");
            sb.AppendLine($"            return {dataAccessClass}.GetAll{businessClass.Replace("cls", "")}();");
            sb.AppendLine("        }");
            sb.AppendLine();

            // 11. Delete Method
            if (primaryKey != null)
            {
                string pkType = GetCSharpType(primaryKey.DataType, primaryKey.IsNullable);
                sb.AppendLine($"        public static bool Delete{businessClass.Replace("cls", "")}({pkType} {primaryKey.ColumnName})");
                sb.AppendLine("        {");
                sb.AppendLine($"            return {dataAccessClass}.Delete{businessClass.Replace("cls", "")}({primaryKey.ColumnName});");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            // 12. IsExist Methods
            if (primaryKey != null)
            {
                string pkType = GetCSharpType(primaryKey.DataType, primaryKey.IsNullable);
                sb.AppendLine($"        public static bool Is{businessClass.Replace("cls", "")}Exist({pkType} {primaryKey.ColumnName})");
                sb.AppendLine("        {");
                sb.AppendLine($"            return {dataAccessClass}.Is{businessClass.Replace("cls", "")}Exist({primaryKey.ColumnName});");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            if (uniqueStringColumn != null)
            {
                sb.AppendLine($"        public static bool Is{businessClass.Replace("cls", "")}Exist({GetCSharpType(uniqueStringColumn.DataType, uniqueStringColumn.IsNullable)} {uniqueStringColumn.ColumnName})");
                sb.AppendLine("        {");
                sb.AppendLine($"            return {dataAccessClass}.Is{businessClass.Replace("cls", "")}Exist({uniqueStringColumn.ColumnName});");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        // Helper Methods
        private string GetCSharpType(string sqlType, bool isNullable)
        {
            string lowerSqlType = sqlType.ToLower();
            string type;

            switch (lowerSqlType)
            {
                case "int": type = "int"; break;
                case "tinyint": type = "byte"; break;
                case "smallint": type = "short"; break;
                case "bigint": type = "long"; break;
                case "bit": type = "bool"; break;
                case "decimal":
                case "numeric":
                case "money":
                    type = "decimal"; break;
                case "float": type = "double"; break;
                case "real": type = "float"; break;
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "date":
                    type = "DateTime"; break;
                case "time": type = "TimeSpan"; break;
                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                case "xml":
                    type = "string"; break;
                case "uniqueidentifier": type = "Guid"; break;
                case "binary":
                case "varbinary":
                case "image":
                    type = "byte[]"; break;
                default: type = "object"; break;
            }

            if (isNullable && type != "string" && type != "byte[]" && type != "object")
            {
                type += "?";
            }

            return type;
        }

        private bool IsValueType(string csharpType)
        {
            string[] valueTypes = { "int", "byte", "short", "long", "bool", "decimal",
                                   "double", "float", "DateTime", "TimeSpan", "Guid" };

            return valueTypes.Any(t => csharpType.StartsWith(t));
        }

        private bool HasNameComponents(List<clsColumnInfo> columns)
        {
            return columns.Any(c => c.ColumnName == "FirstName") &&
                   columns.Any(c => c.ColumnName == "LastName");
        }
        private string GenerateConstructorParameters(List<clsColumnInfo> columns, clsColumnInfo primaryKey)
        {
            var parameters = new List<string>();

            foreach (var column in columns)
            {
                string type = GetCSharpType(column.DataType, column.IsNullable);
                parameters.Add($"{type} {column.ColumnName}");
            }

            return string.Join(", ", parameters);
        }

        private string GenerateConstructorCallParameters(List<clsColumnInfo> columns)
        {
            var parameters = new List<string>();

            foreach (var column in columns)
            {
                parameters.Add($"{column.ColumnName}");
            }

            return string.Join(", ", parameters);
        }

        private string GenerateAddNewParameters(List<clsColumnInfo> columns,
        clsColumnInfo primaryKey, clsColumnInfo identityColumn)
        {
            var parameters = new List<string>();

            foreach (var column in columns)
            {
                if (column != primaryKey && column != identityColumn)
                {
                    parameters.Add($"this.{column.ColumnName}");
                }
            }

            return string.Join(", ", parameters);
        }

        private string GenerateUpdateParameters(List<clsColumnInfo> columns, clsColumnInfo primaryKey)
        {
            var parameters = new List<string>();

            foreach (var column in columns)
            {
                parameters.Add($"this.{column.ColumnName}");
            }

            return string.Join(", ", parameters);
        }

        private string GenerateFindRefParameters(List<clsColumnInfo> columns, clsColumnInfo primaryKey)
        {
            var parameters = new List<string>();

            foreach (var column in columns)
            {
                if (column != primaryKey)
                {
                    parameters.Add($"ref {column.ColumnName}");
                }
            }

            return string.Join(", ", parameters);
        }

        private string GenerateFindByStringRefParameters(List<clsColumnInfo> columns,
        clsColumnInfo primaryKey, clsColumnInfo searchColumn)
        {
            var parameters = new List<string>();

            foreach (var column in columns)
            {
                if (column != searchColumn)
                {
                    if (column == primaryKey || column.IsIdentity)
                        parameters.Add($"ref {column.ColumnName}");
                    else
                        parameters.Add($"ref {column.ColumnName}");
                }
            }

            return string.Join(", ", parameters);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using static Code_Generator.Global_Classes.clsGlobal;

namespace Code_Generator.Generator_Classes
{
    public class clsDataAccessLayerGenerator
    {
        private string _connectionString;
        private string _namespaceName;
        private string _outputPath;
        private List<string> _DatabaseTables;
        private clsGeneratorSettings _settingsSearchingFK;
        private clsGeneratorSettings _settingsAddingStaticMethods;


        public clsDataAccessLayerGenerator(string connectionString, string namespaceName,
                                           string outputPath, List<string> DatabaseTables, clsGeneratorSettings settings1 = null, clsGeneratorSettings settings2 = null)
        {
            _connectionString = connectionString;
            _namespaceName = namespaceName;
            _outputPath = outputPath;
            _DatabaseTables = DatabaseTables;
            _settingsSearchingFK = settings1;
            _settingsAddingStaticMethods = settings2;
        }
        public bool GenerateAllDataAccessClasses()
        {
            try
            {
                List<string> tables = _DatabaseTables;

                foreach (var table in tables)
                {
                    GenerateDataAccessClassForTable(table);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in generate classes: {ex.Message}");
                return false;
            }
        }

        private List<clsColumnInfo> GetTableColumns(string tableName)
        {
            var columns = new List<clsColumnInfo>();

            string query = @"
                SELECT 
                    COLUMN_NAME,
                    DATA_TYPE,
                    IS_NULLABLE,
                    CHARACTER_MAXIMUM_LENGTH,
                    ORDINAL_POSITION
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
                        string columnName = reader["COLUMN_NAME"].ToString();

                        var column = new clsColumnInfo
                        {
                            ColumnName = columnName,
                            DataType = reader["DATA_TYPE"].ToString(),
                            IsNullable = reader["IS_NULLABLE"].ToString().ToUpper() == "YES",
                            MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] as int?,
                            IsPrimaryKey = IsPrimaryKey(tableName, columnName)
                        };

                        // Only check Foreign Keys if FKSearchMode = All
                        if (_settingsSearchingFK.FKSearchMode == clsGeneratorSettings.enFKSearchMode.All)
                        {
                            column.IsForeignKey = IsForeignKey(tableName, columnName);

                            if (column.IsForeignKey)
                            {
                                GetForeignKeyInfo(tableName, column.ColumnName,
                                    out string refTable, out string refColumn);
                                column.ReferencedTable = refTable;
                                column.ReferencedColumn = refColumn;
                            }
                        }
                        else // Just This - ignore foreign keys
                        {
                            column.IsForeignKey = false;
                            column.ReferencedTable = null;
                            column.ReferencedColumn = null;
                        }

                        columns.Add(column);
                    }
                }
            }

            return columns;
        }
        // Help Get Name DataAccess Name for class
        private string GetDataAccessClassName(string tableName)
        {
            // People -> clsPersonData
            // Users -> clsUserData
            string singleName = tableName.TrimEnd('s');
            return $"cls{singleName}Data";
        }

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

        private bool IsPrimaryKey(string tableName, string columnName)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                WHERE TABLE_NAME = @TableName 
                    AND COLUMN_NAME = @ColumnName 
                    AND CONSTRAINT_NAME LIKE '%PK%'";

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

        public void GenerateDataAccessClassForTable(string tableName)
        {
            var columns = GetTableColumns(tableName);
            var primaryKey = columns.Find(c => c.IsPrimaryKey);
            var identityColumn = columns.Find(c => c.IsIdentity);

            string className = GetDataAccessClassName(tableName);
            string fileName = $"{className}.cs";
            string filePath = Path.Combine(_outputPath, fileName);

            string classContent = GenerateDataAccessClassContent(
                tableName,className, columns, primaryKey, identityColumn);

            Directory.CreateDirectory(_outputPath);
            File.WriteAllText(filePath, classContent, Encoding.UTF8);

        }

        private string GenerateDataAccessClassContent(string tableName, string className,
                                          List<clsColumnInfo> columns, clsColumnInfo primaryKey, clsColumnInfo identityColumn)
        {
            StringBuilder sb = new StringBuilder();

            // Using statements
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Data.SqlClient;");
            sb.AppendLine();

            // Namespace
            sb.AppendLine($"namespace {_namespaceName}_DataAccess");
            sb.AppendLine("{");
            sb.AppendLine($"    public class cls{className}Data");
            sb.AppendLine("    {");
            sb.AppendLine();

            // 1. GetByID Method
            if (primaryKey.IsPrimaryKey != false)
            {
                sb.AppendLine($"        public static bool Get{className}ByID({GetCSharpType(primaryKey.DataType, primaryKey.IsNullable)} {primaryKey.ColumnName}, " +
                             $"{GenerateParametersForGetMethod(columns, primaryKey.ColumnName)})");
                sb.AppendLine("        {");
                sb.AppendLine("            bool isFound = false;");
                sb.AppendLine();
                sb.AppendLine("            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);");
                sb.AppendLine();
                sb.AppendLine($"            string query = \"SELECT * FROM {tableName} WHERE {primaryKey.ColumnName} = @{primaryKey.ColumnName}\";");
                sb.AppendLine();
                sb.AppendLine("            SqlCommand command = new SqlCommand(query, connection);");
                sb.AppendLine();
                sb.AppendLine($"            command.Parameters.AddWithValue(\"@{primaryKey.ColumnName}\", {primaryKey.ColumnName});");
                sb.AppendLine();
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                sb.AppendLine("                connection.Open();");
                sb.AppendLine("                SqlDataReader reader = command.ExecuteReader();");
                sb.AppendLine();
                sb.AppendLine("                if (reader.Read())");
                sb.AppendLine("                {");
                sb.AppendLine("                    isFound = true;");
                sb.AppendLine();

                foreach (var column in columns)
                {
                    if (column.ColumnName != primaryKey.ColumnName)
                    {
                        sb.AppendLine($"                    {GenerateReaderCodeForColumn(column)}");
                    }
                }

                sb.AppendLine("                }");
                sb.AppendLine("                else");
                sb.AppendLine("                {");
                sb.AppendLine("                    isFound = false;");
                sb.AppendLine("                }");
                sb.AppendLine();
                sb.AppendLine("                reader.Close();");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine("                Console.WriteLine(\"Error: \" + ex.Message);");
                sb.AppendLine("                isFound = false;");
                sb.AppendLine("            }");
                sb.AppendLine("            finally");
                sb.AppendLine("            {");
                sb.AppendLine("                connection.Close();");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            return isFound;");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            // 2. AddNew Method
            sb.AppendLine($"        public static int AddNew{className}({GenerateParametersForAddMethod(columns, primaryKey)})");
            sb.AppendLine("        {");
            sb.AppendLine("            int newID = -1;");
            sb.AppendLine();
            sb.AppendLine("            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);");
            sb.AppendLine();
            sb.AppendLine($"            string query = @\"INSERT INTO {tableName} ({GetColumnNamesForInsert(columns, primaryKey)}) " +
                         $"\n                             VALUES ({GetParameterNamesForInsert(columns, primaryKey)}); " +
                         "\n                             SELECT SCOPE_IDENTITY();\";");
            sb.AppendLine();
            sb.AppendLine("            SqlCommand command = new SqlCommand(query, connection);");
            sb.AppendLine();

            foreach (var column in columns)
            {
                if (column.ColumnName != primaryKey.ColumnName || !IsIdentityColumn(tableName, column.ColumnName))
                {
                    sb.AppendLine(GenerateParameterCodeForColumn(column));
                }
            }

            sb.AppendLine();
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                connection.Open();");
            sb.AppendLine();
            sb.AppendLine("                object result = command.ExecuteScalar();");
            sb.AppendLine();
            sb.AppendLine("                if (result != null && int.TryParse(result.ToString(), out int insertedID))");
            sb.AppendLine("                {");
            sb.AppendLine("                    newID = insertedID;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                Console.WriteLine(\"Error: \" + ex.Message);");
            sb.AppendLine("            }");
            sb.AppendLine("            finally");
            sb.AppendLine("            {");
            sb.AppendLine("                connection.Close();");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            return newID;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // 3. Update Method
            if (primaryKey.IsPrimaryKey != false)
            {
                sb.AppendLine($"        public static bool Update{className}({GenerateParametersForUpdateMethod(columns)})");
                sb.AppendLine("        {");
                sb.AppendLine("            int rowsAffected = 0;");
                sb.AppendLine();
                sb.AppendLine("            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);");
                sb.AppendLine();
                sb.AppendLine($"            string query = @\"UPDATE {tableName} " +
                             $"SET {GetUpdateSetClause(columns, primaryKey.ColumnName)} " +
                             $"\n                                                  WHERE {primaryKey.ColumnName} = @{primaryKey.ColumnName}\";");
                sb.AppendLine();
                sb.AppendLine("            SqlCommand command = new SqlCommand(query, connection);");
                sb.AppendLine();

                foreach (var column in columns)
                {
                    sb.AppendLine(GenerateParameterCodeForColumn(column));
                }

                sb.AppendLine();
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                sb.AppendLine("                connection.Open();");
                sb.AppendLine("                rowsAffected = command.ExecuteNonQuery();");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine("                Console.WriteLine(\"Error: \" + ex.Message);");
                sb.AppendLine("                return false;");
                sb.AppendLine("            }");
                sb.AppendLine("            finally");
                sb.AppendLine("            {");
                sb.AppendLine("                connection.Close();");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            return (rowsAffected > 0);");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            // 4. Delete Method
            if (primaryKey.IsPrimaryKey != false)
            {
                sb.AppendLine($"        public static bool Delete{className}({GetCSharpType(primaryKey.DataType, primaryKey.IsNullable)} {primaryKey.ColumnName})");
                sb.AppendLine("        {");
                sb.AppendLine("            int rowsAffected = 0;");
                sb.AppendLine();
                sb.AppendLine("            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);");
                sb.AppendLine();
                sb.AppendLine($"            string query = $\"DELETE FROM {tableName} WHERE {primaryKey.ColumnName} = @{primaryKey.ColumnName}\";");
                sb.AppendLine();
                sb.AppendLine("            SqlCommand command = new SqlCommand(query, connection);");
                sb.AppendLine();
                sb.AppendLine($"            command.Parameters.AddWithValue(\"@{primaryKey.ColumnName}\", {primaryKey.ColumnName});");
                sb.AppendLine();
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                sb.AppendLine("                connection.Open();");
                sb.AppendLine("                rowsAffected = command.ExecuteNonQuery();");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine("                Console.WriteLine(\"Error: \" + ex.Message);");
                sb.AppendLine("                return false;");
                sb.AppendLine("            }");
                sb.AppendLine("            finally");
                sb.AppendLine("            {");
                sb.AppendLine("                connection.Close();");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            return (rowsAffected > 0);");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            // 5. GetAll Method
            sb.AppendLine($"        public static DataTable GetAll{className}s()");
            sb.AppendLine("        {");
            sb.AppendLine("            DataTable dt = new DataTable();");
            sb.AppendLine();
            sb.AppendLine("            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);");
            sb.AppendLine();
            sb.AppendLine($"            string query = $\"SELECT * FROM {tableName} ORDER BY 1\";");
            sb.AppendLine();
            sb.AppendLine("            SqlCommand command = new SqlCommand(query, connection);");
            sb.AppendLine();
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                connection.Open();");
            sb.AppendLine("                SqlDataAdapter adapter = new SqlDataAdapter(command);");
            sb.AppendLine("                adapter.Fill(dt);");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                Console.WriteLine(\"Error: \" + ex.Message);");
            sb.AppendLine("            }");
            sb.AppendLine("            finally");
            sb.AppendLine("            {");
            sb.AppendLine("                connection.Close();");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            return dt;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // 6. IsExist Method
            if (primaryKey.IsPrimaryKey != false)
            {
                sb.AppendLine($"        public static bool Is{className}Exist({GetCSharpType(primaryKey.DataType, primaryKey.IsNullable)} {primaryKey.ColumnName})");
                sb.AppendLine("        {");
                sb.AppendLine("            bool isFound = false;");
                sb.AppendLine();
                sb.AppendLine("            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);");
                sb.AppendLine();
                sb.AppendLine($"            string query = $\"SELECT 1 FROM {tableName} WHERE {primaryKey.ColumnName} = @{primaryKey.ColumnName}\";");
                sb.AppendLine();
                sb.AppendLine("            SqlCommand command = new SqlCommand(query, connection);");
                sb.AppendLine();
                sb.AppendLine($"            command.Parameters.AddWithValue(\"@{primaryKey.ColumnName}\", {primaryKey.ColumnName});");
                sb.AppendLine();
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                sb.AppendLine("                connection.Open();");
                sb.AppendLine("                SqlDataReader reader = command.ExecuteReader();");
                sb.AppendLine("                isFound = reader.HasRows;");
                sb.AppendLine("                reader.Close();");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine("                Console.WriteLine(\"Error: \" + ex.Message);");
                sb.AppendLine("                isFound = false;");
                sb.AppendLine("            }");
                sb.AppendLine("            finally");
                sb.AppendLine("            {");
                sb.AppendLine("                connection.Close();");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            return isFound;");
                sb.AppendLine("        }");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
        private string GetCSharpType(string sqlType, bool isNullable)
        {
            string type;
            string lowerSqlType = sqlType.ToLower();

            switch (lowerSqlType)
            {
                case "int":
                    type = "int";
                    break;
                case "tinyint":
                    type = "byte";
                    break;
                case "smallint":
                    type = "short";
                    break;
                case "bigint":
                    type = "long";
                    break;
                case "bit":
                    type = "bool";
                    break;
                case "decimal":
                case "numeric":
                    type = "decimal";
                    break;
                case "float":
                    type = "double";
                    break;
                case "real":
                    type = "float";
                    break;
                case "money":
                    type = "decimal";
                    break;
                case "datetime":
                case "date":
                case "smalldatetime":
                    type = "DateTime";
                    break;
                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                    type = "string";
                    break;
                case "uniqueidentifier":
                    type = "Guid";
                    break;
                case "binary":
                case "varbinary":
                case "image":
                    type = "byte[]";
                    break;
                default:
                    type = "object";
                    break;
            }

            // إضافة ? للنوع إذا كان nullable وليس string
            if (isNullable && type != "string" && type != "byte[]" && type != "object")
            {
                type += "?";
            }

            return type;
        }

        private string GenerateReaderCodeForColumn(clsColumnInfo column)
        {
            string csharpType = GetCSharpType(column.DataType, column.IsNullable);

            if (column.IsNullable && csharpType != "string")
            {
                return $@"if (reader[""{column.ColumnName}""] != DBNull.Value)
                    {{
                        {column.ColumnName} = ({csharpType.Replace("?", "")})reader[""{column.ColumnName}""];
                    }}
                    else
                    {{
                        {column.ColumnName} = null;
                    }}";
            }
            else if (csharpType == "string")
            {
                return $@"{column.ColumnName} = (string)reader[""{column.ColumnName}""];";
            }
            else
            {
                return $@"{column.ColumnName} = ({csharpType})reader[""{column.ColumnName}""];";
            }
        }

        private string GenerateParameterCodeForColumn(clsColumnInfo column)
        {
            string csharpType = GetCSharpType(column.DataType, column.IsNullable);

            if (column.IsNullable && csharpType != "string")
            {
                return $@"            if ({column.ColumnName}.HasValue)
                command.Parameters.AddWithValue(""@{column.ColumnName}"", {column.ColumnName}.Value);
            else
                command.Parameters.AddWithValue(""@{column.ColumnName}"", DBNull.Value);";
            }
            else if (csharpType == "string")
            {
                return $@"            if (!string.IsNullOrEmpty({column.ColumnName}))
                command.Parameters.AddWithValue(""@{column.ColumnName}"", {column.ColumnName});
            else
                command.Parameters.AddWithValue(""@{column.ColumnName}"", DBNull.Value);";
            }
            else
            {
                return $@"            command.Parameters.AddWithValue(""@{column.ColumnName}"", {column.ColumnName});";
            }
        }
        private string GenerateParametersForGetMethod(List<clsColumnInfo> columns, string excludeParam)
        {
            var parameters = new List<string>();
            int count = 0;

            foreach (var column in columns)
            {
                if (column.ColumnName != excludeParam)
                {
                    count++;
                    string type = GetCSharpType(column.DataType, column.IsNullable);
                    parameters.Add($"ref {type} {column.ColumnName}");
                    if (count == 5)
                    {
                        parameters.Add("\n                                                    ");
                        continue;
                    }
                }
            }
            return string.Join(", ", parameters);
        }

        private string GenerateParametersForAddMethod(List<clsColumnInfo> columns, clsColumnInfo primaryKey)
        {
            var parameters = new List<string>();
            int count = 0;
            foreach (var column in columns)
            {
                if (column.ColumnName != primaryKey.ColumnName || !IsIdentityColumn("", column.ColumnName))
                {
                    count++;
                    string type = GetCSharpType(column.DataType, column.IsNullable);
                    parameters.Add($"{type} {column.ColumnName}");
                    if (count == 6)
                    {
                        parameters.Add("\n                                          ");
                        continue;
                    }
                }
            }
            return string.Join(", ", parameters);
        }

        private bool IsIdentityColumn(string tableName, string columnName)
        {
            // هذا مثال مبسط، يمكن تحسينه
            return columnName.ToLower().EndsWith("id") ||
                   columnName.ToLower() == "id" ||
                   columnName.ToLower() == "personid";
        }

        private string GetColumnNamesForInsert(List<clsColumnInfo> columns, clsColumnInfo primaryKey)
        {
            var columnNames = new List<string>();
            foreach (var column in columns)
            {
                if (column.ColumnName != primaryKey.ColumnName || !IsIdentityColumn("", column.ColumnName))
                {
                    columnNames.Add(column.ColumnName);
                }
            }
            return string.Join(", ", columnNames);
        }

        private string GetParameterNamesForInsert(List<clsColumnInfo> columns, clsColumnInfo primaryKey)
        {
            var paramNames = new List<string>();
            foreach (var column in columns)
            {
                if (column.ColumnName != primaryKey.ColumnName || !IsIdentityColumn("", column.ColumnName))
                {
                    paramNames.Add($"@{column.ColumnName}");
                }
            }
            return string.Join(", ", paramNames);
        }

        private string GenerateParametersForUpdateMethod(List<clsColumnInfo> columns)
        {
            var parameters = new List<string>();
            int count = 0;
            foreach (var column in columns)
            {
                count++;
                string type = GetCSharpType(column.DataType, column.IsNullable);
                parameters.Add($"{type} {column.ColumnName}");
                if (count == 7)
                {
                    parameters.Add("\n                                           ");
                    continue;
                }
            }
            return string.Join(", ", parameters);
        }

        private string GetUpdateSetClause(List<clsColumnInfo> columns, string primaryKeyName)
        {
            var setClauses = new List<string>();
            foreach (var column in columns)
            {
                if (column.ColumnName != primaryKeyName)
                {
                    setClauses.Add($"{column.ColumnName} = @{column.ColumnName}");
                }
            }
            return string.Join(", \n                                                  ", setClauses);
        }



    }

}

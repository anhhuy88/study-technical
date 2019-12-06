private void GenerateDatabase(Guid customerId, string companyName)
        {
            var dbName = companyName.Replace(".", "") + "_Customer";
            var pathFileScript = Path.Combine(@"E:\Research\MVC\NetcoreEf\NetcoreEfTests", "generate_database.sql");
            var strGenerate = File.ReadAllText(pathFileScript);

            var strCommand = $"RESTORE FILELISTONLY FROM  DISK = N'{_dbServer.FileBackup}'";
            var strServerProperty = "SELECT SERVERPROPERTY('INSTANCEDEFAULTDATAPATH') as [DefaultDataPath], SERVERPROPERTY('INSTANCEDEFAULTLOGPATH') as [DefaultLogPath]";

            string defaultDataPath = string.Empty;
            string defaultLogPath = string.Empty;

            string localNameData = string.Empty;
            string locaNameLog = string.Empty;

            using (var con = new SqlConnection($"Server={_dbServer.Server};User ID={_dbServer.UserId};Password={_dbServer.Password}"))
            {
                con.Open();
                var fields = new Dictionary<string, int>();
                // get logical name
                var command = new SqlCommand(strCommand, con);
                var reader = command.ExecuteReader();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    fields[reader.GetName(i)] = i;
                }
                while (reader.Read())
                {
                    var fileType = reader.GetString(fields["Type"]);
                    if (string.IsNullOrWhiteSpace(localNameData) && fileType.Equals("D"))
                    {
                        localNameData = reader.GetString(fields["LogicalName"]);
                    }
                    if (string.IsNullOrWhiteSpace(locaNameLog) && fileType.Equals("L"))
                    {
                        locaNameLog = reader.GetString(fields["LogicalName"]);
                    }
                }
                reader.Close();
                command.Dispose();
                if (con.State != System.Data.ConnectionState.Open) con.Open();
                // Get server property
                command = new SqlCommand(strServerProperty, con);
                reader = command.ExecuteReader();
                fields.Clear();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    fields[reader.GetName(i)] = i;
                }
                while (reader.Read())
                {
                    if (string.IsNullOrWhiteSpace(defaultDataPath))
                    {
                        defaultDataPath = reader.GetString(fields["DefaultDataPath"]);
                    }
                    if (string.IsNullOrWhiteSpace(defaultLogPath))
                    {
                        defaultLogPath = reader.GetString(fields["DefaultLogPath"]);
                    }
                }
                reader.Close();
                command.Dispose();
                if (con.State != System.Data.ConnectionState.Open) con.Open();
                // Generate database
                strGenerate = strGenerate.Replace("{dbName}", dbName)
                    .Replace("{fileBackup}", _dbServer.FileBackup)
                    .Replace("{localNameData}", localNameData)
                    .Replace("{localNameLog}", locaNameLog)
                    .Replace("{defaultDataPath}", defaultDataPath)
                    .Replace("{defaultLogPath}", defaultLogPath)
                    // new connection
                    .Replace("{DBBASE}", _dbServer.DbBase)
                    .Replace("{CUSTOMERID}", customerId.ToString())
                    .Replace("{NAME}", $"{companyName} - Connection")
                    .Replace("{INSTANCE}", _dbServer.Server)
                    .Replace("{CATALOG}", dbName)
                    .Replace("{USER}", _dbServer.UserId)
                    .Replace("{PASSWORD}", _dbServer.Password);
                command = new SqlCommand(strGenerate, con);
                var result = command.ExecuteNonQuery();
            }
        }
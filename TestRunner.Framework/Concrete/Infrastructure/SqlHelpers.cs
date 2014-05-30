using System;
using System.IO;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace TestRunner.Framework.Concrete.Infrastructure
{
    public static class SqlHelpers
    {
        public static void BackupDatabase(string databaseName, string userName, string password, string serverName,
            string destinationPath, string backUpName)
        {
            Console.WriteLine("started backing up {0}", databaseName);
            //Define a Backup object variable.
            var sqlBackup = new Backup
            {
                Action = BackupActionType.Database,
                BackupSetDescription = "BackUp of:" + databaseName + "on" + DateTime.Now.ToShortDateString(),
                BackupSetName = backUpName,
                Database = databaseName
            };

            //Specify the type of backup, the description, the name, and the database to be backed up.

            //Declare a BackupDeviceItem
            var deviceItem = new BackupDeviceItem(destinationPath + backUpName + ".bak", DeviceType.File);
            //Define Server connection
            var connection = new ServerConnection(serverName, userName, password);
            //To Avoid TimeOut Exception
            var sqlServer = new Server(connection);
            sqlServer.ConnectionContext.StatementTimeout = 200*200;

            sqlBackup.Initialize = true;
            sqlBackup.Checksum = true;
            sqlBackup.ContinueAfterError = true;

            //Add the device to the Backup object.
            sqlBackup.Devices.Add(deviceItem);
            //Set the Incremental property to False to specify that this is a full database backup.
            sqlBackup.Incremental = false;


            //sqlBackup.ExpirationDate = DateTime.Now.AddDays(3);
            //Specify that the log must be truncated after the backup is complete.
            sqlBackup.LogTruncation = BackupTruncateLogType.Truncate;

            sqlBackup.FormatMedia = false;
            //Run SqlBackup to perform the full database backup on the instance of SQL Server.
            sqlBackup.SqlBackup(sqlServer);
            //Remove the backup device from the Backup object.
            sqlBackup.Devices.Remove(deviceItem);
            Console.WriteLine("finished backing up {0}", databaseName);

        }

        public static void RestoreDatabase(string serverName, string databaseName, string filePath, string userName,
            string password)
        {
            Console.WriteLine("started restoring up {0}", databaseName);

            var conn = new ServerConnection(serverName, userName, password)
            {
                ConnectTimeout = 200*200
            };
            var srv = new Server(conn);
            srv.ConnectionContext.StatementTimeout = 200*200;
            try
            {
                var res = new Restore();

                res.Devices.AddDevice(filePath, DeviceType.File);

                var dataFile = new RelocateFile
                {
                    LogicalFileName = res.ReadFileList(srv).Rows[0][0].ToString(),
                    PhysicalFileName = srv.Databases[databaseName].FileGroups[0].Files[0].FileName
                };

                var logFile = new RelocateFile
                {
                    LogicalFileName = res.ReadFileList(srv).Rows[1][0].ToString(),
                    PhysicalFileName = srv.Databases[databaseName].LogFiles[0].FileName
                };

                res.RelocateFiles.Add(dataFile);
                res.RelocateFiles.Add(logFile);

                res.Database = databaseName;
                res.NoRecovery = false;
                res.ReplaceDatabase = true;
                srv.KillAllProcesses(databaseName);
                res.SqlRestore(srv);
                conn.Disconnect();

                Console.WriteLine("finished restoring up {0}", databaseName);

            }
            catch (SmoException ex)
            {
                throw new SmoException(ex.Message, ex.InnerException);
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message, ex.InnerException);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TestRunner.Framework.Concrete.Enum;
using TestRunner.Framework.Concrete.Manager;

namespace TestRunner.Framework.Concrete.Infrastructure
{
    /// <summary>
    /// Please use me for database persistence/loading!
    /// </summary>
    public class StoredProcedure : IDisposable
    {
        #region Members

        private SqlConnection DbConnection { get; set; }
        private SqlCommand DbCommand { get; set; }
        private string ProcedureName { get; set; }
        private IList<SqlParameter> Parameters { get; set; }
        public int Timeout { get; set; }

        #endregion

        #region Private Constructors

        private StoredProcedure(DataBase database)
        {
            switch (database)
            {
                case DataBase.Default:
                    DbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                    break;
            }
        }

        private StoredProcedure(string connString)
        {
            DbConnection = new SqlConnection(connString);
        }

        #endregion

        #region Constructors

        public StoredProcedure(DataBase database, string procName)
            : this(database)
        {
            ProcedureName = procName;
            Parameters = new List<SqlParameter>();
        }

        public StoredProcedure(DataBase database, string procName, SqlParameter parameter)
            : this(database)
        {
            ProcedureName = procName;
            Parameters = new List<SqlParameter> { parameter };
        }

        public StoredProcedure(DataBase database, string procName, IList<SqlParameter> parameters)
            : this(database)
        {
            ProcedureName = procName;
            Parameters = parameters;
        }

        public StoredProcedure(string connection, string procName)
            : this(connection)
        {
            ProcedureName = procName;
            Parameters = new List<SqlParameter>();
        }

        public StoredProcedure(string connection, string procName, SqlParameter parameter)
            : this(connection)
        {
            ProcedureName = procName;
            Parameters = new List<SqlParameter> { parameter };
        }

        public StoredProcedure(string connection, string procName, IList<SqlParameter> parameters)
            : this(connection)
        {
            ProcedureName = procName;
            Parameters = parameters;
        }

        public StoredProcedure(DataBase database, string procName, IList<SqlParameter> parameters, SqlParameter parameter)
            : this(database)
        {
            ProcedureName = procName;
            Parameters = parameters;
            Parameters.Add(parameter);
        }

        #endregion

        #region Public Methods

        public SqlDataReader GetDataReader()
        {
            SqlDataReader dataReader;
            DbCommand = new SqlCommand(ProcedureName, DbConnection) { CommandType = CommandType.StoredProcedure };

            if (Parameters.Count > 0)
            {
                DbCommand.Parameters.AddRange(Parameters.ToArray());
            }

            try
            {
                DbCommand.CommandTimeout = Timeout;
                DbCommand.Connection.Open();
                dataReader = DbCommand.ExecuteReader();
            }
            catch (Exception exception)
            {
                Dispose();
                Log4NetLogger.LogEntry(GetType(), "GetDataReader", "GetDataReaderError", LoggerLevel.Error, exception);
                throw;
            }

            return dataReader;
        }

        public object GetScalar()
        {
            object value;

            DbCommand = new SqlCommand(ProcedureName, DbConnection) { CommandType = CommandType.StoredProcedure };

            if (Parameters.Count > 0)
            {
                DbCommand.Parameters.AddRange(Parameters.ToArray());
            }

            try
            {
                DbCommand.CommandTimeout = Timeout;
                DbCommand.Connection.Open();
                value = DbCommand.ExecuteScalar();
            }
            finally
            {
                Dispose();
            }
            return value;
        }

        public void Execute()
        {
            DbCommand = new SqlCommand(ProcedureName, DbConnection) { CommandType = CommandType.StoredProcedure };

            if (Parameters.Count > 0)
            {
                DbCommand.Parameters.AddRange(Parameters.ToArray());
            }

            try
            {
                DbCommand.CommandTimeout = Timeout;
                DbCommand.Connection.Open();
                DbCommand.ExecuteNonQuery();
            }
            finally
            {
                Dispose();
            }
        }

        public DataTable GetDataTable()
        {
            DataTable dtResults = new DataTable();
            DbCommand = new SqlCommand(ProcedureName, DbConnection) { CommandType = CommandType.StoredProcedure };

            if (Parameters.Count > 0)
            {
                DbCommand.Parameters.AddRange(Parameters.ToArray());
            }

            try
            {
                DbCommand.CommandTimeout = Timeout;
                DbCommand.Connection.Open();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(DbCommand);
                dataAdapter.Fill(dtResults);
            }
            finally
            {
                Dispose();
            }

            return dtResults;
        }

        public DataSet GetDataSet(IEnumerable<string> tableNames)
        {
            DataSet dataSet = new DataSet();
            DbCommand = new SqlCommand(ProcedureName, DbConnection) { CommandType = CommandType.StoredProcedure };

            if (Parameters.Count > 0)
            {
                DbCommand.Parameters.AddRange(Parameters.ToArray());
            }

            try
            {
                DbCommand.CommandTimeout = Timeout;
                DbCommand.Connection.Open();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(DbCommand);

                foreach (string tableName in tableNames)
                {
                    dataSet.Tables.Add(tableName);
                    dataAdapter.Fill(dataSet, tableName);
                }
            }
            catch (Exception exception)
            {
                Dispose();
                Log4NetLogger.LogEntry(GetType(), "GetDataSet", "GetDataSetError", LoggerLevel.Error, exception);
                throw;
            }

            return dataSet;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            DbCommand.Dispose();
            if (DbConnection.State == ConnectionState.Open)
            {
                DbConnection.Close();
            }
            DbConnection.Dispose();
        }

        #endregion

    }
}

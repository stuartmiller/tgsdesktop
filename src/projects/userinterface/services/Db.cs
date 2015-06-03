using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.services {
    internal class Db {

        protected const string SINGLE_KEY_ID_PARAM_NAME = "@ids";

        internal Db() {
            this.Cn = new SqlConnection(this.ConnectionString);
            this.Command = Cn.CreateCommand();
        }

        string ConnectionString {
            get { return Config.Instance.ConnectionString; }
        }

        SqlConnection Cn { get; set; }
        internal SqlCommand Command { get; set; }

        internal void Reset() {
            if (Cn.State == ConnectionState.Open)
                Cn.Close();
            if (this.Command != null)
                this.Command.Dispose();
            this.Command = Cn.CreateCommand();
        }

        internal SqlDataReader ExecuteReader(CommandBehavior behavior = CommandBehavior.CloseConnection) {
            this.InitializeCommand();

            if (this.Cn.State != ConnectionState.Open)
                this.Cn.Open();
            return this.Command.ExecuteReader(behavior);
        }

        internal Task<SqlDataReader> ExecuteReaderAsync(CommandBehavior behavior = CommandBehavior.CloseConnection) {
            this.InitializeCommand();

            if (this.Cn.State != ConnectionState.Open)
                this.Cn.Open();

            return this.Command.ExecuteReaderAsync(behavior);
        }

        internal int Execute() {
            this.InitializeCommand();
            var cnState = this.Cn.State;
            if (cnState != ConnectionState.Open)
                this.Cn.Open();
            try {
                return this.Command.ExecuteNonQuery();
            } catch {
                throw;
            } finally {
                if (cnState == ConnectionState.Closed)
                    this.Cn.Close();
            }

        }

        internal T ExecuteScaler<T>() {
            this.InitializeCommand();
            var cnState = this.Cn.State;
            if (cnState != ConnectionState.Open)
                this.Cn.Open();
            try {
                var retVal = this.Command.ExecuteScalar();
                if (retVal == DBNull.Value || retVal == null)
                    return default(T);
                return (T)retVal;
            } catch {
                throw;
            } finally {
                if (cnState == ConnectionState.Closed)
                    this.Cn.Close();
            }

        }

        void InitializeCommand() {
            if (this.Cn == null)
                this.Cn = new SqlConnection(this.ConnectionString);
            if (this.Command == null)
                this.Command = this.Cn.CreateCommand();
            if (this.Command.Connection == null)
                this.Command.Connection = this.Cn;
            if (this.Command.CommandText.StartsWith("proc_"))
                this.Command.CommandType = CommandType.StoredProcedure;
        }

        protected void AppendSingleKeyTableParameter(IEnumerable<int> idArray) {

            int index = this.Command.Parameters.IndexOf(SINGLE_KEY_ID_PARAM_NAME);
            if (index > 0)
                this.Command.Parameters.RemoveAt(index);

            var sqlDataRecords = idArray.Distinct().Select(x => {
                var dataRecord = new SqlDataRecord(new SqlMetaData[] {
                    new SqlMetaData("id", SqlDbType.Int)
                });
                dataRecord.SetInt32(0, x);
                return dataRecord;
            });

            var pkParam = this.Command.Parameters.AddWithValue(SINGLE_KEY_ID_PARAM_NAME, sqlDataRecords);
            pkParam.SqlDbType = SqlDbType.Structured;
            pkParam.TypeName = "udt_intIdArray";
        }
    }
}

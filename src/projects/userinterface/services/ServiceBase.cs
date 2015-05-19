using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.services {
    public abstract class ServiceBase {

        protected const string SINGLE_KEY_ID_PARAM_NAME = "@ids";

        protected static models.User User { get; set; }

        public ServiceBase() {
            this.Cn = new SqlConnection(this.ConnectionString);
            this.Command = Cn.CreateCommand();
        }
        private static string FormatResourceName(Assembly assembly, string resourceName) {
            return assembly.GetName().Name + "." + resourceName.Replace(" ", "_")
                                                               .Replace("\\", ".")
                                                               .Replace("/", ".");
        }

        string ConnectionString {
            get {
                var connectionString = ConfigurationManager.ConnectionStrings["tgs"].ConnectionString;
                var passphrase = utilities.ResourceAccessor.GetStringResource("Passphrase");
                var decriptor = new utilities.RijndaelEnhanced(passphrase, connectionString.Substring(0, 16));
                return decriptor.Decrypt(connectionString.Substring(16, connectionString.Length - 16));
            }
        }

        SqlConnection Cn { get; set; }
        public SqlCommand Command { get; set; }
        
        public void Reset(){
            if (Cn.State == ConnectionState.Open)
                Cn.Close();
            if (this.Command != null)
                this.Command.Dispose();
            this.Command = Cn.CreateCommand();
        }

        public SqlDataReader ExecuteReader(CommandBehavior behavior = CommandBehavior.CloseConnection) {
            this.InitializeCommand();

            if (this.Cn.State != ConnectionState.Open)
                this.Cn.Open();
            return this.Command.ExecuteReader(behavior);
        }

        public int Execute() {
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

        public T ExecuteScaler<T>() {
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

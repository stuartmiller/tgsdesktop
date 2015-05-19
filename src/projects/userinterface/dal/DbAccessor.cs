using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.services {
    public class DbAccessor : IDisposable {
        readonly string _connString = "DataSource=leaf.arvixe.com;Initial Catalog=tgs;User Id=stuart;Password=d33pbr34th";

        public DbAccessor() { }

        public SqlCommand Command { get; private set; }

        public void Dispose()
        {
            try {
                if (this.Command != null) {
                    if (this.Command.Connection != null) {
                        if (this.Command.Connection.State == ConnectionState.Open)
                            this.Command.Connection.Close();
                    }
                }
            } catch { } finally {
                try {
                    this.Command.Connection.Dispose();
                    this.Command.Connection = null;
                } catch { }
            }
        }

        public SqlDataReader ExecuteReader(CommandBehavior behavior = CommandBehavior.CloseConnection)
        {
            this.InitializeCommand();
            try {
                if (this.Command.Connection.State == System.Data.ConnectionState.Closed)
                    this.Command.Connection.Open();
                return this.Command.ExecuteReader(behavior);
            } catch (Exception) {
                if (this.Command.Connection.State != ConnectionState.Closed)
                    this.Command.Connection.Close();
                throw;
            }
        }

        public T ExecuteScalar<T>() {
            this.InitializeCommand();
            try {
                if (this.Command.Connection.State == ConnectionState.Closed)
                    this.Command.Connection.Open();
                var retVal = this.Command.ExecuteScalar();
                if (retVal == DBNull.Value)
                    return default(T);
                return (T)retVal;
            } catch (Exception) {
                if (this.Command.Connection.State != ConnectionState.Closed)
                    this.Command.Connection.Close();
                throw;
            }
        }

        public int Execute() {
            this.InitializeCommand();
            try {
                if (this.Command.Connection.State == ConnectionState.Closed)
                    this.Command.Connection.Open();
                var retVal = this.Command.ExecuteNonQuery();
                return retVal;
            } catch (Exception) {
                if (this.Command.Connection.State != ConnectionState.Closed)
                    this.Command.Connection.Close();
                throw;
            }
        }

        void InitializeCommand() {
            if (this.Command.Connection == null) {
                var cn = new SqlConnection(_connString);
                this.Command.Connection = cn;
            }
        }

        public void Reset() {
            if (this.Command.Connection != null) {
                try {
                    if (this.Command.Connection.State != System.Data.ConnectionState.Closed)
                        this.Command.Connection.Close();
                } finally {
                    try { this.Command.Connection.Dispose(); } catch { }
                }
            }
            this.Command = new SqlCommand();
        }

    }
}

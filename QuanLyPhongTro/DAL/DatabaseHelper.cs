using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace QuanLyPhongTro.DAL
{
    /// <summary>
    /// Database helper với Transaction support và connection management
    /// </summary>
    public class DatabaseHelper : IDisposable
    {
        private static readonly string _connStr = ConfigurationManager.ConnectionStrings["QuanLyPhongTroDB"]?.ConnectionString
            ?? throw new InvalidOperationException("Connection string 'QuanLyPhongTroDB' not found in App.config");

        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private bool _disposed;

        #region Static Methods (Backward compatible, Auto-commit)

        public static DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }
        }

        #endregion

        #region Instance Methods (Transaction support)

        public void BeginTransaction()
        {
            _connection = new SqlConnection(_connStr);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            try
            {
                _transaction?.Commit();
            }
            finally
            {
                CloseConnection();
            }
        }

        public void Rollback()
        {
            try
            {
                _transaction?.Rollback();
            }
            finally
            {
                CloseConnection();
            }
        }

        public int ExecuteNonQueryWithTransaction(string query, params SqlParameter[] parameters)
        {
            if (_connection == null || _transaction == null)
                throw new InvalidOperationException("Transaction has not been started. Call BeginTransaction() first.");

            using (var cmd = new SqlCommand(query, _connection, _transaction))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public object ExecuteScalarWithTransaction(string query, params SqlParameter[] parameters)
        {
            if (_connection == null || _transaction == null)
                throw new InvalidOperationException("Transaction has not been started. Call BeginTransaction() first.");

            using (var cmd = new SqlCommand(query, _connection, _transaction))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }

        private void CloseConnection()
        {
            _transaction?.Dispose();
            _transaction = null;

            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CloseConnection();
                }
                _disposed = true;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Tạo SqlParameter an toàn, xử lý null value
        /// </summary>
        public static SqlParameter CreateParameter(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        /// <summary>
        /// Kiểm tra kết nối database
        /// </summary>
        public static bool TestConnection()
        {
            try
            {
                using (var conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
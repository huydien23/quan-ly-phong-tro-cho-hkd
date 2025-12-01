using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace QuanLyPhongTro.DAL
{
    /// <summary>
    /// Base Repository với các method chung cho tất cả DAL
    /// </summary>
    public abstract class BaseRepository
    {
        /// <summary>
        /// Map DataRow to DTO object
        /// </summary>
        protected T MapToObject<T>(DataRow row) where T : new()
        {
            var obj = new T();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (row.Table.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                {
                    try
                    {
                        var value = Convert.ChangeType(row[prop.Name], prop.PropertyType);
                        prop.SetValue(obj, value);
                    }
                    catch
                    {
                        // Skip if conversion fails
                    }
                }
            }
            return obj;
        }

        /// <summary>
        /// Map DataTable to List of DTO
        /// </summary>
        protected List<T> MapToList<T>(DataTable dt) where T : new()
        {
            var list = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapToObject<T>(row));
            }
            return list;
        }

        /// <summary>
        /// Shortcut để tạo parameter
        /// </summary>
        protected SqlParameter Param(string name, object value)
        {
            return DatabaseHelper.CreateParameter(name, value);
        }
    }
}

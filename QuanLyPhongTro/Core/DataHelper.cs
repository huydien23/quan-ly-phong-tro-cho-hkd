using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace QuanLyPhongTro.Core
{
    public static class DataHelper
    {
        // Hàm thần thánh chuyển DataTable -> List<T>
        public static List<T> ToList<T>(this DataTable dt) where T : new()
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = new T();
                foreach (PropertyInfo prop in item.GetType().GetProperties())
                {
                    // Kiểm tra nếu DataTable có cột trùng tên với Property của DTO
                    if (dt.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        // Convert dữ liệu an toàn
                        object value = row[prop.Name];
                        prop.SetValue(item, Convert.ChangeType(value, prop.PropertyType), null);
                    }
                }
                data.Add(item);
            }
            return data;
        }
    }
}
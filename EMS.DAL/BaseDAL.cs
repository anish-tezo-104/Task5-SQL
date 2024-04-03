using System.Data.SqlClient;
using System.Reflection;
using EMS.DAL.Interfaces;

namespace EMS.DAL;
public class BaseDAL : IBaseDAL
{

    public List<T>? LoadDataToList<T>(SqlDataReader reader)
    {
        List<T> dataList = [];

        try
        {
            while (reader.Read())
            {
                T data = Activator.CreateInstance<T>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    PropertyInfo? prop = typeof(T).GetProperty(reader.GetName(i));
                    if (prop != null && !reader.IsDBNull(i))
                    {
                        object value = reader.GetValue(i);
                        Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        prop.SetValue(data, Convert.ChangeType(value, propType));
                    }
                }
                dataList.Add(data);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error executing SQL query: {ex.Message}", ex);
        }
        finally
        {
            reader.Close();
        }

        return dataList ?? null;
    }

}
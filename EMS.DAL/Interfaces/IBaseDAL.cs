
using System.Data.SqlClient;

namespace EMS.DAL.Interfaces;

public interface IBaseDAL
{
    public List<T>? LoadDataToList<T>(SqlDataReader reader);
}
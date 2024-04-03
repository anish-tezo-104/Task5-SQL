using System.Data.SqlClient;
using EMS.DAL.DBO;
using EMS.DAL.DTO;

namespace EMS.DAL.Interfaces;

public interface IDropdownDAL
{
    List<Dropdown>? GetLocationsList();
    List<Dropdown>? GetDepartmentsList();
    List<Dropdown>? GetManagersList();
    List<Dropdown>? GetProjectsList();
    List<Role>? GetRolesList();
    Dictionary<int, string>? GetRoleNamesByDepartmentId(int? departmentId);
    SqlDataReader ExecuteQuery(string query);
}
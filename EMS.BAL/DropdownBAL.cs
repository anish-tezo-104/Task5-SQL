using EMS.BAL.Interfaces;
using EMS.DAL.DBO;
using EMS.DAL.DTO;
using EMS.DAL.Interfaces;

namespace EMS.BAL;

public class DropdownBAL : IDropdownBAL
{
    private readonly IDropdownDAL _dropdownDAL;

    public DropdownBAL(IDropdownDAL dropdownDAL)
    {
        _dropdownDAL = dropdownDAL;
    }
    
    public Dictionary<int, string> GetDepartments()
    {
        List<Dropdown> departmentsList = _dropdownDAL.GetDepartmentsList() ?? [];
        if (departmentsList == null)
        {
            return [];
        }
        Dictionary<int, string> departments = ConvertListToDictionary(departmentsList);
        return departments;
    }

    public Dictionary<int, string> GetLocations()
    {
        List<Dropdown> locationsList = _dropdownDAL.GetLocationsList() ?? [];
        Dictionary<int, string> locations = ConvertListToDictionary(locationsList);
        return locations;
    }

    public Dictionary<int, string> GetManagers()
    {
        List<Dropdown> managersList = _dropdownDAL.GetManagersList() ?? [];
        Dictionary<int, string> managers = ConvertListToDictionary(managersList);
        return managers;
    }

    public Dictionary<int, string> GetProjects()
    {
        List<Dropdown> projectsList = _dropdownDAL.GetProjectsList() ?? [];
        Dictionary<int, string> projects = ConvertListToDictionary(projectsList);
        return projects;
    }

    public Dictionary<int, string> GetRoles()
    {
        List<Role> rolesList = _dropdownDAL.GetRolesList() ?? [];
        if (rolesList == null)
        {
            return [];
        }
        Dictionary<int, string> roles = ConvertListToDictionary(rolesList);
        return roles;
    }

    public Dictionary<int, string>? GetRoleNamesByDepartmentId(int? departmentId)
    {
        Dictionary<int, string>? roles = _dropdownDAL.GetRoleNamesByDepartmentId(departmentId) ?? [];
        return roles;
    }

    private static Dictionary<int, string> ConvertListToDictionary<T>(List<T> items)
    {
        if (items == null)
        {
            return [];
        }

        var dictionary = new Dictionary<int, string>();
        foreach (var item in items)
        {
            var idProperty = typeof(T).GetProperty("Id");
            var nameProperty = typeof(T).GetProperty("Name");

            if (idProperty != null && nameProperty != null)
            {
                var idValue = idProperty.GetValue(item);
                var nameValue = nameProperty.GetValue(item);

                if (idValue != null && nameValue != null && idValue is int && nameValue is string)
                {
                    dictionary[(int)idValue] = (string)nameValue;
                }
            }
        }
        return dictionary;
    }
}
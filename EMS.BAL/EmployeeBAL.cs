using EMS.BAL.Interfaces;
using EMS.Common.Logging;
using EMS.DAL.DTO;
using EMS.DAL.Interfaces;

namespace EMS.BAL;

public class EmployeeBAL : IEmployeeBAL
{
    private readonly IEmployeeDAL _employeeDal;
    private readonly IDropdownBAL _dropdownBal;
    private readonly ILogger _logger;

    public EmployeeBAL(ILogger logger, IEmployeeDAL employeeDal, IDropdownBAL dropdownBal)
    {
        _employeeDal = employeeDal;
        _dropdownBal = dropdownBal;
        _logger = logger;
    }

    public List<EmployeeDetails>? GetAll()
    {
        List<EmployeeDetails> employees;
        employees = _employeeDal.RetrieveAll() ?? [];
        return employees;
    }

    public bool AddEmployee(EmployeeDetails employee)
    {
        return _employeeDal.Insert(employee);
    }

    public bool DeleteEmployee(int id)
    {
        return _employeeDal.Delete(id);
    }

    public bool UpdateEmployee(int id, EmployeeDetails employee)
    {
        return _employeeDal.Update(id, employee);
    }

    public List<EmployeeDetails> GetEmployeeById(int id)
    {
        return _employeeDal.GetEmployeeById(id);
    }

    public List<EmployeeDetails>? SearchEmployees(string keyword)
    {
        List<EmployeeDetails> searchedEmployees;
        EmployeeFilters filters = new()
        {
            Search = keyword
        };

        searchedEmployees = _employeeDal.Filter(filters) ?? [];
        if (searchedEmployees != null && searchedEmployees.Count > 0)
        {
            searchedEmployees = GetEmployeeDetails(searchedEmployees);
        }
        return searchedEmployees;
    }

    public List<EmployeeDetails>? FilterEmployees(EmployeeFilters filters)
    {
        List<EmployeeDetails> filteredEmployees;
        filteredEmployees = _employeeDal.Filter(filters) ?? [];
        if (filteredEmployees != null && filteredEmployees.Count > 0)
        {
            filteredEmployees = GetEmployeeDetails(filteredEmployees);
        }
        return filteredEmployees;
    }

    public int CountEmployees()
    {
        return _employeeDal.Count();
    }

    private List<EmployeeDetails> GetEmployeeDetails(List<EmployeeDetails> employees)
    {
        if (employees == null || employees.Count == 0)
        {
            return [];
        }

        Dictionary<int, string> locationNames = _dropdownBal.GetLocations();
        Dictionary<int, string> roleNames = _dropdownBal.GetRoles();
        Dictionary<int, string> departmentNames = _dropdownBal.GetDepartments();
        Dictionary<int, string> managerNames = _dropdownBal.GetManagers();
        Dictionary<int, string> projectNames = _dropdownBal.GetProjects();


        foreach (EmployeeDetails employee in employees)
        {
            employee.LocationName = employee.LocationId.HasValue && locationNames.TryGetValue(employee.LocationId.Value, out string? locationValue) ? locationValue : null;
            employee.RoleName = employee.RoleId.HasValue && roleNames.TryGetValue(employee.RoleId.Value, out string? roleValue) ? roleValue : null;
            employee.DepartmentName = employee.DepartmentId.HasValue && departmentNames.TryGetValue(employee.DepartmentId.Value, out string? departmentValue) ? departmentValue : null;
            employee.ManagerName = employee.ManagerId.HasValue && managerNames.TryGetValue(employee.ManagerId.Value, out string? managerValue) ? managerValue : null;
            employee.ProjectName = employee.ProjectId.HasValue && projectNames.TryGetValue(employee.ProjectId.Value, out string? projectValue) ? projectValue : null;
        }
        return employees;
    }
}


using EMS.BAL.Interfaces;
using EMS.Common.Logging;
using EMS.DAL.DTO;
using EMS.DAL.Interfaces;

namespace EMS.BAL;

public class EmployeeBAL : IEmployeeBAL
{
    private readonly IEmployeeDAL _employeeDal;
    private readonly ILogger _logger;

    public EmployeeBAL(ILogger logger, IEmployeeDAL employeeDal)
    {
        _employeeDal = employeeDal;
        _logger = logger;
    }

    public List<EmployeeDetails>? GetAll()
    {
        List<EmployeeDetails> employees;
        employees = _employeeDal.RetrieveAll() ?? [];
        return employees;
    }

    public int AddEmployee(EmployeeDetails employee)
    {
        return _employeeDal.Insert(employee);
    }

    public int DeleteEmployee(int id)
    {
        return _employeeDal.Delete(id);
    }

    public int UpdateEmployee(int id, EmployeeDetails employee)
    {
        return _employeeDal.Update(id, employee);
    }

    public List<EmployeeDetails> GetEmployeeById(int id)
    {
        EmployeeFilters filters = new()
        {
            Search = id.ToString(),
        };
        return _employeeDal.Filter(filters) ?? [];
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
            return searchedEmployees;
        }
        return [];
    }

    public List<EmployeeDetails>? FilterEmployees(EmployeeFilters filters)
    {
        List<EmployeeDetails> filteredEmployees;
        filteredEmployees = _employeeDal.Filter(filters) ?? [];
        if (filteredEmployees != null && filteredEmployees.Count > 0)
        {
            return filteredEmployees;
        }
        return [];
    }

    public int CountEmployees()
    {
        return _employeeDal.Count();
    }
}


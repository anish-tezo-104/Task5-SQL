
using EMS.DAL.DBO;
using EMS.DAL.DTO;

namespace EMS.BAL.Interfaces;

public interface IEmployeeBAL
{
    public List<EmployeeDetails>? GetAll();
    public bool AddEmployee(EmployeeDetails employee);
    public bool DeleteEmployee(int id);
    public bool UpdateEmployee(int id, EmployeeDetails employee);
    public List<EmployeeDetails> GetEmployeeById(int id);
    public List<EmployeeDetails>? SearchEmployees(string keyword);
    public List<EmployeeDetails>? FilterEmployees(EmployeeFilters filters);
    public int CountEmployees();
}
using EMS.DAL.DTO;

namespace EMS.BAL.Interfaces;

public interface IEmployeeBAL
{
    public List<EmployeeDetails>? GetAll();
    public int AddEmployee(EmployeeDetails employee);
    public int DeleteEmployee(int id);
    public int UpdateEmployee(int id, EmployeeDetails employee);
    public List<EmployeeDetails> GetEmployeeById(int id);
    public List<EmployeeDetails>? SearchEmployees(string keyword);
    public List<EmployeeDetails>? FilterEmployees(EmployeeFilters filters);
    public int CountEmployees();
}
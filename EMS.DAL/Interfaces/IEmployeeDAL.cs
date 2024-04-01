using EMS.DAL.DBO;
using EMS.DAL.DTO;

namespace EMS.DAL.Interfaces;

public interface IEmployeeDAL
{
    public bool Insert(EmployeeDetails employee);
    public List<EmployeeDetails>? RetrieveAll();
    public bool Update(int id, EmployeeDetails updatedEmployee);
    public List<EmployeeDetails> GetEmployeeById(int id);
    public bool Delete(int id);
    public List<EmployeeDetails>? Filter(EmployeeFilters? filters);
    public int Count();
}


using EMS.DAL.DBO;
using EMS.DAL.DTO;

namespace EMS.DAL.Interfaces;

public interface IEmployeeDAL
{
    public int Insert(EmployeeDetails employee);
    public List<EmployeeDetails>? RetrieveAll();
    public int Update(int id, EmployeeDetails updatedEmployee);
    public int Delete(int id);
    public List<EmployeeDetails>? Filter(EmployeeFilters? filters);
    public int Count();
}


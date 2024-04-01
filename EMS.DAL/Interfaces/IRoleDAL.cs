using EMS.DAL.DBO;

namespace EMS.DAL.Interfaces;

public interface IRoleDAL
{
    public bool Insert(Role role);
    public List<Role>? RetrieveAll();
}
using EMS.DAL.DBO;

namespace EMS.DAL.Interfaces;

public interface IRoleDAL
{
    public int Insert(Role role);
    public List<Role>? RetrieveAll();
}
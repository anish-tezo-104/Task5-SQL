using EMS.DAL.DBO;

namespace EMS.BAL.Interfaces;

public interface IRoleBAL
{
    public bool AddRole(Role role);
    public List<Role>? GetAll();
}
using EMS.DAL.DBO;

namespace EMS.BAL.Interfaces;

public interface IRoleBAL
{
    public int AddRole(Role role);
    public List<Role>? GetAll();
}
using EMS.Common.Logging;
using EMS.DAL.Interfaces;
using EMS.DAL.DBO;
using System.Data.SqlClient;

namespace EMS.DAL;

public class RoleDAL : IRoleDAL
{
    private readonly string? _connectionString = "";
    private readonly ILogger _logger;

    public RoleDAL(ILogger logger, string connectionString)
    {
        _logger = logger;
        _connectionString = connectionString;
    }

    public int Insert(Role role)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        try
        {
            connection.Open();

            string query = @"INSERT INTO Role (Name, DepartmentId)
                         VALUES (@Name, @DepartmentId);
                         SELECT SCOPE_IDENTITY();";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Name", role.Name);
            command.Parameters.AddWithValue("@DepartmentId", role.DepartmentId);

            int insertedId = Convert.ToInt32(command.ExecuteScalar());
            return insertedId;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred while adding new role: {ex.Message}");
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public List<Role>? RetrieveAll()
    {
        List<Role> roles = [];

        using SqlConnection connection = new SqlConnection(_connectionString);

        try
        {
            connection.Open();
            string query = "SELECT * FROM Role";
            using SqlCommand command = new(query, connection);
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Role role = new Role
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    DepartmentId = reader.GetInt32(2)
                };
                roles.Add(role);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred while retrieving all roles: {ex.Message}");
            throw;
        }
        finally
        {
            connection.Close();
        }
        return roles;
    }

}

using Microsoft.Extensions.Configuration;
using EMS.Common.Logging;
using EMS.DAL.Interfaces;
using EMS.DAL.DBO;
using System.Data.SqlClient;

namespace EMS.DAL;

public class RoleDAL : IRoleDAL
{
    private readonly string? _connectionString = "";
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public RoleDAL(ILogger logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _connectionString = _configuration["ConnectionString"];
    }

    public bool Insert(Role role)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();

        string query = @"INSERT INTO Role (Name, DepartmentId)
                         VALUES (@Name, @DepartmentId)";

        using SqlCommand command = new SqlCommand(query, connection);


        command.Parameters.AddWithValue("@Name", role.Name);
        command.Parameters.AddWithValue("@DepartmentId", role.DepartmentId);

        int rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected > 0)
        {
            return true;
        }
        else
        {
            _logger.LogWarning("No rows were affected during insert operation.");
            return false;
        }
    }

    public List<Role>? RetrieveAll()
    {
        using SqlConnection connection = new(_connectionString);
        connection.Open();
        string query = "SELECT * FROM Role";
        using SqlCommand command = new(query, connection);
        using SqlDataReader reader = command.ExecuteReader();
        List<Role> roles = [];
        while (reader.Read())
        {
            Role role = new()
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                DepartmentId = reader.GetInt32(2)
            };
            roles.Add(role);
        }
        if (roles == null || roles.Count == 0)
        {
            return [];
        }
        connection.Close();
        return roles;
    }
}

using System.Data.SqlClient;
using System.Text.Json;
using Dapper;
using EMS.DAL.DBO;
using EMS.DAL.DTO;
using EMS.DAL.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EMS.DAL;

public class DropdownDAL : IDropdownDAL
{

    public readonly IConfiguration _configuration;
    private readonly string? _connectionString = "";

    public DropdownDAL(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration["ConnectionString"];
    }

    public List<Dropdown>? GetLocationsList()
    {
        return LoadDataToList<Dropdown>("SELECT ID, Name FROM Location");
    }

    public List<Dropdown>? GetDepartmentsList()
    {
        return LoadDataToList<Dropdown>("SELECT * FROM Department");
    }

    public List<Dropdown>? GetManagersList()
    {
        return LoadDataToList<Dropdown>("SELECT ID, CONCAT(FirstName, ' ', LastName) AS Name FROM Employee WHERE IsManager = 1");
    }

    public List<Dropdown>? GetProjectsList()
    {
        return LoadDataToList<Dropdown>("SELECT * FROM Project");
    }

    public List<Dropdown>? GetStatusList()
    {
        return LoadDataToList<Dropdown>("SELECT * FROM Status");
    }

    public List<Role>? GetRolesList()
    {
        return LoadDataToList<Role>("SELECT * FROM Role");
    }

    public Dictionary<int, string>? GetRoleNamesByDepartmentId(int? departmentId)
    {
        Dictionary<int, string> roleNames = new Dictionary<int, string>();

        string query = @"
    SELECT r.Id AS Id, r.Name AS RoleName
    FROM Role AS r
    INNER JOIN Department AS d ON r.DepartmentId = d.Id
    WHERE d.Id = @DepartmentId";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DepartmentId", departmentId);

            connection.Open();
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int roleId = reader.GetInt32(reader.GetOrdinal("Id"));
                string roleName = reader.GetString(reader.GetOrdinal("RoleName"));
                roleNames.Add(roleId, roleName);
            }

            reader.Close();
        }

        return roleNames;
    }

    private List<T>? LoadDataToList<T>(string query)
    {
        using var connection = new SqlConnection(_connectionString);
        try
        {
            connection.Open();
            return connection.Query<T>(query).AsList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error executing SQL query: {ex.Message}", ex);
        }
    }
}

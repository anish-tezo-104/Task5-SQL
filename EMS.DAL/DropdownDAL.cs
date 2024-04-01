using System.Data.SqlClient;
using EMS.DAL.DBO;
using EMS.DAL.DTO;
using EMS.DAL.Interfaces;
namespace EMS.DAL;

public class DropdownDAL : BaseDAL, IDropdownDAL
{
    private readonly string? _connectionString = "";

    public DropdownDAL(string connectionString) : base()
    {
        _connectionString = connectionString;
    }

    public List<Dropdown>? GetLocationsList()
    {
        string query = "SELECT * FROM Location";
        using SqlDataReader reader = ExecuteQuery(query);
        if (reader != null && reader.HasRows)
        {
            return LoadDataToList<Dropdown>(reader);
        }
        return [];
    }

    public List<Dropdown>? GetDepartmentsList()
    {
        string query = "SELECT * FROM Department";
        using SqlDataReader reader = ExecuteQuery(query);
        if (reader != null && reader.HasRows)
        {
            return LoadDataToList<Dropdown>(reader);
        }
        return [];
    }

    public List<Dropdown>? GetManagersList()
    {
        string query = "SELECT Id, CONCAT(FirstName, ' ', LastName) AS Name FROM Employee WHERE IsManager = 1";
        using SqlDataReader reader = ExecuteQuery(query);
        if (reader != null && reader.HasRows)
        {
            return LoadDataToList<Dropdown>(reader);
        }
        return [];
    }

    public List<Dropdown>? GetProjectsList()
    {
        string query = "SELECT * FROM Project";
        using SqlDataReader reader = ExecuteQuery(query);
        if (reader != null && reader.HasRows)
        {
            return LoadDataToList<Dropdown>(reader);
        }
        return [];
    }

    public List<Role>? GetRolesList()
    {
        string query = "SELECT * FROM Role";
        using SqlDataReader reader = ExecuteQuery(query);
        if (reader != null && reader.HasRows)
        {
            return LoadDataToList<Role>(reader);
        }
        return [];
    }

    public Dictionary<int, string>? GetRoleNamesByDepartmentId(int? departmentId)
    {
        Dictionary<int, string> roleNames = [];

        string query = @"
    SELECT r.Id AS Id, r.Name AS RoleName
    FROM Role AS r
    INNER JOIN Department AS d ON r.DepartmentId = d.Id
    WHERE d.Id = @DepartmentId";

        using (SqlConnection connection = new(_connectionString))
        {
            SqlCommand command = new(query, connection);
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

    public SqlDataReader ExecuteQuery(string query)
    {
        SqlConnection connection = new(_connectionString);
        SqlCommand command = new(query, connection);
        try
        {
            connection.Open();
            SqlDataReader? reader = command.ExecuteReader();
            return reader;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error executing SQL query: {ex.Message}", ex);
        }
    }
}

using System.Data;
using System.Data.SqlClient;
using EMS.Common.Logging;
using EMS.DAL.DBO;
using EMS.DAL.DTO;
using EMS.DAL.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EMS.DAL;

public class EmployeeDAL : IEmployeeDAL
{
    private readonly string? _connectionString = "";
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public EmployeeDAL(ILogger logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _connectionString = _configuration["ConnectionString"];
    }

    public bool Insert(EmployeeDetails employee)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();

        string query = @"INSERT INTO Employee (UID, Status, FirstName, LastName, Dob, Email, MobileNumber, JoiningDate, LocationId, RoleId, DepartmentId, ProjectId, ManagerId)
                         VALUES (@UID, @Status, @FirstName, @LastName, @Dob, @Email, @MobileNumber, @JoiningDate, @LocationId, @RoleId, @DepartmentId, @ProjectId, @ManagerId)";

        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@UID", employee.UID);
        command.Parameters.AddWithValue("@Status", employee.Status);
        command.Parameters.AddWithValue("@FirstName", employee.FirstName);
        command.Parameters.AddWithValue("@LastName", employee.LastName);
        command.Parameters.AddWithValue("@Dob", employee.Dob);
        command.Parameters.AddWithValue("@Email", employee.Email);
        command.Parameters.AddWithValue("@MobileNumber", employee.MobileNumber);
        command.Parameters.AddWithValue("@JoiningDate", employee.JoiningDate);
        command.Parameters.AddWithValue("@LocationId", employee.LocationId);
        command.Parameters.AddWithValue("@RoleId", employee.RoleId);
        command.Parameters.AddWithValue("@DepartmentId", employee.DepartmentId);
        command.Parameters.AddWithValue("@ProjectId", employee.ProjectId);
        command.Parameters.AddWithValue("@ManagerId", employee.ManagerId);
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

    public List<EmployeeDetails> RetrieveAll()
    {
        using SqlConnection connection = new(_connectionString);
        connection.Open();
        string query = "SELECT * FROM Employee";
        using SqlCommand command = new(query, connection);
        using SqlDataReader reader = command.ExecuteReader();
        List<EmployeeDetails> employees = ConvertDataReaderToEmployeeDetailsList(reader);
        connection.Close();
        return employees;
    }

    public bool Update(int id, EmployeeDetails employee)
    {
        string query = @"
        UPDATE Employee 
        SET 
            FirstName = ISNULL(@FirstName, FirstName), 
            LastName = ISNULL(@LastName, LastName), 
            Dob = ISNULL(@Dob, Dob), 
            Email = ISNULL(@Email, Email), 
            MobileNumber = ISNULL(@MobileNumber, MobileNumber), 
            JoiningDate = ISNULL(@JoiningDate, JoiningDate), 
            LocationId = ISNULL(@LocationId, LocationId), 
            RoleId = ISNULL(@RoleId, RoleId), 
            DepartmentId = ISNULL(@DepartmentId, DepartmentId), 
            ManagerId = ISNULL(@ManagerId, ManagerId), 
            ProjectId = ISNULL(@ProjectId, ProjectId) 
        WHERE 
            ID = @ID";

        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();
        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ID", id);
        if (!string.IsNullOrEmpty(employee.FirstName) && !string.IsNullOrWhiteSpace(employee.FirstName))
        {
            command.Parameters.AddWithValue("@FirstName", employee.FirstName);
        }
        else
        {
            command.Parameters.AddWithValue("@FirstName", DBNull.Value);
        }
        command.Parameters.AddWithValue("@LastName", !string.IsNullOrEmpty(employee.LastName) ? employee.LastName : DBNull.Value);
        command.Parameters.AddWithValue("@Dob", employee.Dob != null ? employee.Dob : DBNull.Value);
        command.Parameters.AddWithValue("@Email", !string.IsNullOrEmpty(employee.Email) ? employee.Email : DBNull.Value);
        command.Parameters.AddWithValue("@MobileNumber", !string.IsNullOrEmpty(employee.MobileNumber) ? employee.MobileNumber : DBNull.Value);
        command.Parameters.AddWithValue("@JoiningDate", employee.JoiningDate != null ? employee.JoiningDate : DBNull.Value);
        command.Parameters.AddWithValue("@LocationId", employee.LocationId != null ? employee.LocationId : DBNull.Value);
        command.Parameters.AddWithValue("@RoleId", employee.RoleId != null ? employee.RoleId : DBNull.Value);
        command.Parameters.AddWithValue("@DepartmentId", employee.DepartmentId != null ? employee.DepartmentId : DBNull.Value);
        command.Parameters.AddWithValue("@ManagerId", employee.ManagerId != null ? employee.ManagerId : DBNull.Value);
        command.Parameters.AddWithValue("@ProjectId", employee.ProjectId != null ? employee.ProjectId : DBNull.Value);

        try
        {
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error executing SQL query: {ex.Message}");
            return false;
        }
    }

    public bool Delete(int id)
    {
        string query = "DELETE FROM Employee WHERE ID = @ID";

        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@ID", id);

        connection.Open();
        int rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected == 0)
        {
            return false;
        }
        return true;
    }

    public List<EmployeeDetails>? Filter(EmployeeFilters? filters)
    {
        List<EmployeeDetails> employees = RetrieveAll() ?? [];
        if (filters != null)
        {
            // Apply filters
            employees = ApplyFilter(employees, filters);

            // Apply search
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                employees = ApplySearch(employees, filters.Search);
            }
        }

        return employees;
    }

    public int Count()
    {
        string query = "SELECT COUNT(*) AS Count FROM Employee;";
        int count = 0;
        using SqlConnection connection = new(_connectionString);
        connection.Open();
        using SqlCommand command = new(query, connection);
        count = (int)command.ExecuteScalar();
        return count;
    }

    public List<EmployeeDetails> GetEmployeeById(int id)
    {
        using SqlConnection connection = new(_connectionString);
        connection.Open();
        string query = @"SELECT * FROM Employee WHERE ID = @Id";

        using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        using SqlDataReader reader = command.ExecuteReader();
        List<EmployeeDetails> employees = ConvertDataReaderToEmployeeDetailsList(reader);
        connection.Close();
        return employees;
    }

    private static List<EmployeeDetails> ApplyFilter(List<EmployeeDetails> employees, EmployeeFilters filters)
    {
        List<Func<Employee, bool>> filterConditions = [];

        if (filters.Alphabet != null && filters.Alphabet.Count != 0)
        {
            filterConditions.Add(e => e.FirstName != null && filters.Alphabet.Contains(char.ToLower(e.FirstName[0])));
        }

        if (filters.Locations != null && filters.Locations.Count != 0)
        {
            filterConditions.Add(e => e.LocationId.HasValue && filters.Locations.Contains(e.LocationId.Value));
        }

        if (filters.Departments != null && filters.Departments.Count != 0)
        {
            filterConditions.Add(e => e.DepartmentId.HasValue && filters.Departments.Contains(e.DepartmentId.Value));
        }

        if (filters.Status != null && filters.Status.Count != 0)
        {
            filterConditions.Add(e => filters.Status.Contains(e.Status));
        }

        return employees.Where(e => filterConditions.All(condition => condition(e))).ToList();
    }

    private static List<EmployeeDetails> ApplySearch(List<EmployeeDetails> employees, string searchKeyword)
    {
        if (string.IsNullOrWhiteSpace(searchKeyword))
        {
            return employees;
        }

        string keyword = searchKeyword.ToLower();
        return employees.Where(e =>
        e.UID?.Equals(keyword, StringComparison.CurrentCultureIgnoreCase) == true ||
        e.FirstName?.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) == true ||
        e.LastName?.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) == true
        ).ToList();

    }

    private string? GetDropdownName(int? Id, string tableName)
    {
        string query;
        object? result = null;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            if (tableName == "Manager")
            {
                query = @"
        SELECT CONCAT(m.FirstName, ' ', m.LastName) AS Name 
        FROM Employee e
        LEFT JOIN Employee m ON e.ManagerId = m.ID
        WHERE e.ID = @ID";
            }
            else
            {
                query = $"SELECT Name FROM {tableName} WHERE ID = @ID";
            }

            using SqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@ID", Id.HasValue ? Id.Value : DBNull.Value);

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                result = reader["Name"];
            }
        }

        return result as string;
    }

    private List<EmployeeDetails> ConvertDataReaderToEmployeeDetailsList(SqlDataReader reader)
    {
        List<EmployeeDetails> employees = [];
        while (reader.Read())
        {
            EmployeeDetails employee = new()
            {
                Id = (int)reader["ID"],
                UID = reader["UID"].ToString(),
                Status = Convert.ToInt32(reader["Status"]),
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                Dob = reader["Dob"] as DateTime?,
                Email = reader["Email"].ToString(),
                MobileNumber = reader["MobileNumber"].ToString(),
                JoiningDate = reader["JoiningDate"] as DateTime?,
                LocationId = reader["LocationId"] as int?,
                RoleId = reader["RoleId"] as int?,
                DepartmentId = reader["DepartmentId"] as int?,
                ManagerId = reader["ManagerId"] as int?,
                ProjectId = reader["ProjectId"] as int?
            };

            employee.LocationName = GetDropdownName(employee.LocationId, "Location");
            employee.RoleName = GetDropdownName(employee.RoleId, "Role");
            employee.DepartmentName = GetDropdownName(employee.DepartmentId, "Department");
            employee.ManagerName = GetDropdownName(employee.Id, "Manager");
            employee.ProjectName = GetDropdownName(employee.ProjectId, "Project");

            employees.Add(employee);
        }
        return employees;
    }

}
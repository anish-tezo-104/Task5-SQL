using System.Data;
using System.Data.SqlClient;
using System.Text;
using EMS.Common.Logging;
using EMS.DAL.DBO;
using EMS.DAL.DTO;
using EMS.DAL.Interfaces;

namespace EMS.DAL;

public class EmployeeDAL : BaseDAL, IEmployeeDAL
{
    private readonly string? _connectionString = "";
    private readonly ILogger _logger;

    public EmployeeDAL(ILogger logger, string connectionString) : base()
    {
        _logger = logger;
        _connectionString = connectionString;
    }

    public int Insert(EmployeeDetails employee)
    {
        using SqlConnection connection = new(_connectionString);
        try
        {
            connection.Open();
            string query = @"INSERT INTO Employee (UID, FirstName, LastName, Dob, Email, MobileNumber, JoiningDate, LocationId, RoleId, DepartmentId, ProjectId, ManagerId)
                             VALUES (@UID, @FirstName, @LastName, @Dob, @Email, @MobileNumber, @JoiningDate, @LocationId, @RoleId, @DepartmentId, @ProjectId, @ManagerId);
                             SELECT SCOPE_IDENTITY();";

            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@UID", employee.UID);
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
            int insertedId = Convert.ToInt32(command.ExecuteScalar());
            return insertedId;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred while inserting employee: {ex.Message}");
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public List<EmployeeDetails>? RetrieveAll()
    {
        List<EmployeeDetails>? employees = [];
        using (SqlConnection connection = new(_connectionString))
        {
            try
            {
                connection.Open();
                string query = @"
                SELECT 
                    Employee.*, 
                    Location.Name as LocationName, 
                    Role.Name as RoleName, 
                    Department.Name as DepartmentName, 
                    Project.Name as ProjectName, 
                    Manager.Name AS ManagerName
                FROM 
                    Employee
                LEFT JOIN 
                    Location ON Employee.LocationId = Location.Id
                LEFT JOIN 
                    Role ON Employee.RoleId = Role.Id
                LEFT JOIN 
                    Department ON Employee.DepartmentId = Department.Id
                LEFT JOIN 
                    Project ON Employee.ProjectId = Project.Id
                LEFT JOIN 
                    (SELECT ID, CONCAT(FirstName, ' ', LastName) AS Name FROM Employee WHERE IsManager = 1) AS Manager 
                    ON Employee.ManagerId = Manager.ID";

                using SqlCommand command = new(query, connection);
                using SqlDataReader reader = command.ExecuteReader();
                employees = LoadDataToList<EmployeeDetails>(reader);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving employee details: {ex.Message}");
                throw;
            }
            finally
            {
                connection.Close();
            }
        }
        return employees ?? null;
    }

    public int Update(int id, EmployeeDetails employee)
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

        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(query, connection);
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
            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error executing SQL query: {ex.Message}");
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public int Delete(int id)
    {
        string query = "DELETE FROM Employee WHERE ID = @ID";

        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@ID", id);

        try
        {
            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting employee: {ex.Message}");
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public List<EmployeeDetails>? Filter(EmployeeFilters? filters)
    {
        if (filters == null)
        {
            return null;
        }

        string query = BuildFilterQuery(filters);

        using SqlConnection connection = new(_connectionString);
        SqlCommand command = new(query, connection);

        try
        {
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            return LoadDataToList<EmployeeDetails>(reader);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error Filtering: {ex.Message}");
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    private static string BuildFilterQuery(EmployeeFilters filters)
    {
        string query = @"
            SELECT 
                Employee.*, 
                Location.Name as LocationName, 
                Role.Name as RoleName, 
                Department.Name as DepartmentName, 
                Project.Name as ProjectName, 
                Manager.Name AS ManagerName
            FROM 
                Employee
            LEFT JOIN 
                Location ON Employee.LocationId = Location.Id
            LEFT JOIN 
                Role ON Employee.RoleId = Role.Id
            LEFT JOIN 
                Department ON Employee.DepartmentId = Department.Id
            LEFT JOIN 
                Project ON Employee.ProjectId = Project.Id
            LEFT JOIN 
                (SELECT ID, CONCAT(FirstName, ' ', LastName) AS Name FROM Employee WHERE IsManager = 1) AS Manager 
                ON Employee.ManagerId = Manager.ID
            WHERE 1 = 1";

        // Apply filter conditions
        if (filters.Alphabet != null && filters.Alphabet.Count > 0)
        {
            string alphabetCondition = string.Join(" OR ", filters.Alphabet.Select(c => $"Employee.FirstName LIKE '{c}%'"));
            query += $" AND ({alphabetCondition})";
        }

        if (filters.Locations != null && filters.Locations.Count > 0)
        {
            string locationCondition = string.Join(", ", filters.Locations);
            query += $" AND Employee.LocationId IN ({locationCondition})";
        }

        if (filters.Departments != null && filters.Departments.Count > 0)
        {
            string departmentCondition = string.Join(", ", filters.Departments);
            query += $" AND Employee.DepartmentId IN ({departmentCondition})";
        }

        if (filters.Status != null && filters.Status.Count > 0)
        {
            string statusCondition = string.Join(", ", filters.Status);
            query += $" AND Employee.Status IN ({statusCondition})";
        }

        // Apply search condition
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            string searchCondition = $"(Employee.FirstName LIKE '%{filters.Search}%' OR Employee.LastName LIKE '%{filters.Search}%' OR Employee.UID LIKE '%{filters.Search}%' OR Employee.Id LIKE '%{filters.Search}%')";
            query += $" AND {searchCondition}";
        }

        return query;
    }

    public int Count()
    {
        string query = "SELECT COUNT(*) AS Count FROM Employee;";
        int count = 0;
        using SqlConnection connection = new SqlConnection(_connectionString);
        try
        {
            connection.Open();

            using SqlCommand command = new SqlCommand(query, connection);
            count = (int)command.ExecuteScalar();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error counting employees: {ex.Message}");
            throw;
        }
        finally
        {
            connection.Close();
        }

        return count;
    }
}
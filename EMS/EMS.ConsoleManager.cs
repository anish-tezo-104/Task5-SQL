using EMS.DAL.DBO;
using EMS.DAL.DTO;
using EMS.Models;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EMS;

public partial class EMS
{
    //Employees related Partial Functions
    private static partial EmployeeDetails GetEmployeeDataFromConsole()
    {
        PrintConsoleMessage("Enter employee details:\n");
        bool required = true;
        EmployeeDetails employee = new();
        string? empNo = ValidateEmployeeNo("Employee Number", required);
        employee.UID = string.IsNullOrWhiteSpace(empNo) ? null : empNo;
        string? firstName = GetDataFromField("First Name", required);
        employee.FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName;
        string? lastName = GetDataFromField("Last Name", required);
        employee.LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName;
        string? dob = GetDataFromField("Date of Birth (YYYY-MM-DD)");
        DateTime.TryParse(dob, out DateTime dobValue);
        employee.Dob = dobValue;
        string? email = GetDataFromField("Email", required);
        employee.Email = string.IsNullOrWhiteSpace(email) ? null : email;
        string? mobileNumber = GetDataFromField("Mobile Number");
        employee.MobileNumber = string.IsNullOrWhiteSpace(mobileNumber) ? null : mobileNumber;
        string? joiningDate = GetDataFromField("Joining Date (YYYY-MM-DD)", required);
        DateTime.TryParse(joiningDate, out DateTime joiningDateValue);
        employee.JoiningDate = joiningDateValue;
        employee.LocationId = GetIdFromUser("Location", required);
        employee.DepartmentId = GetIdFromUser("Department", required);
        employee.RoleId = GetRoleIdFromUserForDepartment(employee.DepartmentId, required);
        employee.ManagerId = GetIdFromUser("Manager");
        employee.ProjectId = GetIdFromUser("Project");
        return employee;
    }

    private static string? ValidateEmployeeNo(string fieldName, bool isRequired = false)
    {
        PrintConsoleMessage($"{fieldName}: ", ConsoleColor.White, false);
        string? empNo = Console.ReadLine();
        if (empNo != null && IsEmpNoDuplicate(empNo))
        {
            PrintWarning($"Employee already exists.");
            return ValidateEmployeeNo(fieldName, isRequired);
        }

        if (!string.IsNullOrEmpty(empNo) && !Regex.IsMatch(empNo, @"^TZ\d{4}$"))
        {
            PrintWarning("Employee No must be in the format 'TZ' followed by a four-digit number (e.g., TZ1001).\n");
            return ValidateEmployeeNo(fieldName, isRequired);
        }

        if (isRequired && (string.IsNullOrEmpty(empNo) || string.IsNullOrWhiteSpace(empNo)))
        {
            PrintWarning("Field is required. Please enter a value.\n");
            return ValidateEmployeeNo(fieldName, isRequired);
        }

        return empNo;
    }

    private static bool IsEmpNoDuplicate(string UID)
    {
        List<EmployeeDetails> employees = _employeeBal?.SearchEmployees(UID) ?? [];
        return employees.Count > 0;
    }

    private static int? GetRoleIdFromUserForDepartment(int? departmentId, bool isRequired = false)
    {
        if(departmentId == null)
        {
            return null;
        }
        Dictionary<int, string>? validRoles = _dropdownBal.GetRoleNamesByDepartmentId(departmentId);

        if (validRoles == null || validRoles.Count == 0)
        {
            PrintWarning($"No roles found for the specified department.");
            return null;
        }

        PrintConsoleMessage($"\nValid roles :\n", ConsoleColor.Cyan);
        foreach (var role in validRoles)
        {
            PrintConsoleMessage($"- {role.Value} (ID: {role.Key})", ConsoleColor.Cyan);
        }
        PrintConsoleMessage("\n");

        int? roleId;
        do
        {
            string? userInput = GetDataFromField("Job Title", isRequired);
            if (string.IsNullOrWhiteSpace(userInput)) return null;

            if (!int.TryParse(userInput, out int parsedRoleId) || !validRoles.ContainsKey(parsedRoleId))
            {
                PrintError($"Invalid role. Please select Id from the roles listed for the specified department.");
                roleId = null;
            }
            else
            {
                roleId = parsedRoleId;
            }
        } while (roleId == null);

        return roleId;
    }

    private static int? GetIdFromUser(string fieldName, bool isRequired = false)
    {
        Dictionary<int, string>? dictionary = GetDropdownDictionary(fieldName);
        if (dictionary == null)
        {
            PrintError("Could not find the dropdown list.");
            return null;
        }
        string? userInput;
        int? id;
        do
        {
            userInput = GetDataFromField(fieldName, isRequired);
            if (string.IsNullOrWhiteSpace(userInput)) return null;

            id = FindIdInDictionary(userInput, dictionary);
            if (id == null)
            {
                PrintError($"Invalid {fieldName}. Please try again.");
            }
        } while (id == null);

        return id;
    }

    private static Dictionary<int, string>? GetDropdownDictionary(string fieldName)
    {
        Dictionary<int, string> dictionary;
        if (fieldName == "Location")
        {
            dictionary = _dropdownBal.GetLocations();
        }
        else if (fieldName == "Department")
        {
            dictionary = _dropdownBal.GetDepartments();
        }
        else if (fieldName == "Manager")
        {
            dictionary = _dropdownBal.GetManagers();
        }
        else if (fieldName == "Project")
        {
            dictionary = _dropdownBal.GetProjects();
        }
        else if (fieldName == "Role")
        {
            dictionary = _dropdownBal.GetRoles();
        }
        else
        {
            return null;
        }
        return dictionary;
    }

    private static int? FindIdInDictionary(string userInput, Dictionary<int, string> dictionary)
    {
        foreach (var kvp in dictionary)
        {
            if (kvp.Value.Equals(userInput, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Key;
            }
        }
        return null;
    }

    private static partial EmployeeFilters? GetEmployeeFiltersFromConsole()
    {
        DisplayFilters("Location");
        DisplayFilters("Department");
        DisplayFilters("Status");

        EmployeeFilters filters = new();
        PrintConsoleMessage("\nEnter the filter criteria:\n\n");

        string? alphabetInput = GetDataFromField("Enter alphabet letters (separated by comma if multiple)");
        filters.Alphabet = string.IsNullOrEmpty(alphabetInput) ? null : alphabetInput.Split(',').SelectMany(x => x.Trim().Select(char.ToLower)).ToList();

        ValidateFilters<Dropdown>("Location", out var locationIds);
        filters.Locations = locationIds;

        ValidateFilters<Dropdown>("Department", out var departmentIds);
        filters.Departments = departmentIds;

        ValidateFilters<Dropdown>("Status", out var statusIds);
        filters.Status = statusIds;

        return filters;
    }

    private static void DisplayFilters(string filterName)
    {
        if (filterName == "Status")
        {
            PrintConsoleMessage("\nStatus :\n\n1. Active\n2. Inactive\n", ConsoleColor.Cyan);
            return;
        }
        Dictionary<int, string>? dictionary = GetDropdownDictionary(filterName);
        if (dictionary == null)
        {
            PrintError("Could not find the dropdown list.");
            return;
        }
        PrintConsoleMessage($"\n{filterName} :\n", ConsoleColor.Cyan);
        foreach (var kvp in dictionary)
        {
            PrintConsoleMessage($"- {kvp.Key} : {kvp.Value}", ConsoleColor.Cyan);
        }
    }

    private static void ValidateFilters<T>(string fieldName, out List<int> ids) where T : class
    {
        string? input;
        ids = [];
        if (fieldName == "Status")
        {
            input = GetDataFromField(fieldName);
            if (input != null && input.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                ids = [1];
            }
            else if (input != null && input.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                ids = [0];
            }
            else
            {
                ids = [];
            }
            return;
        }

        Dictionary<int, string>? dictionary = GetDropdownDictionary(fieldName);
        if (dictionary == null)
        {
            PrintError("Could not find the dropdown list.");
            return;
        }
        do
        {
            input = GetDataFromField(fieldName);

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            var filterValues = input.Split(',').Select(value => value.Trim());
            foreach (var filterValue in filterValues)
            {
                var id = FindIdInDictionary(filterValue, dictionary);
                if (id.HasValue)
                {
                    ids.Add(id.Value);
                }
                else
                {
                    PrintError($"Invalid value '{filterValue}'. Please enter a valid {fieldName}.");
                }
            }
        } while (true);
    }

    private static partial EmployeeDetails GetUpdatedDataFromUser()
    {
        EmployeeDetails employee = new()
        {
            FirstName = GetDataFromField("First Name")!,
            LastName = GetDataFromField("Last Name")!,
            Dob = ParseNullableDate(GetDataFromField("Date of Birth (YYYY-MM-DD)")),
            Email = GetDataFromField("Email")!,
            MobileNumber = GetDataFromField("Mobile Number")!,
            JoiningDate = ParseNullableDate(GetDataFromField("Joining Date (YYYY-MM-DD)")),
            LocationId = GetIdFromUser("Location"),
            DepartmentId = GetIdFromUser("Department")
        };
        employee.RoleId = GetRoleIdFromUserForDepartment(employee.DepartmentId);
        employee.ManagerId = GetIdFromUser("Manager");
        employee.ProjectId = GetIdFromUser("Project");

        return employee;
    }

    private static partial IConfiguration GetIConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        return configuration;
    }

    private static DateTime? ParseNullableDate(string? dateValue)
    {
        if (string.IsNullOrWhiteSpace(dateValue))
        {
            return null;
        }
        else
        {
            if (DateTime.TryParseExact(dateValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate;
            }
            else
            {
                throw new FormatException("Invalid date format. Please use the format YYYY-MM-DD or enter '--d' to indicate no date.");
            }
        }
    }

    private static string? GetDataFromField(string message, bool isRequired = false)
    {
        PrintConsoleMessage($"{message}: ", ConsoleColor.White, false);
        string? fieldInput = Console.ReadLine();
        if (isRequired && (string.IsNullOrEmpty(fieldInput) || string.IsNullOrWhiteSpace(fieldInput)))
        {
            PrintWarning("Field is required. Please enter a value.\n");
            return GetDataFromField(message, isRequired);
        }
        return fieldInput;
    }

    private static void ResetFilters(EmployeeFilters Filters)
    {
        if (Filters != null)
        {
            Filters.Alphabet = [];
            Filters.Locations = [];
            Filters.Departments = [];
            Filters.Status = [];
            Filters.Search = "";
        }
    }

    private static partial void PrintEmployeesDetails(List<EmployeeDetails> employees)
    {
        PrintEmployeesTableHeader();
        foreach (EmployeeDetails employee in employees)
        {
            string? Status = employee.Status == 1 ? "Active" : "Inactive";
            string? fullName = $"{employee.FirstName ?? null} {employee.LastName ?? null}";
            string? dob = employee.Dob?.ToString("dd-MM-yyyy") ?? null;
            string? email = employee.Email ?? null;
            string? mobileNumber = employee.MobileNumber ?? null;
            string? locationName = employee.LocationName ?? null;
            string? jobTitle = employee.RoleName ?? null;
            string? departmentName = employee.DepartmentName ?? null;
            string? ManagerName = employee.ManagerName ?? null;
            string? ProjectName = employee.ProjectName ?? null;

            PrintConsoleMessage($" {employee.Id}\t|{employee.UID}\t\t|{fullName,-20}\t|{Status,-10}\t|{dob}\t|{email,-30}\t|{mobileNumber}\t|{locationName,-10}\t\t|{jobTitle,-30}\t|{departmentName}");
        }
        PrintConsoleMessage("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
    }

    private static void PrintEmployeesTableHeader()
    {
        PrintConsoleMessage("\nEmployee Details:\n");
        PrintConsoleMessage("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
        PrintConsoleMessage(" ID\t|UID\t\t|Name\t\t\t|Status\t\t|Date of Birth\t|Email\t\t\t\t|Mobile Number\t|Location\t\t|Job Title\t\t\t|Department");
        PrintConsoleMessage("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
    }

    // Roles related partial functions
    private static partial Role GetRoleDataFromConsole(){
        PrintConsoleMessage("Enter Role details:\n");
        bool required = true;

        Role role = new()
        {
            DepartmentId = GetIdFromUser("Department", required),
            Name = GetDataFromField("Role Name", required)!
        };

        return role;
    }

    private static void PrintRolesTableHeader()
    {
        PrintConsoleMessage("\nRoles Details:\n");
        PrintConsoleMessage("------------------------------------------------------------------------------------");
        PrintConsoleMessage(" Role ID\t|Role Name\t\t\t|Department Name");
        PrintConsoleMessage("------------------------------------------------------------------------------------");
    }

    private static partial void PrintRoles(List<Role> roles)
    {
        Dictionary<int, string>? departmentNames = _dropdownBal.GetDepartments();

        if (departmentNames != null)
        {
            PrintRolesTableHeader();
            foreach (Role role in roles)
            {
                int id = role.Id;
                string roleName = role.Name;
                int departmentId = role.DepartmentId ?? 0;
                string? departmentName = departmentNames.TryGetValue(departmentId, out string? value) ? value : null;
                PrintConsoleMessage($" {id}\t\t|{roleName,-30}\t|{departmentName}");
            }
            PrintConsoleMessage("--------------------------------------------------------------------------------------\n");
        }
        else
        {
            PrintError(Constants.DropdownListError);
        }
    }

    private static void PrintConsoleMessage(string message, ConsoleColor color = ConsoleColor.White, bool newLine = true)
    {
        Console.ForegroundColor = color;
        if (newLine)
        {
            Console.WriteLine(message);
        }
        else
        {
            Console.Write(message);
        }
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void PrintSuccess(string message, bool newLine = true)
    {
        PrintConsoleMessage(message, ConsoleColor.Green, newLine);
    }

    public static void PrintError(string message, bool newLine = true)
    {
        PrintConsoleMessage(message, ConsoleColor.Red, newLine);
    }

    public static void PrintWarning(string message, bool newLine = true)
    {
        PrintConsoleMessage(message, ConsoleColor.Yellow, newLine);
    }
}

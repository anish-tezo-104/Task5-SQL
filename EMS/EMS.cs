
using EMS.BAL.Interfaces;
using EMS.Common.Logging;
using EMS.DAL.Interfaces;
using EMS.DAL.DBO;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Configuration;
using EMS.DAL.DTO;
using EMS.DAL;
using EMS.BAL;
using EMS.Models;

namespace EMS;

public partial class EMS
{
    private readonly static IEmployeeDAL _employeeDal;
    private readonly static IRoleDAL _roleDal;
    private readonly static IDropdownDAL _dropdownDal;
    private readonly static IEmployeeBAL _employeeBal;
    private readonly static IDropdownBAL _dropdownBal;
    private readonly static IRoleBAL _roleBal;
    private static readonly ILogger _logger;
    private static readonly IConfiguration _configuration;

    private static partial EmployeeDetails GetEmployeeDataFromConsole();
    private static partial Role GetRoleDataFromConsole();

    private static partial void PrintEmployeesDetails(List<EmployeeDetails> employees);
    private static partial IConfiguration GetIConfiguration();
    private static partial EmployeeFilters? GetEmployeeFiltersFromConsole();
    private static partial EmployeeDetails GetUpdatedDataFromUser();
    private static partial void PrintRoles(List<Role> roles);
    private static string _connectionString = "";

    static EMS()
    {
        _configuration = GetIConfiguration();
        _logger = new ConsoleLogger();
        _connectionString = _configuration["ConnectionString"] ?? throw new ArgumentNullException("ConnectionString");
        _dropdownDal = new DropdownDAL(_connectionString);
        _dropdownBal = new DropdownBAL(_dropdownDal);
        _employeeDal = new EmployeeDAL(_logger, _connectionString);
        _roleDal = new RoleDAL(_logger, _connectionString);
        _employeeBal = new EmployeeBAL(_logger, _employeeDal);
        _roleBal = new RoleBAL(_roleDal);
    }

    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Employee Management System");
        return HandleCommandArgs(args, rootCommand);
    }

    private static int HandleCommandArgs(string[] args, RootCommand rootCommand)
    {

        //Employees Command
        var addEmployeesCommand = new Command("--add-emp", "Add a new employee")
        {
            Handler = CommandHandler.Create(() => AddEmployee())
        };
        var showEmployeesCommand = new Command("--show-emp", "Show employees list")
        {
            Handler = CommandHandler.Create(() => DisplayEmployees())
        };
        var filterEmployeesCommand = new Command("--filter-emp", "Filter employees list")
        {
            Handler = CommandHandler.Create(() => FilterEmployees())
        };
        var deleteEmployeesCommand = new Command("--delete-emp", "Delete an employee]")
        {
            Handler = CommandHandler.Create(() => DeleteEmployee())
        };
        var updateEmployeesCommand = new Command("--update-emp", "Update an employee detail")
        {
            Handler = CommandHandler.Create(() => UpdateEmployee())
        };

        var searchEmployeesCommand = new Command("--search-emp", "Search an employee details [Employee Number, Employee Name]")
        {
            Handler = CommandHandler.Create(() => SearchEmployee())
        };

        var countEmployees = new Command("--count-emp", "Count of employees")
        {
            Handler = CommandHandler.Create(() => CountEmployees())
        };

        // Roles Command

        var addRolesCommand = new Command("--add-role", "Add new Role")
        {
            Handler = CommandHandler.Create(() => AddRoles())
        };

        var showRolesCommand = new Command("--show-role", "Show roles list")
        {
            Handler = CommandHandler.Create(() => DisplayRoles())
        };

        rootCommand.AddOption(new Option<string>("-o", "Display all operations"));
        rootCommand.AddCommand(addEmployeesCommand);
        rootCommand.AddCommand(showEmployeesCommand);
        rootCommand.AddCommand(deleteEmployeesCommand);
        rootCommand.AddCommand(updateEmployeesCommand);
        rootCommand.AddCommand(searchEmployeesCommand);
        rootCommand.AddCommand(filterEmployeesCommand);
        rootCommand.AddCommand(countEmployees);

        rootCommand.AddCommand(addRolesCommand);
        rootCommand.AddCommand(showRolesCommand);

        if (args.Length == 1 && (args[0] == "-o" || args[0] == "-options"))
        {
            DisplayAvailableCommands(rootCommand);
            return 0;
        }
        return rootCommand.Invoke(args);
    }

    private static void DisplayAvailableCommands(RootCommand rootCommand)
    {
        PrintConsoleMessage("\nAvailable Commands:");
        foreach (var command in rootCommand.Children)
        {
            if (command.Name != "o")
            {
                PrintConsoleMessage($"{command.Name}: {command.Description}");
            }
        }
    }

    private static void AddEmployee()
    {
        try
        {
            EmployeeDetails employee = GetEmployeeDataFromConsole();
            if (employee == null)
            {
                PrintError(Constants.GettingDataFromConsoleError);
                return;
            }
            int id = _employeeBal.AddEmployee(employee);
            if (id > 0)
            {
                PrintSuccess(Constants.AddEmployeeSuccess + $"\nID: {id}");
            }
            else
            {
                PrintError(Constants.ErrorMessage);
            }
        }
        catch (Exception)
        {
            _logger.LogError($"{Constants.ErrorMessage} ");
        }
    }

    private static void DisplayEmployees()
    {
        try
        {
            List<EmployeeDetails>? employees = _employeeBal.GetAll();
            if (employees == null)
            {
                PrintError(Constants.ErrorMessage);
                return;
            }

            PrintSuccess($"{employees.Count} {Constants.RetrieveAllEmployeesSuccess}");
            if (employees.Count > 0)
            {
                PrintEmployeesDetails(employees);
            }
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ErrorMessage);
        }
    }

    private static void DeleteEmployee()
    {
        PrintConsoleMessage("Enter the ID :  ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            PrintError("Invalid ID");
            return;
        }

        List<EmployeeDetails>? employees;
        try
        {
            employees = _employeeBal.GetEmployeeById(id);
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ErrorMessage);
            return;
        }

        if (employees == null || employees.Count == 0)
        {
            PrintSuccess(Constants.EmpNoNotFound);
            return;
        }

        try
        {
            int rowAffected = _employeeBal.DeleteEmployee(id);
            if (rowAffected >= 0)
            {
                PrintSuccess($"{rowAffected} {Constants.DeleteEmployeeSuccess}");
            }
            else
            {
                PrintError(Constants.ErrorMessage);
            }
        }
        catch (Exception)
        {
            _logger.LogError($"{Constants.ErrorMessage} ");
        }
    }

    private static void UpdateEmployee()
    {
        PrintConsoleMessage("Enter the ID :  ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            PrintError("Invalid ID");
            return;
        }

        List<EmployeeDetails> employees;
        try
        {
            employees = _employeeBal.GetEmployeeById(id);
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ErrorMessage);
            return;
        }

        if (employees == null || employees.Count == 0)
        {
            PrintSuccess(Constants.EmpNoNotFound);
            return;
        }

        PrintEmployeesDetails(employees);
        PrintConsoleMessage("\nPress 'Enter' to keep the original value.");
        EmployeeDetails updatedEmployee = GetUpdatedDataFromUser();
        try
        {
            int rowsAffected = _employeeBal.UpdateEmployee(id, updatedEmployee);
            if (rowsAffected >= 0)
            {
                PrintSuccess($"{rowsAffected} {Constants.UpdateEmployeeSuccess}");
            }
            else
            {
                PrintError(Constants.ErrorMessage);
            }
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ErrorMessage);
        }
    }

    private static void SearchEmployee()
    {
        PrintConsoleMessage("Enter the keyword :  ");
        string? keyword = Console.ReadLine()?.Trim();
        if (keyword == null)
        {
            PrintError(Constants.ErrorMessage);
            return;
        }
        try
        {
            List<EmployeeDetails>? employees = _employeeBal.SearchEmployees(keyword);
            if (employees != null)
            {
                PrintSuccess($"{Constants.SearchEmployeeSuccess} {employees.Count}");
                if (employees.Count > 0)
                {
                    PrintEmployeesDetails(employees);
                }

            }
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ErrorMessage);
        }
    }

    private static void FilterEmployees()
    {
        EmployeeFilters? filters = GetEmployeeFiltersFromConsole();
        if (filters == null)
        {
            PrintError(Constants.GettingDataFromConsoleError);
            return;
        }
        try
        {
            List<EmployeeDetails>? employees = _employeeBal.FilterEmployees(filters);
            if (employees != null)
            {
                PrintSuccess($"{Constants.FilterEmployeesSuccess} {employees.Count}");
                if (employees.Count > 0)
                {
                    PrintEmployeesDetails(employees);
                }

            }
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ErrorMessage);
        }
        ResetFilters(filters);
    }

    private static void CountEmployees()
    {
        try
        {
            int count = _employeeBal.CountEmployees();
            PrintSuccess($"{Constants.CountEmployeesSuccess} {count}");
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ErrorMessage);
        }
    }

    // Roles Command Functions
    private static void AddRoles()
    {
        try
        {
            Role role = GetRoleDataFromConsole();
            if (role == null)
            {
                PrintError(Constants.GettingDataFromConsoleError);
                return;
            }
            int id = _roleBal.AddRole(role);
            if (id > 0)
            {
                PrintSuccess(Constants.AddRoleSuccess + $"\nID: {id}");
            }
            else
            {
                PrintError(Constants.ErrorMessage);
            }
        }
        catch (Exception)
        {
            _logger.LogError(Constants.ErrorMessage);
        }
    }

    private static void DisplayRoles()
    {
        try
        {
            List<Role>? roles = _roleBal.GetAll();
            if (roles != null)
            {
                PrintSuccess($"{roles.Count} {Constants.RetrieveAllRolesSuccess}");
                if (roles.Count > 0)
                {
                    PrintRoles(roles);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"{Constants.ErrorMessage} : {e.Message}");
        }
    }
}

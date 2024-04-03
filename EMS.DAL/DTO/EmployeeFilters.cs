namespace EMS.DAL.DTO;

public class EmployeeFilters
{
    public List<char>? Alphabet { get; set; }
    public List<int>? Locations { get; set; }
    public List<int>? Departments { get; set; }
    public List<int>? Status { get; set; }
    public string? Search { get; set; }
}

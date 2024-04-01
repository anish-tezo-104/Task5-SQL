using System.Text.Json;

namespace EMS.Common.Utils;

public class JSONUtils
{

    public List<T> ReadJSON<T>(string filePath)
    {
        List<T> items;
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                items = JsonSerializer.Deserialize<List<T>>(json) ?? [];
            }
            else
            {
                throw new FileNotFoundException($"Error: File '{filePath}' is not present. No Data Available");
            }
        }
        catch (IOException e)
        {
            throw new IOException($"Error reading file '{filePath}': {e.Message}", e);
        }

        return items ?? [];
    }

    public void WriteJSON<T>(List<T> data, string filePath)
    {
        try
        {
            string jsonData = JsonSerializer.Serialize(data);
            File.WriteAllText(filePath, jsonData);
        }
        catch (IOException e)
        {
            throw new IOException($"Error writing to file '{filePath}': {e.Message}", e);
        }
    }
}

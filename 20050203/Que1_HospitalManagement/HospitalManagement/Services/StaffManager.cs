using HospitalManagement.Models;

namespace HospitalManagement.Services;

public class StaffManager
{
    private readonly List<Staff> staffList = new();

    public int Count => staffList.Count;

    public void Add(Staff newStaff)
    {
        if (newStaff is null)
        {
            return;
        }

        foreach (Staff existingStaff in staffList)
        {
            if (existingStaff.StaffId.Equals(newStaff.StaffId, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Staff ID already exists.");
                return;
            }
        }

        staffList.Add(newStaff);
        Console.WriteLine($"Staff member {newStaff.Name} ({newStaff.StaffId}) added.");
    }

    public IReadOnlyList<Staff> GetAll()
    {
        return staffList;
    }

    public bool SearchStaff(string idToSearch)
    {
        if (string.IsNullOrWhiteSpace(idToSearch))
        {
            return false;
        }

        string idToFind = idToSearch.Trim();

        foreach (Staff existingStaff in staffList)
        {
            if (existingStaff.StaffId.Equals(idToFind, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

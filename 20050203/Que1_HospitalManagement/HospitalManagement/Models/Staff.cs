namespace HospitalManagement.Models;

public abstract class Staff
{
    private string staffName = string.Empty;
    private string staffID = string.Empty;
    private string staffDepartment = string.Empty;

    public string Name
    {
        get => staffName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Name cannot be empty.", nameof(value));
            }

            staffName = value.Trim();
        }
    }

    public string StaffId
    {
        get => staffID;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Staff ID cannot be empty.", nameof(value));
            }

            staffID = value.Trim().ToUpperInvariant();
        }
    }

    public string Department
    {
        get => staffDepartment;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Department cannot be empty.", nameof(value));
            }

            staffDepartment = value.Trim();
        }
    }

    protected Staff(string name, string staffId, string department)
    {
        Name = name;
        StaffId = staffId;
        Department = department;
    }

    public abstract string PerformDuty();

    public virtual string GetDetails()
        => $"ID: {StaffId,-6} | {Name,-20} | {Department,-15} | {GetType().Name}";
}

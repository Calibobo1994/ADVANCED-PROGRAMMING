namespace HospitalManagement.Models;

public class AdministrativeStaff : Staff
{
    public string Role { get; set; } = string.Empty;
    public string DeskExtension { get; set; } = string.Empty;

    public AdministrativeStaff(string name, string staffId, string department,
                               string role, string deskExtension)
        : base(name, staffId, department)
    {
        Role = role;
        DeskExtension = deskExtension;
    }

    public override string PerformDuty()
        => "Handles patient records, appointments and front-desk enquiries.";

    public override string GetDetails()
        => $"{base.GetDetails()} | Role: {Role} | Ext: {DeskExtension}";
}

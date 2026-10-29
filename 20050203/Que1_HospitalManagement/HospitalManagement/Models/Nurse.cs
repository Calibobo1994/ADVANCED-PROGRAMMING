namespace HospitalManagement.Models;

public class Nurse : Staff
{
    public string Ward { get; set; } = string.Empty;
    public string Shift { get; set; } = string.Empty;

    public Nurse(string name, string staffId, string department,
                 string ward, string shift)
        : base(name, staffId, department)
    {
        Ward = ward;
        Shift = shift;
    }

    public override string PerformDuty()
        => "Monitors patients, administers medication and updates care records.";

    public override string GetDetails()
        => $"{base.GetDetails()} | Ward: {Ward} | Shift: {Shift}";
}

namespace HospitalManagement.Models;

public class Doctor : Staff
{
    public string Specialisation { get; set; } = string.Empty;
    public string LicenceNumber { get; set; } = string.Empty;

    public Doctor(string name, string staffId, string department,
                  string specialisation, string licenceNumber)
        : base(name, staffId, department)
    {
        Specialisation = specialisation;
        LicenceNumber = licenceNumber;
    }

    public override string PerformDuty()
        => "Diagnoses patients, orders tests and prescribes treatment.";

    public override string GetDetails()
        => $"{base.GetDetails()} | Specialisation: {Specialisation} | Licence: {LicenceNumber}";
}

using HospitalManagement.Models;
using HospitalManagement.Services;

namespace HospitalManagement.Tests;

public class SearchStaffTests
{
    [Fact]
    public void SearchStaff_ReturnsTrue_WhenStaffIdExists()
    {
        var staffManager = new StaffManager();
        staffManager.Add(new Nurse("Aoife Byrne", "N001", "Cardiology", "Ward 3", "Day"));

        bool found = staffManager.SearchStaff("N001");

        Assert.True(found);
    }

    [Fact]
    public void SearchStaff_ReturnsFalse_WhenStaffIdDoesNotExist()
    {
        var staffManager = new StaffManager();
        staffManager.Add(new Doctor("Alice Doyle", "D001", "Cardiology", "Cardiology", "LIC-1"));

        bool found = staffManager.SearchStaff("D999");

        Assert.False(found);
    }

    [Fact]
    public void SearchStaff_IsCaseInsensitive()
    {
        var staffManager = new StaffManager();
        staffManager.Add(new Nurse("Aoife Byrne", "N001", "Cardiology", "Ward 3", "Day"));

        bool found = staffManager.SearchStaff(" n001 ");

        Assert.True(found);
    }

    [Fact]
    public void Add_RejectsDuplicateStaffId_AndKeepsCountAtOne()
    {
        var staffManager = new StaffManager();
        staffManager.Add(new Doctor("Alice Doyle", "D001", "Cardiology", "Cardiology", "LIC-1"));

        staffManager.Add(new Nurse("Aoife Byrne", " d001 ", "Oncology", "Ward 3", "Day"));

        Assert.Equal(1, staffManager.Count);
    }

    [Fact]
    public void Add_IgnoresNull()
    {
        var staffManager = new StaffManager();

        staffManager.Add(null!);

        Assert.Equal(0, staffManager.Count);
    }

    [Fact]
    public void GetAll_ReturnsRecordsInInsertionOrder()
    {
        var staffManager = new StaffManager();
        staffManager.Add(new Doctor("Alice Doyle", "D001", "Cardiology", "Cardiology", "LIC-1"));
        staffManager.Add(new Nurse("Aoife Byrne", "N001", "Oncology", "Ward 3", "Day"));
        staffManager.Add(new AdministrativeStaff("Brendan Kelly", "A001", "Records", "Clerk", "204"));

        IReadOnlyList<Staff> all = staffManager.GetAll();

        Assert.Collection(all,
            staffMember => Assert.Equal("D001", staffMember.StaffId),
            staffMember => Assert.Equal("N001", staffMember.StaffId),
            staffMember => Assert.Equal("A001", staffMember.StaffId));
    }

    [Fact]
    public void SearchStaff_ReturnsFalse_ForEmptyInput()
    {
        var staffManager = new StaffManager();
        staffManager.Add(new Doctor("Alice Doyle", "D001", "Cardiology", "Cardiology", "LIC-1"));

        Assert.False(staffManager.SearchStaff(""));
        Assert.False(staffManager.SearchStaff("   "));
        Assert.False(staffManager.SearchStaff(null!));
    }

    [Fact]
    public void StaffId_IsStoredInUpperCase_AndTrimmed()
    {
        var doctor = new Doctor(" Alice Doyle ", " d001 ", " Cardiology ", "Cardiology", "LIC-1");

        Assert.Equal("D001", doctor.StaffId);
        Assert.Equal("Alice Doyle", doctor.Name);
        Assert.Equal("Cardiology", doctor.Department);
    }

    [Fact]
    public void StaffSetters_RejectBlankValues()
    {
        var doctor = new Doctor("Alice Doyle", "D001", "Cardiology", "Cardiology", "LIC-1");

        Assert.Throws<ArgumentException>(() => doctor.Name = "   ");
        Assert.Throws<ArgumentException>(() => doctor.StaffId = "");
        Assert.Throws<ArgumentException>(() => doctor.Department = " ");
    }

    [Fact]
    public void PerformDuty_IsDifferentForEachStaffType()
    {
        var doctor = new Doctor("Alice Doyle", "D001", "Cardiology", "Cardiology", "LIC-1");
        var nurse = new Nurse("Aoife Byrne", "N001", "Oncology", "Ward 3", "Day");
        var admin = new AdministrativeStaff("Brendan Kelly", "A001", "Records", "Clerk", "204");

        var dutyList = new[]
        {
            doctor.PerformDuty(),
            nurse.PerformDuty(),
            admin.PerformDuty()
        };

        Assert.All(dutyList, duty => Assert.False(string.IsNullOrWhiteSpace(duty)));
        Assert.Equal(3, dutyList.Distinct().Count());
    }
}

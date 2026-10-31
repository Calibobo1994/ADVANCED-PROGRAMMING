using HospitalManagement.Models;
using HospitalManagement.Services;

StaffManager staffManager = new();

while (true)
{
    ShowMenu();

    Console.Write("Select an option: ");
    string? menuOption = Console.ReadLine()?.Trim();

    switch (menuOption)
    {
        case "1":
        {
            string staffName = InputPrompt.ReadRequired("Full name: ");
            string id = InputPrompt.ReadRequired("Staff ID: ");
            string dept = InputPrompt.ReadRequired("Department: ");
            string specialty = InputPrompt.ReadRequired("Specialisation: ");
            string licenceNo = InputPrompt.ReadRequired("Licence number: ");

            var newDoctor = new Doctor(staffName, id, dept, specialty, licenceNo);
            staffManager.Add(newDoctor);
            break;
        }

        case "2":
        {
            string staffName = InputPrompt.ReadRequired("Full name: ");
            string id = InputPrompt.ReadRequired("Staff ID: ");
            string dept = InputPrompt.ReadRequired("Department: ");
            string ward = InputPrompt.ReadRequired("Ward: ");
            string shift = InputPrompt.ReadRequired("Shift: ");

            var newNurse = new Nurse(staffName, id, dept, ward, shift);
            staffManager.Add(newNurse);
            break;
        }

        case "3":
        {
            string staffName = InputPrompt.ReadRequired("Full name: ");
            string id = InputPrompt.ReadRequired("Staff ID: ");
            string dept = InputPrompt.ReadRequired("Department: ");
            string role = InputPrompt.ReadRequired("Role: ");
            string deskExtension = InputPrompt.ReadRequired("Desk extension: ");

            var newAdmin = new AdministrativeStaff(staffName, id, dept, role, deskExtension);
            staffManager.Add(newAdmin);
            break;
        }

        case "4":
        {
            IReadOnlyList<Staff> staffList = staffManager.GetAll();

            if (staffList.Count == 0)
            {
                Console.WriteLine("No staff records yet.");
                break;
            }

            Console.WriteLine();
            Console.WriteLine("--- All Staff ---");

            foreach (Staff staffMember in staffList)
            {
                Console.WriteLine(staffMember.GetDetails());
                Console.WriteLine($"Duty: {staffMember.PerformDuty()}");
                Console.WriteLine();
            }

            break;
        }

        case "5":
        {
            string searchId = InputPrompt.ReadRequired("Enter staff ID: ");

            if (staffManager.SearchStaff(searchId))
            {
                Console.WriteLine("Staff found.");

                foreach (Staff staffMember in staffManager.GetAll())
                {
                    if (staffMember.StaffId.Equals(searchId.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(staffMember.GetDetails());
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine("No staff member with that ID.");
            }

            break;
        }

        case "0":
            Console.WriteLine("Goodbye.");
            return;

        default:
            Console.WriteLine("Invalid option. Please enter 0-5.");
            break;
    }
}

static void ShowMenu()
{
    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine("STAFF MANAGEMENT");
    Console.WriteLine("========================================");
    Console.WriteLine("1. Add Doctor");
    Console.WriteLine("2. Add Nurse");
    Console.WriteLine("3. Add Administrative Staff");
    Console.WriteLine("4. Display All Staff");
    Console.WriteLine("5. Search Staff");
    Console.WriteLine("0. Exit");
    Console.WriteLine("========================================");
}


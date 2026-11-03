using StudentRecords.Data;
using StudentRecords.Models;

var studentRepository = new StudentRepository();
studentRepository.SeedSampleData();

Console.WriteLine("========================================");
Console.WriteLine("STUDENT RECORDS");
Console.WriteLine("========================================");
Console.WriteLine($"Students loaded: {studentRepository.Count}");

while (true)
{
    Console.Write("Enter Student ID to look up (or 0 to exit): ");
    string? studentId = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(studentId))
    {
        Console.WriteLine("Please enter a Student ID.");
        continue;
    }

    if (studentId == "0")
    {
        Console.WriteLine("Goodbye.");
        return;
    }

    Student? foundStudent = studentRepository.GetById(studentId);

    if (foundStudent is null)
    {
        Console.WriteLine($"No student found with ID {studentId}.");
        continue;
    }

    Console.WriteLine();
    Console.WriteLine($"{foundStudent.StudentId} | {foundStudent.Name}");
    Console.WriteLine(new string('-', 44));

    foreach (KeyValuePair<string, int> moduleMark in foundStudent.Marks)
    {
        Console.WriteLine($"{moduleMark.Key,-28} {moduleMark.Value,4}   {Student.GetGrade(moduleMark.Value)}");
    }

    Console.WriteLine(new string('-', 44));
    Console.WriteLine($"Average: {foundStudent.Average:F2}");
    Console.WriteLine();
}

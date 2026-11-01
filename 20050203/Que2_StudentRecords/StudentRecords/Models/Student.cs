namespace StudentRecords.Models;

public class Student
{
    public string StudentId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;

    public Dictionary<string, int> Marks { get; } = new();

    public Student(string id, string studentName)
    {
        StudentId = id;
        Name = studentName;
    }

    public double Average => Marks.Count == 0 ? 0 : Marks.Values.Average();

    public static string GetGrade(int markValue)
    {
        if (markValue < 0 || markValue > 100)
        {
            return "N/A";
        }

        if (markValue >= 70)
        {
            return "B+";
        }
        else if (markValue >= 65)
        {
            return "B";
        }
        else if (markValue >= 55)
        {
            return "C";
        }
        else if (markValue >= 45)
        {
            return "D";
        }
        else if (markValue >= 40)
        {
            return "E";
        }
        else
        {
            return "F";
        }
    }

    public override string ToString() => $"{StudentId} | {Name}";
}

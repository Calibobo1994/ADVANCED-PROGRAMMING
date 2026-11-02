using StudentRecords.Models;

namespace StudentRecords.Data;

public class StudentRepository
{
    private readonly Dictionary<string, Student> studentDictionary = new();

    public int Count => studentDictionary.Count;

    public void Add(Student newStudent)
    {
        if (newStudent is null)
        {
            return;
        }

        studentDictionary[newStudent.StudentId] = newStudent;
    }

    public Student? GetById(string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return null;
        }

        string id = studentId.Trim();

        return studentDictionary.TryGetValue(id, out Student? student) ? student : null;
    }

    public void SeedSampleData()
    {
        AddNewStudent("20001010", "Charles Severance", 70, 65, 55, 60, 72);
        AddNewStudent("20001011", "Aoife Byrne", 78, 74, 69, 71, 66);
        AddNewStudent("20001012", "Brendan Kelly", 55, 61, 58, 49, 64);
        AddNewStudent("20001013", "Ciara Murphy", 88, 91, 84, 79, 86);
        AddNewStudent("20001014", "Declan O'Brien", 44, 51, 47, 39, 53);
        AddNewStudent("20001015", "Eimear Walsh", 67, 63, 70, 72, 65);
        AddNewStudent("20001016", "Fionn Gallagher", 92, 89, 94, 90, 87);
        AddNewStudent("20001017", "Grainne Ni Dhalaigh", 73, 77, 68, 75, 70);
        AddNewStudent("20001018", "Hugh Brennan", 58, 52, 60, 55, 49);
        AddNewStudent("20001019", "Isabelle Ryan", 81, 76, 79, 83, 74);
        AddNewStudent("20001020", "Jakub Kowalski", 64, 67, 61, 70, 59);
        AddNewStudent("20001021", "Karen Doyle", 49, 45, 53, 42, 47);
        AddNewStudent("20001022", "Liam Fitzgerald", 35, 42, 38, 51, 40);
        AddNewStudent("20001023", "Maeve Sheehan", 90, 85, 88, 92, 91);
        AddNewStudent("20001024", "Niall McCarthy", 66, 69, 72, 63, 68);
        AddNewStudent("20001025", "Orla Kavanagh", 77, 80, 73, 75, 82);
        AddNewStudent("20001026", "Patrick Egan", 51, 56, 48, 54, 59);
        AddNewStudent("20001027", "Roisin Lynch", 84, 87, 81, 78, 89);
        AddNewStudent("20001028", "Sean Nolan", 62, 58, 65, 69, 57);
        AddNewStudent("20001029", "Tara Hennessy", 71, 68, 74, 66, 73);
    }

    private void AddNewStudent(string studentId, string studentName,
                            int programmingMark, int databaseMark,
                            int networkingMark, int cloudMark,
                            int forensicsMark)
    {
        var student = new Student(studentId, studentName);
        student.Marks["Advanced Programming"] = programmingMark;
        student.Marks["Adv. Databases"] = databaseMark;
        student.Marks["Networking & Security"] = networkingMark;
        student.Marks["Cloud Security"] = cloudMark;
        student.Marks["Cyber Forensics"] = forensicsMark;
        Add(student);
    }
}

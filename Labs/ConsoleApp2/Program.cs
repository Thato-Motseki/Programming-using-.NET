using System;


class Person
{
    // Fields
    public string Name;
    public int Age;

    // Method
    public void Introduce()
    {
        Console.WriteLine("My name is " + Name);
        Console.WriteLine("I am " + Age + " years old.");
    }
}

class Student : Person
{
    // Student-specific field
    public string StudentNumber;

    // Student-specific method
    public void Study()
    {
        Console.WriteLine(Name + " is studying.");
    }
}

class Lecturer : Person
{
    // Lecturer-specific field
    public string Department;

    // Lecturer-specific method
    public void Teach()
    {
        Console.WriteLine(Name + " is teaching " + Department + ".");
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Create a Student object
        Student student = new Student();

        student.Name = "Tay-Tay";
        student.Age = 21;
        student.StudentNumber = "ST001";

        Console.WriteLine("----- STUDENT -----");

        student.Introduce();       // Inherited from Person
        student.Study();           // Student's own method
        Console.WriteLine("Student Number: " + student.StudentNumber);


        Console.WriteLine();


        // Create a Lecturer object
        Lecturer lecturer = new Lecturer();

        lecturer.Name = "Mr. Smith";
        lecturer.Age = 40;
        lecturer.Department = "Computing";

        Console.WriteLine("----- LECTURER -----");

        lecturer.Introduce();      // Inherited from Person
        lecturer.Teach();          // Lecturer's own method
        Console.WriteLine("Department: " + lecturer.Department);
    }
}
using System;
using System.Collections.Generic;

public class Class1
{
    // Properties
    public string Name { get; set; }
    public string Certificate { get; set; }
    public string Organization { get; set; }

    // Default Constructor
    // Parameterized Constructor
    public Class1(string name, string certificate, string organization)
    {
        Name = name;
        Certificate = certificate;
        Organization = organization;
    }

    // Method to get class data
    public string GetClassData() 
    {
        return $"Name: {Name}, Certificate: {Certificate}, Organization: {Organization}";
    }


    public T GenericMethod<T>(T num1, T num2)
    {
        // Create a list of type T
        List<T> list = new List<T> { num1, num2 };  // Adding num1 and num2 to the list

        // Print the contents of the list
        Console.WriteLine(string.Join(", ", list));

        // Return one of the parameters (num1 or num2) or a default value of type T
        return num1;  // Or return num2, or return a default(T) if you don't want to pick either
    }
}

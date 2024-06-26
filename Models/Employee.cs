﻿using SQLite;


namespace MauiCRUDmyyrsepp.Models
{
    public class Employee
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Job { get; set; }
        public int Salary { get; set; }

        public Employee Clone() => MemberwiseClone() as Employee;

        public (bool IsValid, string? ErrorMessage) Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return (false, $"{nameof(Name)} is required.");
            }
            else if (string.IsNullOrWhiteSpace(Job))
            {
                return (false, $"{nameof(Job)} is required.");
            }
            else if (Salary <= 0)
            {
                return (false, $"{nameof(Salary)} should be greater than 0.");
            }
            return (true, null);
        }
    }
}

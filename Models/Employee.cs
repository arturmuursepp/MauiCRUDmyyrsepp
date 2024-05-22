using SQLite;


namespace MauiCRUD.Models
{
    public class Employee
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Job { get; set; }
        public int Age { get; set; }

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
            else if (Age <= 0)
            {
                return (false, $"{nameof(Age)} should be greater than 0.");
            }
            return (true, null);
        }
    }
}

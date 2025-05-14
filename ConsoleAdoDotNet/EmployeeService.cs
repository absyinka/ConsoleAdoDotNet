using ConsoleTables;
using Microsoft.Data.SqlClient;

namespace ConsoleAdoDotNet
{
    public class EmployeeService
    {
        private readonly string _connectionString;

        public EmployeeService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task AddEmployee()
        {
            Console.WriteLine("\nAdd New Employee");

            Console.Write("First Name: ");
            string firstName = Console.ReadLine()!;

            Console.Write("Last Name: ");
            string lastName = Console.ReadLine()!;

            Console.Write("Email: ");
            string email = Console.ReadLine()!;

            Console.Write("Department: ");
            string department = Console.ReadLine()!;

            Console.Write("Hire Date (yyyy-mm-dd): ");

            if (!DateTime.TryParse(Console.ReadLine(), out DateTime hireDate))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }

            Console.Write("Salary: ");

            if (!decimal.TryParse(Console.ReadLine(), out decimal salary))
            {
                Console.WriteLine("Invalid salary format.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Employees (Id,FirstName, LastName, Email, Department, HireDate, Salary) " +
                                "VALUES (@Id,@FirstName, @LastName, @Email, @Department, @HireDate, @Salary)";


                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", Guid.NewGuid());
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@LastName", lastName);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Department", department);
                command.Parameters.AddWithValue("@HireDate", hireDate);
                command.Parameters.AddWithValue("@Salary", salary);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Employee added successfully!");
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task ViewAllEmployees()
        {
            Console.WriteLine("\nAll Employees");

            ConsoleTable table = new("Name", "Email", "Department", "Hire Date", "Salary");

            using (SqlConnection connection = new(_connectionString))
            {
                string query = "SELECT * FROM Employees";
                SqlCommand command = new(query, connection);

                try
                {
                    await connection.OpenAsync();
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        table.AddRow($"{reader["FirstName"]} {reader["LastName"]}", reader["Email"], reader["Department"], ((DateTime)reader["HireDate"]).ToShortDateString(), reader["Salary"]);
                    }

                    table.Write(Format.Default);

                    reader.Close();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public void ViewEmployeeById()
        {
            Console.Write("\nEnter Employee ID: ");

            if (!Guid.TryParse(Console.ReadLine(), out Guid id))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Employees WHERE Id = @Id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        Console.WriteLine("\nEmployee Details");
                        Console.WriteLine($"ID: {reader["Id"]}");
                        Console.WriteLine($"Name: {reader["FirstName"]} {reader["LastName"]}");
                        Console.WriteLine($"Email: {reader["Email"]}");
                        Console.WriteLine($"Department: {reader["Department"]}");
                        Console.WriteLine($"Hire Date: {((DateTime)reader["HireDate"]).ToShortDateString()}");
                        Console.WriteLine($"Salary: {reader["Salary"]}");
                    }
                    else
                    {
                        Console.WriteLine("Employee not found.");
                    }
                    reader.Close();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public void UpdateEmployee()
        {
            Console.Write("\nEnter Employee ID to update: ");

            if (!Guid.TryParse(Console.ReadLine(), out Guid id))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }

            // First check if employee exists
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string checkQuery = "SELECT COUNT(*) FROM Employees WHERE Id = @Id";
                SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    int employeeCount = (int)checkCommand.ExecuteScalar();
                    if (employeeCount == 0)
                    {
                        Console.WriteLine("Employee not found.");
                        return;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return;
                }
            }

            Console.WriteLine("\nEnter new details (leave blank to keep current value):");

            Console.Write("First Name: ");
            string firstName = Console.ReadLine()!;

            Console.Write("Last Name: ");
            string lastName = Console.ReadLine()!;

            Console.Write("Email: ");
            string email = Console.ReadLine()!;

            Console.Write("Department: ");
            string department = Console.ReadLine()!;

            Console.Write("Hire Date (yyyy-mm-dd): ");
            string hireDateStr = Console.ReadLine()!;
            DateTime? hireDate = null;

            if (!string.IsNullOrEmpty(hireDateStr) && DateTime.TryParse(hireDateStr, out DateTime parsedDate))
            {
                hireDate = parsedDate;
            }

            Console.Write("Salary: ");
            string salaryStr = Console.ReadLine()!;
            decimal? salary = null;
            if (!string.IsNullOrEmpty(salaryStr) && decimal.TryParse(salaryStr, out decimal parsedSalary))
            {
                salary = parsedSalary;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Employees SET " +
                               "FirstName = ISNULL(@FirstName, FirstName), " +
                               "LastName = ISNULL(@LastName, LastName), " +
                               "Email = ISNULL(@Email, Email), " +
                               "Department = ISNULL(@Department, Department), " +
                               "HireDate = ISNULL(@HireDate, HireDate), " +
                               "Salary = ISNULL(@Salary, Salary) " +
                               "WHERE Id = @Id";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@FirstName", string.IsNullOrEmpty(firstName) ? (object)DBNull.Value : firstName);
                command.Parameters.AddWithValue("@LastName", string.IsNullOrEmpty(lastName) ? (object)DBNull.Value : lastName);
                command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                command.Parameters.AddWithValue("@Department", string.IsNullOrEmpty(department) ? (object)DBNull.Value : department);
                command.Parameters.AddWithValue("@HireDate", hireDate.HasValue ? (object)hireDate.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Salary", salary.HasValue ? (object)salary.Value : DBNull.Value);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Employee updated successfully!");
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public void DeleteEmployee()
        {
            Console.Write("\nEnter Employee ID to delete: ");

            if (!Guid.TryParse(Console.ReadLine(), out Guid id))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Employees WHERE Id = @Id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Employee deleted successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Employee not found.");
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}

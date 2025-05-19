using ConsoleAdoDotNet;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

string connectionString = configuration.GetConnectionString("DefaultConnection")!;
var employeeService = new EmployeeService(connectionString);

const string MenuPrompt = @"
╔════════════════════════════╗
║  Employee Management System║
╠════════════════════════════╣
║  1. Add Employee           ║
║  2. View All Employees     ║
║  3. View Employee by ID    ║
║  4. Update Employee        ║
║  5. Delete Employee        ║
║  6. Exit                   ║
╚════════════════════════════╝";

while (true)
{
    try
    {
        Console.Clear();
        Console.WriteLine(MenuPrompt);
        Console.Write("Enter your choice (1-6): ");

        if (!int.TryParse(Console.ReadLine(), out int choice))
        {
            Console.WriteLine("Invalid input. Please enter a number.");
            await Task.Delay(2000);
            continue;
        }

        switch (choice)
        {
            case 1:
                await employeeService.AddEmployee();
                break;
            case 2:
                await employeeService.ViewAllEmployees();
                break;
            case 3:
                employeeService.ViewEmployeeById();
                break;
            case 4:
                employeeService.UpdateEmployee();
                break;
            case 5:
                employeeService.DeleteEmployee();
                break;
            case 6:
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                await Task.Delay(2000);
                break;
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}
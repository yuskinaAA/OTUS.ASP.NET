using System.Threading.Tasks;
using MongoDB.Driver;
using Pcf.Administration.Core.Domain.Administration;

namespace Pcf.Administration.DataAccess.Data;

public class MongoDbInitializer : IDbInitializer
{
    private readonly IMongoDatabase _database;

    public MongoDbInitializer(IMongoDatabase database)
    {
        _database = database;
    }

    public void InitializeDb()
    {
        InitializeDbAsync().Wait();
    }

    private async Task InitializeDbAsync()
    {
        await _database.DropCollectionAsync("Employees");
        await _database.DropCollectionAsync("Roles");
        await _database.CreateCollectionAsync("Employees");
        await _database.CreateCollectionAsync("Roles");

        await SeedRolesAsync();
        await SeedEmployeesAsync();
    }

    private async Task SeedRolesAsync()
    {
        var rolesCollection = _database.GetCollection<Role>("Roles");
        var roles = FakeDataFactory.Roles;

        await rolesCollection.InsertManyAsync(roles);
    }

    private async Task SeedEmployeesAsync()
    {
        var employeesCollection = _database.GetCollection<Employee>("Employees");
        var employees = FakeDataFactory.Employees;

        foreach (var employee in employees)
        {
            if (employee.Role != null)
            {
                employee.RoleId = employee.Role.Id;
            }
        }

        await employeesCollection.InsertManyAsync(employees);
    }
}
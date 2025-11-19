using System;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pcf.Administration.Core.Domain.Administration;
using Pcf.Administration.DataAccess.Settings;

namespace Pcf.Administration.DataAccess;

public class MongoDBContext
{
    private readonly IMongoDatabase _database;

    public IMongoCollection<Employee> Employees =>
        _database.GetCollection<Employee>("employees");

    public IMongoCollection<Role> Roles =>
        _database.GetCollection<Role>("roles");

    public MongoDBContext(IOptions<MongoDBSettings> mongoDBSettings)
    {
        var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        _database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
    }

    public IMongoDatabase Database => _database;

    public IMongoCollection<T> GetCollection<T>()
    {
        var type = typeof(T);

        if (type == typeof(Employee))
            return Employees as IMongoCollection<T>;
        else if (type == typeof(Role))
            return Roles as IMongoCollection<T>;
        else
            throw new InvalidOperationException($"No collection defined for type {type.Name}");
    }
}

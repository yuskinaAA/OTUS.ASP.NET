using System;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pcf.Administration.Core.Domain.Administration;
using Pcf.Administration.DataAccess.Settings;

namespace Pcf.Administration.DataAccess;

public class MongoDBContext
{
    private readonly IMongoDatabase _database;

    public MongoDBContext(IOptions<MongoDBSettings> mongoDBSettings)
    {
        var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        _database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
    }

    public IMongoDatabase Database => _database;

    public IMongoCollection<T> GetCollection<T>(string collectionName = null)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            collectionName = typeof(T).Name + "s";
        }

        return _database.GetCollection<T>(collectionName);
    }
}

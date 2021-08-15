using System;
using MongoDB.Driver;

namespace TehGM.EinherjiBot.Database
{
    public interface IMongoConnection
    {
        MongoClient Client { get; }
        event Action<MongoClient> ClientChanged;
        void RegisterClassMap<T>();
    }
}

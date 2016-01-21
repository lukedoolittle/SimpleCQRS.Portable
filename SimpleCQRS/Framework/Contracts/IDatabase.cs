using System.Collections.Generic;
using SimpleCQRS.Domain;

namespace SimpleCQRS.Framework.Contracts
{
    public interface IDatabase<TEntity>
        where TEntity : Entity
    {
        TEntity Get(string id);
        IEnumerable<TEntity> GetAll(string aggregateId); 
        void Update(TEntity item);
        void Put(TEntity item);
        void Delete(string id);
    }

    public interface IDatabase
    {
        IEnumerable<TEntity> GetAll<TEntity>(string aggregateId)
            where TEntity : Entity;
    }
}

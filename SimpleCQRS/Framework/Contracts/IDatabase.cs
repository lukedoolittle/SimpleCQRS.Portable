using System.Collections.Generic;

namespace SimpleCQRS.Framework.Contracts
{
    public interface IDatabase<TEntity>
        where TEntity : class, IUnique
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
            where TEntity : class;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCQRS.Domain;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Test.Mocks
{
    public class DatabaseMock<TEntity> : MockBase<IDatabase<TEntity>>, IDatabase<TEntity>
        where TEntity : Entity
    {
        private readonly Dictionary<string, TEntity> _inMemoryDatabase;
         
        public DatabaseMock()
        {
            _inMemoryDatabase = new Dictionary<string, TEntity>();
        } 

        public TEntity Get(string id)
        {
            if(_inMemoryDatabase.ContainsKey(id))
            {
                return _inMemoryDatabase[id];
            }
            else
            {
                return default(TEntity);
            }
        }

        public IEnumerable<TEntity> GetAll(string aggregateId)
        {
            return _inMemoryDatabase
                .Values
                .Where(e => e.GetPropertyValue<Guid>("AggregateId").ToString() == aggregateId);
        }

        public void Update(TEntity item)
        {
            var id = item.Id.ToString();

            if (!_inMemoryDatabase.ContainsKey(id))
            {
                throw new Exception();
            }

            _inMemoryDatabase[id] = item;
        }

        public void Put(TEntity item)
        {
            _inMemoryDatabase[item.Id.ToString()] = item;
        }

        public void Delete(string id)
        {
            throw new NotImplementedException();
        }
    }

    public class GenericDatabaseMock : MockBase<IDatabase>, IDatabase
    {
        private readonly Dictionary<string, object> _inMemoryDatabase;

        public GenericDatabaseMock()
        {
            _inMemoryDatabase = new Dictionary<string, object>();
        }

        public void Put<TEntity>(TEntity item)
            where TEntity : class, IUnique
        {
            _inMemoryDatabase[item.Id.ToString()] = item;
        }

        public IEnumerable<TEntity> GetAll<TEntity>(string aggregateId) 
            where TEntity : Entity
        {
            return _inMemoryDatabase
                .Values
                .Where(e => e.GetPropertyValue<Guid>("AggregateId").ToString() == aggregateId)
                .Cast<TEntity>();
        }
    }
}

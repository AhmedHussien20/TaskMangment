using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.IRepository;


namespace TaskMangment.Infrastructure.Repositories
{
    public class MongoRepository<T> : IMongoRepository<T>
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<T>(typeof(T).Name);
        }

        public async Task<List<T>> GetAllAsync()
            => await _collection.Find(_ => true).ToListAsync();

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _collection.Find(predicate).ToListAsync();

        public async Task<T> GetByIdAsync(string id)
            => await _collection.Find(Builders<T>.Filter.Eq("Id", id)).FirstOrDefaultAsync();

        public async Task AddAsync(T entity)
            => await _collection.InsertOneAsync(entity);

        public async Task UpdateAsync(string id, T entity)
            => await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("Id", id), entity);

        public async Task DeleteAsync(string id)
            => await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("Id", id));
    }
}

using Microsoft.EntityFrameworkCore; 
using System.Linq.Expressions; 
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> expression = null)
        {
            var query = _dbSet.Where(x => !x.IsDeleted);

            if (expression != null)
                query = query.Where(expression);

            return query.AsQueryable();
        }

        public async Task<TEntity> GetByIDAsync(int id)
        {
            return await _dbSet
                .Where(x => x.Id == id && !x.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void SaveInclude(TEntity entity, params string[] properties)
        {
            _dbSet.Attach(entity);
            foreach (var prop in properties)
                _context.Entry(entity).Property(prop).IsModified = true;
        }

        public void SoftDelete(TEntity entity)
        {
            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.UtcNow;

            _dbSet.Update(entity);
        }

        public void SoftDeleteRange(IEnumerable<TEntity> entities)
        {
            foreach (var e in entities)
            {
                e.IsDeleted = true;
                e.DeletedDate = DateTime.UtcNow;
                _dbSet.Update(e);
            }
        }

        public void HardDelete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            SoftDelete(entity);
            await SaveChangesAsync();
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            SoftDeleteRange(entities);
        }

        public async Task<bool> IsExistAsync(int id)
        {
            return await _dbSet.AnyAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet
                .Where(x => !x.IsDeleted)
                .Where(predicate)
                .CountAsync();
        }
    }
}

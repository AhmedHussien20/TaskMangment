using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;
using Task = System.Threading.Tasks.Task;

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
            return expression == null ? _dbSet.AsQueryable() : _dbSet.Where(expression);
        }

        public async Task<TEntity> GetByIDAsync(int id)
        {
            return await _dbSet.FindAsync(id);
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
            {
                _context.Entry(entity).Property(prop).IsModified = true;
            }
        }


        public void SoftDelete(TEntity entity)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.IsDeleted = true;
                baseEntity.DeletedDate = DateTime.UtcNow;
                _dbSet.Update(entity);  
            }
            else
            {
                _dbSet.Remove(entity); 
            }
        }
        public void HardDelete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }



        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            _dbSet.Remove(entity);
            await SaveChangesAsync();
        }

        public async Task<bool> IsExistAsync(int id)
        {
            return await _dbSet.AnyAsync(e => e.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }
    }
}

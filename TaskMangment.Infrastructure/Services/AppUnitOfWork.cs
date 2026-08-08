using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Services
{
    public class AppUnitOfWork : IAppUnitOfWork
    {
        private readonly AppDbContext _db;
        private IDbContextTransaction? _tx;

        public AppUnitOfWork(AppDbContext db)
        {
            _db = db;
        }

        public async Task BeginTransactionAsync()
            => _tx = await _db.Database.BeginTransactionAsync();

        public async Task CommitAsync()
        {
            if (_tx != null) await _tx.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            if (_tx != null) await _tx.RollbackAsync();
        }
    }

}

using InternsRegister.Persistence;

namespace InternsRegister.Infrastructure.Repositoties
{
    public class Repository<T> where T : class
    {
        protected readonly InternsRegisterDbContext _context;
        public Repository(InternsRegisterDbContext context) => _context = context;

        public async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}

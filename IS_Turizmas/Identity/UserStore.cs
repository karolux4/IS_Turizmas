using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IS_Turizmas.Models;

namespace IS_Turizmas.Identity
{
    public class UserStore : IUserStore<RegistruotiVartotojai>, IUserPasswordStore<RegistruotiVartotojai>
    {
        private readonly ApplicationDbContext _context;

        public UserStore(ApplicationDbContext _context)
        {
            this._context = _context;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
        }

        public Task<string> GetUserIdAsync(RegistruotiVartotojai user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(RegistruotiVartotojai user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Slapyvardis);
        }

        public Task SetUserNameAsync(RegistruotiVartotojai user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException(nameof(SetUserNameAsync));
        }

        public Task<string> GetNormalizedUserNameAsync(RegistruotiVartotojai user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException(nameof(GetNormalizedUserNameAsync));
        }

        public Task SetNormalizedUserNameAsync(RegistruotiVartotojai user, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.FromResult((object)null);
        }

        public async Task<IdentityResult> CreateAsync(RegistruotiVartotojai user, CancellationToken cancellationToken)
        {
            _context.Add(user);

            await _context.SaveChangesAsync(cancellationToken);

            return await Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> UpdateAsync(RegistruotiVartotojai user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException(nameof(UpdateAsync));
        }

        public async Task<IdentityResult> DeleteAsync(RegistruotiVartotojai user, CancellationToken cancellationToken)
        {
            _context.Remove(user);

            int i = await _context.SaveChangesAsync(cancellationToken);

            return await Task.FromResult(i == 1 ? IdentityResult.Success : IdentityResult.Failed());
        }

        public async Task<RegistruotiVartotojai> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (int.TryParse(userId, out int id))
            {
                return await _context.RegistruotiVartotojai.FindAsync(id);
            }
            else
            {
                return await Task.FromResult((RegistruotiVartotojai)null);
            }
        }

        public async Task<RegistruotiVartotojai> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _context.RegistruotiVartotojai.FirstOrDefaultAsync(p => p.Slapyvardis.Equals(normalizedUserName, StringComparison.OrdinalIgnoreCase), cancellationToken);
        }

        public Task SetPasswordHashAsync(RegistruotiVartotojai user, string passwordHash, CancellationToken cancellationToken)
        {
            user.Slaptazodis = passwordHash;

            return Task.FromResult((object)null);
        }

        public Task<string> GetPasswordHashAsync(RegistruotiVartotojai user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Slaptazodis);
        }

        public Task<bool> HasPasswordAsync(RegistruotiVartotojai user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(user.Slaptazodis));
        }
    }
}

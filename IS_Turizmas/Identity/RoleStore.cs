using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IS_Turizmas.Models;
using Microsoft.EntityFrameworkCore;

namespace IS_Turizmas.Identity
{
    public class RoleStore : IRoleStore<VartotojoPlanai>
    {
        private readonly ApplicationDbContext _context;

        public RoleStore(ApplicationDbContext _context)
        {
            this._context = _context;
        }

        public void Dispose()
        {
        }

        public async Task<IdentityResult> CreateAsync(VartotojoPlanai role, CancellationToken cancellationToken)
        {
            _context.Add(role);

            await _context.SaveChangesAsync(cancellationToken);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> UpdateAsync(VartotojoPlanai role, CancellationToken cancellationToken)
        {
            _context.Update(role);

            await _context.SaveChangesAsync(cancellationToken);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> DeleteAsync(VartotojoPlanai role, CancellationToken cancellationToken)
        {
            _context.Remove(role);

            await _context.SaveChangesAsync(cancellationToken);

            return await Task.FromResult(IdentityResult.Success);
        }

        public Task<string> GetRoleIdAsync(VartotojoPlanai role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(VartotojoPlanai role, CancellationToken cancellationToken)
        {
            var type = _context.VartotojoPlanoTipai.Where(o => o.Id == role.Tipas).FirstOrDefault();
            return Task.FromResult(type.Name.ToString());
        }

        public Task SetRoleNameAsync(VartotojoPlanai role, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException(nameof(SetRoleNameAsync));
        }

        public Task<string> GetNormalizedRoleNameAsync(VartotojoPlanai role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException(nameof(GetNormalizedRoleNameAsync));
        }

        public Task SetNormalizedRoleNameAsync(VartotojoPlanai role, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.FromResult((object)null);
        }

        public async Task<VartotojoPlanai> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            if (int.TryParse(roleId, out int id))
            {
                return await _context.VartotojoPlanai.FindAsync(id);
            }
            else
            {
                return await Task.FromResult((VartotojoPlanai)null);
            }
        }

        public async Task<VartotojoPlanai> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await _context.VartotojoPlanai.FirstOrDefaultAsync(p => p.TipasNavigation.Name.Equals(normalizedRoleName, StringComparison.OrdinalIgnoreCase), cancellationToken);
        }
    }
}

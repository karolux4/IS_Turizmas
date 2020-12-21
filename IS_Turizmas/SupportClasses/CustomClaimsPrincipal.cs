using IS_Turizmas.Models;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace IS_Turizmas.SupportClasses
{
    public class CustomClaimsPrincipal : ClaimsPrincipal
    {
        private string username;
        public CustomClaimsPrincipal(IPrincipal principal) : base(principal)
        {
            if (principal != null)
            {
                username = principal.Identity.Name;
            }
        }

        public override bool IsInRole(string role)
        {
            ApplicationDbContext _context = new ApplicationDbContext();
            // ...
            if (username == null)
            {
                return false;
            }
            if(_context.VartotojoPlanai.Where(o => o.TipasNavigation.Name==role && o.DataIki==null &&
            o.FkRegistruotasVartotojasNavigation.Slapyvardis == username).FirstOrDefault() != null)
            {
                return true;
            }
            return false;
        }
    }

    public class ClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var customPrincipal = new CustomClaimsPrincipal(principal) as ClaimsPrincipal;
            return Task.FromResult(customPrincipal);
        }
    }
}

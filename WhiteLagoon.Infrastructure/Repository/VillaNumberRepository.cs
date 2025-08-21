using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class VillaNumberRepository : Repository<VillaNumber>, IVillaNumberRepository
    {
        private ApplicationDbContext _context;
        public VillaNumberRepository(ApplicationDbContext context):base(context) 
        {
            _context = context;
        }
        public void Update(VillaNumber villaNumber)
        {
           _context.Update(villaNumber);
        }
    }
}

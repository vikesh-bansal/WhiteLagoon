using Microsoft.EntityFrameworkCore;
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
    public class VillaRepository :Repository<Villa>, IVillaRepository
    {
        private readonly ApplicationDbContext _context;
        public VillaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        //public void Add(WhiteLagoon.Domain.Entities.Villa entity)
        //{ 
        //    _context.Add(entity);
        //}

        //public WhiteLagoon.Domain.Entities.Villa Get(System.Linq.Expressions.Expression<Func<WhiteLagoon.Domain.Entities.Villa, bool>> filter, string? includeProperties = null)
        //{
        //    IQueryable<Villa> query = _context.Set<Villa>();
        //    if (filter != null)
        //    {
        //        query = query.Where(filter);
        //    }
        //    if (!string.IsNullOrEmpty(includeProperties))
        //    {
        //        foreach (var incProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            query = query.Include(incProp);
        //        }
        //    }
        //    return query.FirstOrDefault();
        //}

        //public IEnumerable<WhiteLagoon.Domain.Entities.Villa> GetAll(System.Linq.Expressions.Expression<Func<WhiteLagoon.Domain.Entities.Villa, bool>>? filter = null, string? includeProperties = null)
        //{
        //    IQueryable<Villa> query = _context.Set<Villa>();
        //    if (filter != null)
        //    {
        //        query = query.Where(filter);
        //    }
        //    if (!string.IsNullOrEmpty(includeProperties))
        //    {
        //        foreach(var incProp in includeProperties.Split(new char[] { ','},StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            query = query.Include(incProp);
        //        }
        //    }
        //    return query.ToList();
        //}

        //public void Remove(WhiteLagoon.Domain.Entities.Villa entity)
        //{
        //    _context.Remove(entity);
        //}

        //public void Save()
        //{
        //    _context.SaveChanges(); 
        //}

        public void Update(WhiteLagoon.Domain.Entities.Villa entity)
        {
            _context.Update(entity);
        }
    }
}

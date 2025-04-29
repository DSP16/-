using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ИП_Хевеши.Data;
using System.Data.Entity;

namespace ИП_Хевеши.Classes
{
    internal class SearchArrivalsBack
    {
        public static ИП_ХевешиEntities _context;
        public List<Arrivals> GetArrivalsByName(string name)
        {
            _context = new ИП_ХевешиEntities();
            return _context.Arrivals
                            .Include(a => a.Components).Include(a => a.Users)
                            .Where(a => a.Components.Name.Contains(name))
                            .ToList();
        }
    }
}

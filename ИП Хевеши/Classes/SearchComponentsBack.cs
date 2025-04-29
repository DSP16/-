using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ИП_Хевеши.Data;
using System.Data.Entity;

namespace ИП_Хевеши.Classes
{
    internal class SearchComponentsBack
    {
        public static ИП_ХевешиEntities _context;
        public List<Components> GetComponentsByName(string name)
        {
            _context = new ИП_ХевешиEntities();
            return _context.Components
                            .Include(c => c.Manufacturers)
                            .Where(c => c.Name.Contains(name))
                            .ToList();
        }
    }
}

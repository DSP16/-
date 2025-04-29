using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ИП_Хевеши.Data;
using System.Data;
using System.Configuration;


namespace ИП_Хевеши.Classes
{
    internal class LoadProductCards
    {

        public List<Components> GetComponentsList()
        {
            using (var context = new ИП_ХевешиEntities())
            {
                var components = context.Components
                                        .Include("Manufacturers")
                                        .ToList();
                return components;
            }
        }
    }
}

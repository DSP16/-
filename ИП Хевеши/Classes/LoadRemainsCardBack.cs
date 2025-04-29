using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ИП_Хевеши.Data;

namespace ИП_Хевеши.Classes
{
    internal class LoadRemainsCardBack
    {

        public List<Components> GetComponentsList()
        {

            using (var context = new ИП_ХевешиEntities())
            {
                try
                {
                    var components =  context.Components
                                                  .Include("Manufacturers")
                                                  .Where(c => c.Quantity > 0)
                                                  .ToList();
                    return components;
                }
                catch (Exception ex)
                {
                    // Логируем ошибку
                    Console.WriteLine(ex.Message);
                    // Обработка или повторное выбрасывание исключения
                    throw;
                }
                
            }
        }
    }
}

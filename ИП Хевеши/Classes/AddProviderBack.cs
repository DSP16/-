using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ИП_Хевеши.Data;
using ИП_Хевеши.UI.Pages;
using ИП_Хевеши.UI.Winds;
using ИП_Хевеши.Classes;

namespace ИП_Хевеши.Classes
{
    public class AddProviderBack
    {
        private static ИП_ХевешиEntities _context = new ИП_ХевешиEntities();
        public void AddProvider(TextBox tbProviderName, TextBox tbCountry)
        {
            try
            {
                if (tbProviderName.Text == "" || tbCountry.Text == "" )
                {
                    MessageBox.Show("Заполните все поля!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    var provider = new Providers
                    {
                        Name = tbProviderName.Text,
                        Country = tbCountry.Text,
                        

                    };
                    _context.Providers.Add(provider);
                    _context.SaveChanges();
                    MessageBox.Show("Поставщик добавлен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

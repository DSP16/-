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
using ИП_Хевеши.Classes;
using ИП_Хевеши.Data;

namespace ИП_Хевеши.UI.Winds
{
    /// <summary>
    /// Логика взаимодействия для AddProviderWn.xaml
    /// </summary>
    public partial class AddProviderWn : Window
    {
        
        public AddProviderWn()
        {
            InitializeComponent();
        }

      

        private void btnAddProvider_Click(object sender, RoutedEventArgs e)
        {
            AddProviderBack addProviderBack = new AddProviderBack();
            addProviderBack.AddProvider(tbProviderName, tbCountry);
        }
    }
}

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

namespace ИП_Хевеши.UI.Winds
{
    /// <summary>
    /// Логика взаимодействия для AddBuyerWn.xaml
    /// </summary>
    public partial class AddBuyerWn : Window
    {
        public AddBuyerWn()
        {
            InitializeComponent();
        }

     

        private void btnAddBuyer_Click(object sender, RoutedEventArgs e)
        {
            AddBuyerBack addBuyerBack = new AddBuyerBack();
            addBuyerBack.AddBuyer(tbBuyerName, tbCountry);
        }
    }
}

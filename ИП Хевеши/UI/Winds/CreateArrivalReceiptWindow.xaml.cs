using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using ИП_Хевеши.Views;

namespace ИП_Хевеши.UI.Winds
{
    /// <summary>
    /// Логика взаимодействия для CreateArrivalReceiptWindow.xaml
    /// </summary>
    public partial class CreateArrivalReceiptWindow : Window
    {
        public int CreatedReceiptId { get; private set; }

        public CreateArrivalReceiptWindow()
        {
            InitializeComponent();

            using (var db = new ИП_ХевешиEntities())
            {
                cbProvider.ItemsSource = db.Providers.ToList();
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (cbProvider.SelectedValue == null)
            {
                MessageBox.Show("Выберите поставщика.");
                return;
            }

            using (var db = new ИП_ХевешиEntities())
            {
                // Автогенерация номера
               

                var receipt = new Receipts
                {
                    ReceiptNumber = null, 
                    Date = DateTime.Now,
                    ProviderID = (int)cbProvider.SelectedValue,
                    UserID = CurrentUser.UserID + 1
                };

                db.Receipts.Add(receipt);
                db.SaveChanges();

                CreatedReceiptId = receipt.ID;
                DialogResult = true;
                Close();
            }
        }
    }
}

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

namespace ИП_Хевеши.Winds
{
    /// <summary>
    /// Логика взаимодействия для CreateIssuanceReceiptWindow.xaml
    /// </summary>
    public partial class CreateIssuanceReceiptWindow : Window
    {
        public int CreatedReceiptId { get; private set; }

        public CreateIssuanceReceiptWindow()
        {
            InitializeComponent();

            using (var db = new ИП_ХевешиEntities())
            {
                cbBuyer.ItemsSource = db.Buyers.ToList();
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (cbBuyer.SelectedValue == null)
            {
                MessageBox.Show("Выберите покупателя.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            using (var db = new ИП_ХевешиEntities())
            {
                var receipt = new IssuanceReceipts
                {
                    ReceiptNumber = null, 
                    Date = DateTime.Now,
                    BuyerID = (int)cbBuyer.SelectedValue,
                    UserID = CurrentUser.UserID + 1
                };

                db.IssuanceReceipts.Add(receipt);
                db.SaveChanges();

                CreatedReceiptId = receipt.ID;
                DialogResult = true;
                Close();
            }
        }
    }
}

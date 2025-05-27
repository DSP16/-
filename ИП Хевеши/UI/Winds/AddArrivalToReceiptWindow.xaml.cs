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
using ИП_Хевеши.UI.Pages;
using ИП_Хевеши.Views;

namespace ИП_Хевеши.UI.Winds
{
    /// <summary>
    /// Логика взаимодействия для AddArrivalToReceiptWindow.xaml
    /// </summary>
    public partial class AddArrivalToReceiptWindow : Window
    {
        private readonly int receiptId;
        public string nextNumber;
        public AddArrivalToReceiptWindow(int receiptId)
        {
            InitializeComponent();
            this.receiptId = receiptId;

            using (var db = new ИП_ХевешиEntities())
            {
                cbComponent.ItemsSource = db.Components.ToList();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cbComponent.SelectedValue == null ||
       !int.TryParse(tbQuantity.Text, out int qty) ||
       !decimal.TryParse(tbPrice.Text, out decimal price))
            {
                MessageBox.Show("Проверьте корректность ввода.");
                return;
            }

            using (var db = new ИП_ХевешиEntities())
            {
                int componentId = (int)cbComponent.SelectedValue;

                var component = db.Components.FirstOrDefault(c => c.ID == componentId);
                if (component == null)
                {
                    MessageBox.Show("Компонент не найден.");
                    return;
                }
                if (receiptId <= 0)
                {
                    MessageBox.Show("Ошибка: неверный ID накладной.");
                    return;
                }
                // 
                var receipt = db.Receipts.Find(receiptId);
                if (receipt.ReceiptNumber == null)
                {
                     nextNumber = GenerateNextNumber(db);
                    receipt.ReceiptNumber = nextNumber;
                }

                var arrival = new Arrivals
                {
                    ComponentID = componentId,
                    Quantity = qty,
                    PurchasePrice = price,
                    ArrivalDate = DateTime.Now,
                    ReceiptID = receipt.ID,
                    ProviderID = receipt.ProviderID,
                    UserID = CurrentUser.UserID + 1
                };

                db.Arrivals.Add(arrival);

                component.Quantity += qty;

               
                db.SaveChanges();
                
            }

            DialogResult = true;
            

        }

        private string GenerateNextNumber(ИП_ХевешиEntities db)
        {
            var last = db.Receipts
                         .Where(r => r.ReceiptNumber != null)
                         .OrderByDescending(r => r.ID)
                         .FirstOrDefault();

            if (last == null) return "НК-001";

            int lastNum = int.Parse(Regex.Match(last.ReceiptNumber, @"\d+").Value);
            return $"НК-{(lastNum + 1):D3}";
        }

        private void tbQuantity_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text == "Количество")
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        private void tbQuantity_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "Количество";
                tb.Foreground = Brushes.Gray;
            }
        }

        private void tbPrice_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text == "Цена закупки")
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        private void tbPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "Цена закупки";
                tb.Foreground = Brushes.Gray;
            }
        }
    }
}

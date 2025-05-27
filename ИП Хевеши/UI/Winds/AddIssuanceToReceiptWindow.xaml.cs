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
using System.Data.Entity;
using System.Text.RegularExpressions;
using ИП_Хевеши.Views;

namespace ИП_Хевеши.UI.Winds
{
    /// <summary>
    /// Логика взаимодействия для AddIssuanceToReceiptWindow.xaml
    /// </summary>
    public partial class AddIssuanceToReceiptWindow : Window
    {
        private readonly int receiptId;
        public string nextNumber;
        public AddIssuanceToReceiptWindow(int receiptId)
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
                !int.TryParse(tbQuantity.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное количество.");
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

                if (component.Quantity < qty)
                {
                    MessageBox.Show($"Недостаточно на складе. Остаток: {component.Quantity}", "Недостаточно на складе", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var receipt = db.IssuanceReceipts.Find(receiptId);
               

                var issuance = new Issuance
                {
                    ComponentID = component.ID,
                    Quantity = qty,
                    IssuanceDate = DateTime.Now,
                    ReceiptID = receipt.ID,
                    BuyerID = receipt.BuyerID,
                    UserID = CurrentUser.UserID - 1
                };

                db.Issuance.Add(issuance);

                // Обновление остатков
                component.Quantity -= qty;
                if (receipt.ReceiptNumber == null)
                {
                    string nextNumber = GenerateNextNumber(db);
                    receipt.ReceiptNumber = nextNumber;
                }
               
                    db.SaveChanges();
            }

            DialogResult = true;
            Close();
        }
        private string GenerateNextNumber(ИП_ХевешиEntities db)
        {
            var last = db.IssuanceReceipts
                         .Where(r => r.ReceiptNumber != null)
                         .OrderByDescending(r => r.ID)
                         .FirstOrDefault();

            if (last == null) return "РАС-001";

            int lastNum = int.Parse(Regex.Match(last.ReceiptNumber, @"\d+").Value);
            return $"РАС-{(lastNum + 1):D3}";
        }
        private void Quantity_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tbQuantity.Text == "Количество")
            {
                tbQuantity.Text = "";
                tbQuantity.Foreground = Brushes.Black;
            }
        }

        private void Quantity_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbQuantity.Text))
            {
                tbQuantity.Text = "Количество";
                tbQuantity.Foreground = Brushes.Gray;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для AddIssuanceWn.xaml
    /// </summary>
    public partial class AddIssuanceWn : Window
    {
        public AddIssuanceWn()
        {
            InitializeComponent();
            LoadComponents();
            LoadUsers();
            LoadBuyers();
            dpIssuanceDate.SelectedDate = DateTime.Today;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadComponents()
        {
            using (var context = new ИП_ХевешиEntities())
            {
                var components = context.Components.ToList();
                cbComponent.ItemsSource = components;
            }
        }

        private void LoadBuyers()
        {
            using (var context = new ИП_ХевешиEntities())
            {
                var buyers = context.Buyers
                    .Select(p => new
                    {
                        p.ID,
                        BuyerName = p.Name
                    })
                    .ToList();
                cbBuyer.ItemsSource = buyers;
            }
        }
        private void LoadUsers()
        {
            using (var context = new ИП_ХевешиEntities())
            {
                var users = context.Users
                    .Select(u => new
                    {
                        u.ID,
                        FullName = u.UserName + " " + u.UserSurname
                    })
                    .ToList();
                cbUser.ItemsSource = users;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddIssuanceBack addIssuanceBack = new AddIssuanceBack();
            addIssuanceBack.AddIssuance(tbQuantity, cbComponent, cbUser, dpIssuanceDate, cbBuyer);
        }

      
    }
}

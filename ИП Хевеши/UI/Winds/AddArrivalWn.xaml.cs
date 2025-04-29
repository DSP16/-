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
using ИП_Хевеши.UI.Winds;
using ИП_Хевеши.UI.Pages;
using System.Runtime.CompilerServices;
using ИП_Хевеши.Classes;

namespace ИП_Хевеши.UI.Winds
{
    /// <summary>
    /// Логика взаимодействия для AddArrivalWn.xaml
    /// </summary>
    public partial class AddArrivalWn : Window
    {
        public AddArrivalWn()
        {
            InitializeComponent();
            LoadComponents();
            LoadUsers();
            LoadProviders();
            dpArrivalDate.SelectedDate = DateTime.Today;
        }

        private void LoadComponents()
        {
            using (var context = new ИП_ХевешиEntities())
            {
                var components = context.Components.ToList();
                cbComponent.ItemsSource = components;
            }
        }

        private void LoadProviders()
        {
            using (var context = new ИП_ХевешиEntities())
            {
                var providers = context.Providers
                    .Select(p => new
                    {
                        p.ID,
                        ProviderName = p.Name 
                    })
                    .ToList();
                cbProvider.ItemsSource = providers;
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

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAddArrival_Click(object sender, RoutedEventArgs e)
        {
            AddArrivalBack addArrivalBack = new AddArrivalBack();
            addArrivalBack.AddArrival(tbQuantity, tbPurchasePrice, cbComponent, cbUser, dpArrivalDate, cbProvider);
            
        }

      
    }
}

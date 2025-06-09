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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ИП_Хевеши.Data;
using ИП_Хевеши.Classes;
using ИП_Хевеши.UI.Pages;
using ИП_Хевеши.UI.Winds.TipWinds;
using ИП_Хевеши.Views;
namespace ИП_Хевеши.UI.Winds
{
    /// <summary>
    /// Логика взаимодействия для MainContentWn.xaml
    /// </summary>
    public partial class MainContentWn : Window
    {
        
        public string UserName;
        public string UserRole;
       
       
        public MainContentWn(string userName, int userId, int userRole)
        {
            InitializeComponent();
            UserName = userName;
            using (var db = new ИП_ХевешиEntities())
            {
                UserRole = db.Roles.FirstOrDefault(x => x.ID == userRole)?.RoleName;
            }
            CurrentUser.UserID = userId;
            CurrentUser.UserName = userName;
            tbProfileName.Text = UserName;
            tbProfileRole.Text = UserRole;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Manager.Frame = MainFrame;
            MainFrame.Navigate(new MainContentPage());
            btnToMain.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1C80B5"));

        }

        private void btnOpenArrivals_Click(object sender, RoutedEventArgs e)
        {
            ReceiptPage receiptPage = new ReceiptPage();
            Manager.Frame.Navigate(receiptPage);
          
            btnToMain.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
            btnOpenIssuences.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
            btnOpenDiagrams.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
            btnOpenArrivals.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1C80B5"));
        }

        private void btnOpenIssuences_Click(object sender, RoutedEventArgs e)
        {
            Manager.Frame.Navigate(new IssuanceReceiptsPage());
            btnToMain.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
            btnOpenIssuences.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1C80B5"));
            btnOpenDiagrams.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
            btnOpenArrivals.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
        }

      
        private void btnOpenDiagrams_Click(object sender, RoutedEventArgs e)
        {
            Manager.Frame.Navigate(new AnalyticsPage());
            btnToMain.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
            btnOpenIssuences.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
            btnOpenDiagrams.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1C80B5"));
            btnOpenArrivals.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            HelperCollectionWn helperCollectionWn = new HelperCollectionWn();
            helperCollectionWn.Show();
            btnToMain.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
            btnOpenIssuences.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
            btnOpenDiagrams.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
            btnOpenArrivals.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
        }

        private void btnToMain_Click(object sender, RoutedEventArgs e)
        {
            Manager.Frame.Navigate(new MainContentPage());
            btnToMain.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1C80B5"));
            btnOpenIssuences.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
            btnOpenDiagrams.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
            btnOpenArrivals.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#249ad7"));
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

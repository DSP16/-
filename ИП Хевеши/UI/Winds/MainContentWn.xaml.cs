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
        public string name;
        public int userId;
        public MainContentWn(string userName, int userId)
        {
            InitializeComponent();
            name = userName;
            CurrentUser.UserID = userId;
            CurrentUser.UserName = userName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Manager.Frame = MainFrame;
            MainFrame.Navigate(new MainContentPage(name));
        }

        private void btnOpenArrivals_Click(object sender, RoutedEventArgs e)
        {
            ReceiptPage receiptPage = new ReceiptPage();
            Manager.Frame.Navigate(receiptPage);
        }

        private void btnOpenIssuences_Click(object sender, RoutedEventArgs e)
        {
            Manager.Frame.Navigate(new IssuanceReceiptsPage());
        }

      
        private void btnOpenDiagrams_Click(object sender, RoutedEventArgs e)
        {
            Manager.Frame.Navigate(new AnalyticsPage());
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            HelperCollectionWn helperCollectionWn = new HelperCollectionWn();
            helperCollectionWn.Show();
        }

        private void btnToMain_Click(object sender, RoutedEventArgs e)
        {
            Manager.Frame.Navigate(new MainContentPage(name));
        }

    }
}

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
using ИП_Хевеши.UI.Winds;
using ИП_Хевеши.UI.Pages;
using ИП_Хевеши.Classes;
using ИП_Хевеши.Data;
namespace ИП_Хевеши
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class AuthorizeWn : Window
    {
        public static List<Users> users;
        public AuthorizeWn()
        {
            InitializeComponent();
            List<Users> users = AuthorizeBack.GetUserList();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Manager.Frame = LogRegFrame;
            LogRegFrame.Navigate(new LoginPage());
        }
    }
}

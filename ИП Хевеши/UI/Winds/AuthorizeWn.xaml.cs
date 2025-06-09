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
            users = AuthorizeBack.GetUserList();

        }

        private void btnAuthorize_Click(object sender, RoutedEventArgs e)
        {
            AuthorizeWn wn = (AuthorizeWn)Window.GetWindow(this);
            AuthorizeBack.AuthorizeUser(tbLogin, pbPassword, tbVisiblePassword, cbHideShowPassword, users, wn);
        }

        private void cbHideShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            pbPassword.Password = tbVisiblePassword.Text;
            pbPassword.Visibility = Visibility.Visible;
            tbVisiblePassword.Visibility = Visibility.Hidden;
        }

        private void cbHideShowPassword_Checked(object sender, RoutedEventArgs e)
        {

            tbVisiblePassword.Text = pbPassword.Password;
            pbPassword.Visibility = Visibility.Hidden;
            tbVisiblePassword.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult message = MessageBox.Show("Вы действитоельно хотите покинуть окно авторизации?","Выход из приложения", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (message == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
            }
            

        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}

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
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ИП_Хевеши.UI.Pages
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        List<Users> users;
        public LoginPage()
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
    }
}

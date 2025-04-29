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

namespace ИП_Хевеши.UI.Winds
{
    /// <summary>
    /// Логика взаимодействия для MainContentWn.xaml
    /// </summary>
    public partial class MainContentWn : Window
    {
        public string name;
        public MainContentWn(string userName)
        {
            InitializeComponent();
            name = userName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Manager.Frame = MainContentFrame;
            MainContentFrame.Navigate(new MainContentPage(name));
        }
    }
}

﻿using System;
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

namespace ИП_Хевеши.UI.Winds.TipWinds
{
    /// <summary>
    /// Логика взаимодействия для HelperWn.xaml
    /// </summary>
    public partial class HelperWn : Window
    {
       
        public HelperWn(string Title,string HelperText)
        {
            InitializeComponent();
            tbHelper.Text = HelperText;
            lTitle.Content = Title;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            HelperCollectionWn helperCollectionWn = new HelperCollectionWn();
            helperCollectionWn.Show();
        }
    }
}

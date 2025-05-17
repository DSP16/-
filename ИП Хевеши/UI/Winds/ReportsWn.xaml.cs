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
using ИП_Хевеши.Classes;
using ИП_Хевеши.UI.Winds.TipWinds;

namespace ИП_Хевеши.UI.Winds
{
    /// <summary>
    /// Логика взаимодействия для ReportsWn.xaml
    /// </summary>
    public partial class ReportsWn : Window
    {
        public ReportsWn()
        {
            InitializeComponent();
        }

        private async void btnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            ReportsBack reportsBack = new ReportsBack();
            if (ReportCalendar.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату в календаре.");
                return;
            }

            DateTime selectedDate = (DateTime)ReportCalendar.SelectedDate;
            int selectedMonth = selectedDate.Month;
            int enteredYear = selectedDate.Year;

            string reportType = "";
            if (ArrivalsRadioButton.IsChecked == true)
            {
                reportType = "Поступления";
            }
            else if (IssuanceRadioButton.IsChecked == true)
            {
                reportType = "Расходы";
            }
            else if (ProvidersRadioButton.IsChecked == true)
            {
                reportType = "По поставщикам";
            }
            else
            {
                MessageBox.Show("Выберите тип отчета.");
                return;
            }

            // Показываем ProgressBar и StatusTextBlock
            ReportProgressBar.Visibility = Visibility.Visible;
            StatusTextBlock.Visibility = Visibility.Visible;
            StatusTextBlock.Text = "Формирование отчета..."; // Сообщение о начале

            // Отключаем кнопку, чтобы избежать повторных нажатий
            btnGenerateReport.IsEnabled = false;

            try
            {
                await Task.Run(() => reportsBack.GenerateExcelReport(selectedMonth, enteredYear, reportType));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
                StatusTextBlock.Text = "Ошибка при формировании отчета.";
            }
            finally
            {
                ReportProgressBar.Visibility = Visibility.Collapsed;
                btnGenerateReport.IsEnabled = true;
                StatusTextBlock.Visibility = Visibility.Collapsed; // Скрываем StatusTextBlock после завершения
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите закрыть текущее окно?", "Информация", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (result == MessageBoxResult.OK)
            {
                this.Close();
            }
        }

       
    }
}

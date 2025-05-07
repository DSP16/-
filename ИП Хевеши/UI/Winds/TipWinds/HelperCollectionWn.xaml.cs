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
using ИП_Хевеши.UI.Winds.TipWinds;

namespace ИП_Хевеши.UI.Winds.TipWinds
{

    /// <summary>
    /// Логика взаимодействия для HelperCollectionWn.xaml
    /// </summary>
    public partial class HelperCollectionWn : Window
    {
        

        public HelperCollectionWn()
        {
            InitializeComponent();
        }

        private void btnOpenReportsHelper_Click(object sender, RoutedEventArgs e)
        {
            string  HelpText = "Для формирования отчета по поступлениям необходимо в календаре на странице выбрать первое число интересующего месяца и года. Далее необходимо выбрать вид отчета:" +
              " поступления или расходы. В завершении необходимо нажать  кнопку 'Сформировать отчет' и в открывшемся окне выбрать директорию сохранения сформированного отчета и при " +
              "необходимости изменить его имя. Имя образуется из типа отчета, месяца и года за который отчет формируется. После нажатия 'сохранить' системное окно свернется и на экране " +
              "появится уведомление об успешном сохранении файла с его директорией скачивания, далее к файлу можно перейти с помощью проводника. При появлении уведомления 'Нет данных для " +
              "отчета за выбранный период.' необходимо проверить существуют ли поступления за выбранную вами дату. При возникновении иных ошибок или неоднозначных результатов сформированных " +
              "отчетов следует обратиться к системному администратору предприятия. При нажатии на кнопку 'Отмена' окно формирования отчетов свернется";
            
            HelperWn helperWn = new HelperWn(HelpText);
            helperWn.Show();
            this.Close();
        }

        private void btnOpenDiagramsHelper_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

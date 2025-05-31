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

        public string HelpText;
        public string Title;
        public HelperCollectionWn()
        {
            InitializeComponent();
        }

      

        private void btnOpenDiagramsHelper_Click(object sender, RoutedEventArgs e)
        {
            string HelpText = "  На странице аналитики отображаются: \n- диаграмма выручки;\n- топ продаваемых компонентов;\n- топ неликвидных компонентов;\n- диаграмма прогноза выручки." +
                "\n   Также внизу страницы есть кнопка, которая сформирует Excel таблицу со всеми данными страницы аналитики." +
                "\n   Если одна из диаграмм или списки нелеквидов и продаваемых товаров пусты обратитесь к системному администратору";
                
            Title = "Аналитика";
            HelperWn helperWn = new HelperWn(Title, HelpText);
            helperWn.Show();
            this.Close();
          
        }

        private void btnOpenIssuancesHelper_Click(object sender, RoutedEventArgs e)
        {
            string HelpText = "На странице расходных накладных отображается список всех расходных накладных, зарегистрированных в системе. " +
                    "Пользователь может:\n" +
                    " - выбрать дату для фильтрации накладных по периоду;\n" +
                    " - применить фильтрацию, нажав кнопку 'Применить фильтр';\n" +
                    " - сбросить фильтрацию с помощью кнопки 'Сбросить';\n" +
                    " - добавить нового покупателя через кнопку 'Добавить покупателя';\n" +
                    " - просмотреть детальную информацию о каждой накладной, нажав на её карточку.\n\n" +
                    "В нижней части окна доступны:\n" +
                    " - кнопка 'Добавить накладную' для регистрации новой расходной накладной;\n" +
                    " - кнопка 'Импорт накладной' для загрузки расходных накладных из Excel файла.\n\n" +
                    "После добавления или импорта новой накладной список автоматически обновляется.";

            string Title = "Расходные накладные";
            HelperWn helperWn = new HelperWn(Title, HelpText);
            helperWn.Show();
            this.Close();
        }

        private void btnOpenComponentsHelper_Click(object sender, RoutedEventArgs e)
        {
            HelpText = "На странице 'Комплектующие' отображается список всех доступных компонентов на складе.\n" +
               "   Каждая карточка комплектующего содержит следующую информацию: наименование, оптовая цена, производитель, текущее количество и актуальность.\n" +
               "   Для поиска конкретного комплектующего используйте строку поиска в верхней части страницы.\n" +
               "   Для добавления нового комплектующего нажмите кнопку 'Добавить компонент' и заполните все обязательные поля.\n" +
               "   Для просмотра только тех комплектующих, у которых остаток ниже минимального количества, нажмите кнопку 'Остатки'.\n" +
               "   Кнопка 'Весь список' позволяет отобразить весь каталог без фильтрации по остаткам.\n" +
               "   Для обновления данных в списке используйте кнопку 'Обновить'.\n" +
               "   При возникновении ошибок или необходимости восстановления работы программы обратитесь к системному администратору.";

            Title = "Работа со списком комплектующих";
            HelperWn helperWn = new HelperWn(Title, HelpText);
            helperWn.Show();
            this.Close();
        }

        private void btnOpenReceiptssHelper_Click(object sender, RoutedEventArgs e)
        {
            string HelpText = "На странице приходных накладных отображается список всех приходных накладных, зарегистрированных в системе. " +
                     "Пользователь может:\n" +
                     " - выбрать дату для фильтрации накладных по периоду;\n" +
                     " - применить фильтрацию, нажав кнопку 'Применить фильтр';\n" +
                     " - сбросить фильтрацию с помощью кнопки 'Сбросить';\n" +
                     " - добавить нового поставщика через кнопку 'Добавить поставщика';\n" +
                     " - просмотреть детальную информацию о каждой накладной, нажав на её карточку.\n\n" +
                     "В нижней части окна доступны:\n" +
                     " - кнопка 'Добавить накладную' для регистрации новой приходной накладной;\n" +
                     " - кнопка 'Импорт накладной' для загрузки приходных накладных из Excel файла.\n\n" +
                     "После добавления или импорта новой накладной список автоматически обновляется.";

            string Title = "Приходные накладные";
            HelperWn helperWn = new HelperWn(Title, HelpText);
            helperWn.Show();
            this.Close();
        }
    }
}

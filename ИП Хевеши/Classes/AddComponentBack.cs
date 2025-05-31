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
using ИП_Хевеши.Data;
using ИП_Хевеши.UI.Pages;
using ИП_Хевеши.UI.Winds;
using ИП_Хевеши.Classes;

namespace ИП_Хевеши.Classes
{
    public class AddComponentBack
    {
        private static ИП_ХевешиEntities _context = new ИП_ХевешиEntities();
       
        public void AddComponent(TextBox tbMinQuantity, ComboBox cbActuality, TextBox tbName, TextBox tbPrice, ComboBox cbManufacturer, 
            ComboBox cbZone, TextBox tbQuantity, TextBox tbRowCell, TextBox tbType)
        {

            try
            {
                // Проверяем, что все обязательные поля заполнены
                if (tbName.Text == "" || tbPrice.Text == "" || tbQuantity.Text == "" || tbRowCell.Text == "" || tbType.Text == "" ||
                 tbMinQuantity.Text == "" || cbManufacturer.SelectedValue == null || cbZone.SelectedValue == null || cbActuality.SelectedValue == null)
                {
                    MessageBox.Show("Одно или более полей незаполненны","Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                // Преобразуем цену из строки в число. Если формат неправильный — сообщаем об ошибке.
                if (string.IsNullOrWhiteSpace(tbName.Text) || string.IsNullOrWhiteSpace(tbPrice.Text) ||
                   string.IsNullOrWhiteSpace(tbQuantity.Text) || string.IsNullOrWhiteSpace(tbRowCell.Text) ||
                   string.IsNullOrWhiteSpace(tbType.Text) || string.IsNullOrWhiteSpace(tbMinQuantity.Text) ||
                   cbManufacturer.SelectedValue == null || cbZone.SelectedValue == null || cbActuality.SelectedValue == null)
                {
                    MessageBox.Show("Все поля должны быть заполнены", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"В процессе добавления возникла ошибка: {ex.Message}. Пожалуйста, обратитесь к системному администратору", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


            try
            { 
                {
                    // Получаем строковое значение актуальности из элемента ComboBox
                    string actuality = (cbActuality.SelectedItem as ComboBoxItem).Content.ToString();
                    var component = new Components
                    {
                        Name = tbName.Text,
                        Type = tbType.Text,
                        ManufacturerID = (int)cbManufacturer.SelectedValue,
                        Price = decimal.Parse(tbPrice.Text),
                        Quantity = int.Parse(tbQuantity.Text),
                        MinQuantity = int.Parse(tbMinQuantity.Text),
                        ZoneID = (int)cbZone.SelectedValue,
                        RowCell = (string)tbRowCell.Text,
                        Actuality = actuality

                    };

                    _context.Components.Add(component);
                    _context.SaveChanges();
                    MessageBox.Show("Товар добавлен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }
    }
}

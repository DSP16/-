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
    internal class AddIssuanceBack
    {
        private static ИП_ХевешиEntities _context = new ИП_ХевешиEntities();
        public void AddIssuance(TextBox tbQuantity, ComboBox cbComponent, ComboBox cbUser, DatePicker dpIssuanceDate, ComboBox cbBuyer)
        {
            try
            {
                if (tbQuantity.Text != ""  || cbComponent.SelectedValue != null || cbUser.SelectedValue != null || dpIssuanceDate.Text != "" || cbBuyer.SelectedValue != null)
                {
                    using (var context = new ИП_ХевешиEntities())
                    {
                        var newIssuance = new Issuance
                        {
                            ComponentID = (int)cbComponent.SelectedValue,
                            IssuanceDate = dpIssuanceDate.SelectedDate,
                            Quantity = int.Parse(tbQuantity.Text),
                            UserID = (int)cbUser.SelectedValue,
                            BuyerID = (int)cbBuyer.SelectedValue
                        };

                        context.Issuance.Add(newIssuance);
                        context.SaveChanges();
                        ArrivalsPage arrivalsPage = new ArrivalsPage();
                        UpdateIssuanceQuantity(newIssuance.ComponentID, newIssuance.Quantity);
                        MessageBox.Show("Расход успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        arrivalsPage.LoadArrivals();
                    }
                }
                else
                {
                    MessageBox.Show("Заполните все поля", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public void UpdateIssuanceQuantity(int? componentId, int? quantity)
        {
            try
            {   
                using (var db = new ИП_ХевешиEntities())
                {
                    // 1. Находим комплектующее
                    var component = db.Components.FirstOrDefault(c => c.ID == componentId);
                    if (component == null)
                    {
                        MessageBox.Show("Ошибка получения комплектующего, пожалуйста, обратитесь к системному администратору", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // 2. Уменьшаем количество (проверка на достаточное количество)
                    if (component.Quantity >= quantity)
                    {
                        component.Quantity -= quantity;
                    }
                    else
                    {
                        MessageBox.Show("Недостаточно комплектующего на складе.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    // 3. Сохраняем изменения
                    db.SaveChanges();                   
                }
              
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при оформлении расхода, пожалуйста, обратитесь к системному администратору", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
        }
    }
}

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
    public class AddArrivalBack
    {
        public static ИП_ХевешиEntities _context = new ИП_ХевешиEntities();
        public void AddArrival(TextBox tbQuantity, TextBox tbPurchasePrice, ComboBox cbComponent, ComboBox cbUser, DatePicker dpArrivalDate, ComboBox cbProvider)
        {
            try
            {
                if (tbQuantity.Text != "" || tbPurchasePrice.Text != "" || cbComponent.SelectedValue != null || cbUser.SelectedValue != null || dpArrivalDate.Text != "")
                {
                    using (var context = new ИП_ХевешиEntities())
                    {
                        var newArrival = new Arrivals
                        {
                            ComponentID = (int)cbComponent.SelectedValue,
                            ArrivalDate = (DateTime)dpArrivalDate.SelectedDate,
                            Quantity = int.Parse(tbQuantity.Text),
                            PurchasePrice = decimal.Parse(tbPurchasePrice.Text),
                            UserID = (int)cbUser.SelectedValue,
                            ProviderID = (int)cbProvider.SelectedValue
                        };

                        context.Arrivals.Add(newArrival);
                        context.SaveChanges();
                        ArrivalsPage arrivalsPage = new ArrivalsPage();
                        UpdateArrivalQuantity(newArrival.ComponentID, newArrival.Quantity);

                        MessageBox.Show("Поступление успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        arrivalsPage.LoadArrivals();
                        
                    }
                }
                else
                {
                    throw new ArgumentException("Заполните все поля");

                    
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show(" ", ex.Message, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public void UpdateArrivalQuantity(int? componentId, int? quantity)
        {
            try
            {
                using (var db = new ИП_ХевешиEntities())
                {
                    // 1. Находим комплектующее
                    var component = db.Components.FirstOrDefault(c => c.ID == componentId);
                    if (component == null)
                    {
                        MessageBox.Show("Ошибка получения комплектующего, обратитесь к системному администратору", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // 2. Увеличиваем количество
                    component.Quantity += quantity;

                    // 3. Сохраняем изменения
                    db.SaveChanges();


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}

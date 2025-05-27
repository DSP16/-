using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ИП_Хевеши.Classes;
using ИП_Хевеши.Data;
using ИП_Хевеши.UI.Winds;
using ИП_Хевеши.UI.Pages;
using NUnit.Framework;
using Moq;
using System.Windows.Controls;
using System.Data.Entity;
using NUnit.Framework.Legacy;
using System.Linq;
using System.Windows;

namespace InventoryTestsAddComponent
{
    [TestClass]
    public class Tests
    {
        public  ИП_ХевешиEntities _context = new ИП_ХевешиEntities();
        [TestMethod]
        public void AddComponent_WithValidData_AddComponent()
        {
            // Arrange


            var tbMinQuantity = new TextBox { Text = "10" };
            var cbActuality = new ComboBox();
            cbActuality.Items.Add(new ComboBoxItem { Content = "актуален" });
            cbActuality.SelectedIndex = 0;

            var tbName = new TextBox { Text = "TestComponent" };
            var tbPrice = new TextBox { Text = "100" };
            var cbManufacturer = new ComboBox { SelectedValue = 1 };
            var cbZone = new ComboBox { SelectedValue = 1 };
            var tbQuantity = new TextBox { Text = "50" };
            var tbRowCell = new TextBox { Text = "A1" };
            var tbType = new TextBox { Text = "TypeA" };

            // Убедимся, что все элементы инициализированы
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(tbMinQuantity, "tbMinQuantity is null");
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(cbActuality, "cbActuality is null");
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(tbName, "tbName is null");
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(tbPrice, "tbPrice is null");
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(cbManufacturer, "cbManufacturer is null");
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(cbZone, "cbZone is null");
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(tbQuantity, "tbQuantity is null");
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(tbRowCell, "tbRowCell is null");
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(tbType, "tbType is null");

            // Act
            var addComponent = new AddComponentBack();
            addComponent.AddComponent(tbMinQuantity, cbActuality, tbName, tbPrice, cbManufacturer, cbZone, tbQuantity, tbRowCell, tbType);
            // Assert
            
            var addedComponent = _context.Components.Any(c => c.Name == "TestComponent");
           
         
        }
        [TestMethod]
        public void AddComponent_WithoutName_ThrowsArgumentException()
        {
            // Arrange


            var tbMinQuantity = new TextBox { Text = "10" };
            var cbActuality = new ComboBox();
            cbActuality.Items.Add(new ComboBoxItem { Content = "актуален" });
            cbActuality.SelectedIndex = 0;

            var tbName = new TextBox { Text = "" };
            var tbPrice = new TextBox { Text = "100" };
            var cbManufacturer = new ComboBox { SelectedValue = 1 };
            var cbZone = new ComboBox { SelectedValue = 1 };
            var tbQuantity = new TextBox { Text = "50" };
            var tbRowCell = new TextBox { Text = "A1" };
            var tbType = new TextBox { Text = "TypeA" };

            var addComponent = new AddComponentBack();
            // Act & Assert
            var ex = Microsoft.VisualStudio.TestTools.UnitTesting.Assert.ThrowsException<ArgumentException>(() =>
            {
                addComponent.AddComponent(tbMinQuantity, cbActuality, tbName, tbPrice, cbManufacturer, cbZone, tbQuantity, tbRowCell, tbType);
            });
            ClassicAssert.That(ex.Message, Is.EqualTo("Одно или более полей незаполненны"));

        }

        [TestMethod]
        public void AddArrival_WithValidData_AddArrival()
        {
            // Arrange


            var cbComponentID = new ComboBox();
            cbComponentID.Items.Add(new ComboBoxItem { Content = "Intel Core i7" });
            var dpArrivalDate = new DatePicker();
            dpArrivalDate.SelectedDate = DateTime.Today; 
            var tbQuantity = new TextBox { Text = "2"};
            var tbPurcharsePrice = new TextBox { Text = "3"};
            var cbUserID = new ComboBox();
            cbUserID.Items.Add(new ComboBoxItem { Content = 1 });
            var cbProviderID = new ComboBox();
            cbProviderID.Items.Add(new ComboBoxItem { Content = 1});
          

            var addArrival = new AddArrivalBack();
            // Act 
                addArrival.AddArrival(tbQuantity, tbPurcharsePrice, cbComponentID, cbUserID, dpArrivalDate, cbProviderID);
            //Assert
            var addedArrival = _context.Arrivals.Any(a => a.Quantity == 2 && a.UserID == 1);
        }

        


        [TestMethod]
        public void AddArrival_WithoutQuantity_ThrowsArgumentException()
        {
            // Arrange


            var cbComponentID = new ComboBox();
            cbComponentID.Items.Add(new ComboBoxItem { Content = "Intel Core i7" });
            var dpArrivalDate = new DatePicker();
            dpArrivalDate.SelectedDate = DateTime.Today;
            var tbQuantity = new TextBox { Text = "" };
            var tbPurcharsePrice = new TextBox { Text = "3" };
            var cbUserID = new ComboBox();
            cbUserID.Items.Add(new ComboBoxItem { Content = 1 });
            var cbProviderID = new ComboBox();
            cbProviderID.Items.Add(new ComboBoxItem { Content = 1 });


            var addArrival = new AddArrivalBack();
            // Act & Assert
            var ex = Microsoft.VisualStudio.TestTools.UnitTesting.Assert.ThrowsException<NullReferenceException>(() =>
            {
                addArrival.AddArrival(tbQuantity, tbPurcharsePrice, cbComponentID, cbUserID, dpArrivalDate, cbProviderID);
            });
            ClassicAssert.That(ex.Message, Is.EqualTo("Заполните все поля"));




        }

        [TestMethod]
        public void AddProvider_WithValidData_AddProvider()
        {
            // Arrange
            
            var tbName= new TextBox { Text = "ООО 'Система ПБО'" };
            var tbCountry = new TextBox { Text = "Россия" };

            var addProvider = new AddProviderBack();
            // Act 
            addProvider.AddProvider(tbName, tbCountry);

            // Assert
            var addedProvider =  _context.Providers.Any(p => p.Name == "ООО 'Система ПБО'" && p.Country == "Россия");


        }
    }
}

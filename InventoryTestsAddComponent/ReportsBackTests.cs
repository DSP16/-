using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ИП_Хевеши.Classes;
using ИП_Хевеши.Data;
using ИП_Хевеши.UI.Winds;
using ИП_Хевеши.UI.Pages;
using Moq;
using System.Windows.Controls;
using System.Data.Entity;
using NUnit.Framework.Legacy;
using System.Windows;
using ИП_Хевеши.Views;
using System.Threading.Tasks;
using System.IO;


namespace InventoryTestsAddComponent
{
    [TestClass]
    public class ReportsBackTests
    {
        [TestMethod]
        public void ExportAnalyticsToExcel_ShouldCreateFile()
        {
            // Arrange
            var vm = new AnalyticsViewModel();
            string path = Path.Combine(Path.GetTempPath(), "analytics_test.xlsx");

            // Act
            vm.ExportAnalyticsToExcel();

            // Assert
            Assert.IsTrue(File.Exists(path), "Файл не был создан.");
            File.Delete(path); // Очистка
        }
        [TestMethod]
        public void ImportReceiptsFromExcel_ValidFile_ShouldSucceed()
        {
            // Arrange
            ReceiptPage receiptPage = new ReceiptPage();
            string path = @"C:\\TestData\\ValidArrivals.xlsx"; // путь к валидному тест-файлу

            // Act & Assert
            try
            {
                receiptPage.ImportReceiptsFromExcel(path);
                Assert.IsTrue(true); // прошёл без исключений
            }
            catch
            {
                Assert.Fail("Импорт выдал исключение при валидных данных.");
            }
        }
        [TestMethod]
        public void ImportReceiptsFromExcel_InvalidFile_ShouldThrow()
        {
            // Arrange
            ReceiptPage receiptPage = new ReceiptPage();
            string path = @"C:\\TestData\\InvalidArrivals.xlsx"; // файл с ошибками

            // Act & Assert
            Assert.ThrowsException<Exception>(() => receiptPage.ImportReceiptsFromExcel(path));
        }
    }
}

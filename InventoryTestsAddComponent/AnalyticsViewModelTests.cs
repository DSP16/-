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
using LiveCharts.Wpf;
using OxyPlot;

namespace InventoryTestsAddComponent
{
        [TestClass]
        public class AnalyticsViewModelTests
        {
        [TestMethod]
        public void RevenueForecastModel_ShouldContainForecastPoint()
        {
            // Arrange
            var vm = new AnalyticsViewModel();

            // Act
            var series = vm.RevenueForecastModel.Series.OfType<OxyPlot.Series.LineSeries>().FirstOrDefault();

            // Assert
            Assert.IsNotNull(series, "Серия прогноза не найдена.");
            Assert.IsTrue(series.Points.Count > 1, "Недостаточно точек для прогноза.");
        }
    }

}


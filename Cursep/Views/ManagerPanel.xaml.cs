using System.Windows.Controls;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;
using Cursep.Models;
using Cursep.Services;

namespace Cursep.Views
{
    public partial class ManagerPanel : UserControl
    {
        private User _currentUser;
        private DatabaseService _dbService;

        public ManagerPanel(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _dbService = new DatabaseService();

            LoadMaterials();
            LoadSuppliers();
            LoadStatistics();

            // Устанавливаем даты по умолчанию
            dpStartDate.SelectedDate = DateTime.Today.AddDays(-30);
            dpEndDate.SelectedDate = DateTime.Today;
        }

        private void LoadMaterials()
        {
            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT m.*, c.CategoryName, s.SupplierName 
                        FROM Materials m 
                        LEFT JOIN Categories c ON m.CategoryID = c.CategoryID 
                        LEFT JOIN Suppliers s ON m.SupplierID = s.SupplierID
                        ORDER BY m.MaterialName";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var materials = new List<MaterialDisplay>();
                        while (reader.Read())
                        {
                            materials.Add(new MaterialDisplay
                            {
                                MaterialID = (int)reader["MaterialID"],
                                MaterialName = reader["MaterialName"].ToString(),
                                Description = reader["Description"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                SupplierName = reader["SupplierName"].ToString(),
                                UnitPrice = (decimal)reader["UnitPrice"],
                                UnitOfMeasure = reader["UnitOfMeasure"].ToString(),
                                MinStockLevel = (int)reader["MinStockLevel"],
                                MaxStockLevel = (int)reader["MaxStockLevel"]
                            });
                        }
                        dgMaterials.ItemsSource = materials;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки материалов: {ex.Message}");
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM Suppliers ORDER BY SupplierName";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var suppliers = new List<Supplier>();
                        while (reader.Read())
                        {
                            suppliers.Add(new Supplier
                            {
                                SupplierID = (int)reader["SupplierID"],
                                SupplierName = reader["SupplierName"].ToString(),
                                ContactPerson = reader["ContactPerson"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                Email = reader["Email"].ToString(),
                                Address = reader["Address"].ToString()
                            });
                        }
                        dgSuppliers.ItemsSource = suppliers;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}");
            }
        }

        private void LoadStatistics()
        {
            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();

                    // Общее количество материалов
                    string materialsCount = "SELECT COUNT(*) FROM Materials";
                    using (var cmd = new SqlCommand(materialsCount, connection))
                    {
                        txtTotalMaterials.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Общее количество операций
                    string operationsCount = "SELECT COUNT(*) FROM Operations";
                    using (var cmd = new SqlCommand(operationsCount, connection))
                    {
                        txtTotalOperations.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Общая стоимость запасов
                    string totalValue = @"
                        SELECT SUM(s.Quantity * m.UnitPrice) 
                        FROM Stock s 
                        INNER JOIN Materials m ON s.MaterialID = m.MaterialID";
                    using (var cmd = new SqlCommand(totalValue, connection))
                    {
                        var result = cmd.ExecuteScalar();
                        decimal value = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                        txtTotalValue.Text = value.ToString("C");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}");
            }
        }

        private void BtnRefreshMaterials_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadMaterials();
        }

        private void BtnRefreshSuppliers_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadSuppliers();
        }

        private void BtnAddMaterial_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Окно добавления материала
            var addMaterialWindow = new AddMaterialWindow();
            if (addMaterialWindow.ShowDialog() == true)
            {
                LoadMaterials();
                LoadStatistics();
            }
        }

        private void BtnEditMaterial_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dgMaterials.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Выберите материал для редактирования");
                return;
            }

            // Здесь можно реализовать окно редактирования
            System.Windows.MessageBox.Show("Функция редактирования в разработке");
        }

        private void BtnAddSupplier_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Окно добавления поставщика
            var addSupplierWindow = new AddSupplierWindow();
            if (addSupplierWindow.ShowDialog() == true)
            {
                LoadSuppliers();
            }
        }

        private void BtnGenerateReport_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null)
            {
                System.Windows.MessageBox.Show("Выберите период для отчета");
                return;
            }

            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            m.MaterialName,
                            ISNULL(SUM(CASE WHEN o.OperationType = 'INCOME' THEN o.Quantity ELSE 0 END), 0) as Income,
                            ISNULL(SUM(CASE WHEN o.OperationType = 'OUTCOME' THEN o.Quantity ELSE 0 END), 0) as Outcome,
                            s.Quantity as Balance,
                            (s.Quantity * m.UnitPrice) as TotalValue
                        FROM Materials m
                        LEFT JOIN Operations o ON m.MaterialID = o.MaterialID 
                            AND o.OperationDate BETWEEN @StartDate AND @EndDate
                        LEFT JOIN Stock s ON m.MaterialID = s.MaterialID
                        GROUP BY m.MaterialID, m.MaterialName, s.Quantity, m.UnitPrice
                        ORDER BY m.MaterialName";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StartDate", dpStartDate.SelectedDate.Value);
                        command.Parameters.AddWithValue("@EndDate", dpEndDate.SelectedDate.Value.AddDays(1));

                        using (var reader = command.ExecuteReader())
                        {
                            var reportData = new List<ReportDisplay>();
                            while (reader.Read())
                            {
                                reportData.Add(new ReportDisplay
                                {
                                    MaterialName = reader["MaterialName"].ToString(),
                                    Income = Convert.ToInt32(reader["Income"]),
                                    Outcome = Convert.ToInt32(reader["Outcome"]),
                                    Balance = Convert.ToInt32(reader["Balance"]),
                                    TotalValue = Convert.ToDecimal(reader["TotalValue"])
                                });
                            }
                            dgReport.ItemsSource = reportData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка формирования отчета: {ex.Message}");
            }
        }
    }
}
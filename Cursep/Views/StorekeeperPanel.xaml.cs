using System.Windows.Controls;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;
using Cursep.Models;
using Cursep.Services;

namespace Cursep.Views
{
    public partial class StorekeeperPanel : UserControl
    {
        private User _currentUser;
        private DatabaseService _dbService;
        private List<Material> _materials;

        public StorekeeperPanel(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _dbService = new DatabaseService();
            _materials = new List<Material>();

            LoadMaterials();
            LoadStock();
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
                        LEFT JOIN Suppliers s ON m.SupplierID = s.SupplierID";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        _materials.Clear();
                        while (reader.Read())
                        {
                            _materials.Add(new Material
                            {
                                MaterialID = (int)reader["MaterialID"],
                                MaterialName = reader["MaterialName"].ToString(),
                                Description = reader["Description"].ToString(),
                                CategoryID = (int)reader["CategoryID"],
                                SupplierID = (int)reader["SupplierID"],
                                UnitPrice = (decimal)reader["UnitPrice"],
                                UnitOfMeasure = reader["UnitOfMeasure"].ToString(),
                                MinStockLevel = (int)reader["MinStockLevel"],
                                MaxStockLevel = reader["MaxStockLevel"] as int? ?? 0
                            });
                        }
                    }
                }

                cmbMaterials.ItemsSource = _materials;
                cmbMaterialsOutcome.ItemsSource = _materials;
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки материалов: {ex.Message}");
            }
        }

        private void LoadStock()
        {
            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT m.MaterialName, s.Quantity, s.Location, c.CategoryName, m.UnitOfMeasure, m.UnitPrice
                        FROM Stock s
                        INNER JOIN Materials m ON s.MaterialID = m.MaterialID
                        INNER JOIN Categories c ON m.CategoryID = c.CategoryID
                        ORDER BY c.CategoryName, m.MaterialName";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var stockList = new List<StockDisplay>();
                        while (reader.Read())
                        {
                            stockList.Add(new StockDisplay
                            {
                                MaterialName = reader["MaterialName"].ToString(),
                                Quantity = (int)reader["Quantity"],
                                Location = reader["Location"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                UnitOfMeasure = reader["UnitOfMeasure"].ToString(),
                                UnitPrice = (decimal)reader["UnitPrice"]
                            });
                        }
                        dgAllStock.ItemsSource = stockList;
                        dgStock.ItemsSource = stockList;
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки остатков: {ex.Message}");
            }
        }

        // Остальные методы остаются без изменений
        private void BtnIncome_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (cmbMaterials.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Выберите материал");
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                System.Windows.MessageBox.Show("Введите корректное количество");
                return;
            }

            var selectedMaterial = cmbMaterials.SelectedItem as Material;
            string reference = txtReferenceNumber.Text.Trim();
            string notes = txtNotes.Text.Trim();

            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();

                    // Добавляем операцию прихода
                    string query = @"
                        INSERT INTO Operations (MaterialID, UserID, OperationType, Quantity, ReferenceNumber, Notes)
                        VALUES (@MaterialID, @UserID, 'INCOME', @Quantity, @ReferenceNumber, @Notes)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MaterialID", selectedMaterial.MaterialID);
                        command.Parameters.AddWithValue("@UserID", _currentUser.UserID);
                        command.Parameters.AddWithValue("@Quantity", quantity);
                        command.Parameters.AddWithValue("@ReferenceNumber", reference);
                        command.Parameters.AddWithValue("@Notes", notes);

                        command.ExecuteNonQuery();
                    }

                    // Обновляем остатки на складе
                    string updateStock = @"
                        UPDATE Stock SET Quantity = Quantity + @Quantity 
                        WHERE MaterialID = @MaterialID";

                    using (var command = new SqlCommand(updateStock, connection))
                    {
                        command.Parameters.AddWithValue("@Quantity", quantity);
                        command.Parameters.AddWithValue("@MaterialID", selectedMaterial.MaterialID);
                        command.ExecuteNonQuery();
                    }
                }

                System.Windows.MessageBox.Show("Приход материалов успешно оформлен!");
                ClearIncomeForm();
                LoadStock();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка оформления прихода: {ex.Message}");
            }
        }

        private void BtnOutcome_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (cmbMaterialsOutcome.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Выберите материал");
                return;
            }

            if (!int.TryParse(txtQuantityOutcome.Text, out int quantity) || quantity <= 0)
            {
                System.Windows.MessageBox.Show("Введите корректное количество");
                return;
            }

            var selectedMaterial = cmbMaterialsOutcome.SelectedItem as Material;
            string reference = txtReferenceOutcome.Text.Trim();
            string notes = txtNotesOutcome.Text.Trim();

            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();

                    // Проверяем достаточно ли материалов на складе
                    string checkStock = "SELECT Quantity FROM Stock WHERE MaterialID = @MaterialID";
                    int currentStock = 0;

                    using (var command = new SqlCommand(checkStock, connection))
                    {
                        command.Parameters.AddWithValue("@MaterialID", selectedMaterial.MaterialID);
                        currentStock = (int)command.ExecuteScalar();
                    }

                    if (currentStock < quantity)
                    {
                        System.Windows.MessageBox.Show($"Недостаточно материалов на складе. Доступно: {currentStock}");
                        return;
                    }

                    // Добавляем операцию расхода
                    string query = @"
                        INSERT INTO Operations (MaterialID, UserID, OperationType, Quantity, ReferenceNumber, Notes)
                        VALUES (@MaterialID, @UserID, 'OUTCOME', @Quantity, @ReferenceNumber, @Notes)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MaterialID", selectedMaterial.MaterialID);
                        command.Parameters.AddWithValue("@UserID", _currentUser.UserID);
                        command.Parameters.AddWithValue("@Quantity", quantity);
                        command.Parameters.AddWithValue("@ReferenceNumber", reference);
                        command.Parameters.AddWithValue("@Notes", notes);

                        command.ExecuteNonQuery();
                    }

                    // Обновляем остатки на складе
                    string updateStock = @"
                        UPDATE Stock SET Quantity = Quantity - @Quantity 
                        WHERE MaterialID = @MaterialID";

                    using (var command = new SqlCommand(updateStock, connection))
                    {
                        command.Parameters.AddWithValue("@Quantity", quantity);
                        command.Parameters.AddWithValue("@MaterialID", selectedMaterial.MaterialID);
                        command.ExecuteNonQuery();
                    }
                }

                System.Windows.MessageBox.Show("Расход материалов успешно оформлен!");
                ClearOutcomeForm();
                LoadStock();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка оформления расхода: {ex.Message}");
            }
        }

        private void ClearIncomeForm()
        {
            txtQuantity.Clear();
            txtReferenceNumber.Clear();
            txtNotes.Clear();
        }

        private void ClearOutcomeForm()
        {
            txtQuantityOutcome.Clear();
            txtReferenceOutcome.Clear();
            txtNotesOutcome.Clear();
        }
    }
}
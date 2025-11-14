using System.Windows;
using System.Data.SqlClient;
using System.Collections.Generic;
using Cursep.Models;
using Cursep.Services;

namespace Cursep.Views
{
    public partial class AddMaterialWindow : Window
    {
        private DatabaseService _dbService;
        private List<Category> _categories;
        private List<Supplier> _suppliers;

        public AddMaterialWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _categories = new List<Category>();
            _suppliers = new List<Supplier>();

            LoadCategories();
            LoadSuppliers();
        }

        private void LoadCategories()
        {
            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM Categories ORDER BY CategoryName";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _categories.Add(new Category
                            {
                                CategoryID = (int)reader["CategoryID"],
                                CategoryName = reader["CategoryName"].ToString(),
                                Description = reader["Description"].ToString()
                            });
                        }
                    }
                }

                cmbCategories.ItemsSource = _categories;
                if (_categories.Count > 0)
                    cmbCategories.SelectedIndex = 0;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
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
                        while (reader.Read())
                        {
                            _suppliers.Add(new Supplier
                            {
                                SupplierID = (int)reader["SupplierID"],
                                SupplierName = reader["SupplierName"].ToString(),
                                ContactPerson = reader["ContactPerson"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                Email = reader["Email"].ToString(),
                                Address = reader["Address"].ToString()
                            });
                        }
                    }
                }

                cmbSuppliers.ItemsSource = _suppliers;
                if (_suppliers.Count > 0)
                    cmbSuppliers.SelectedIndex = 0;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();

                    // Добавляем материал
                    string query = @"
                        INSERT INTO Materials (MaterialName, Description, CategoryID, SupplierID, 
                                              UnitPrice, UnitOfMeasure, MinStockLevel, MaxStockLevel)
                        VALUES (@MaterialName, @Description, @CategoryID, @SupplierID, 
                                @UnitPrice, @UnitOfMeasure, @MinStockLevel, @MaxStockLevel);
                        
                        -- Создаем запись на складе
                        INSERT INTO Stock (MaterialID, Quantity, Location)
                        VALUES (SCOPE_IDENTITY(), 0, 'Новое поступление');";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MaterialName", txtMaterialName.Text.Trim());
                        command.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                        command.Parameters.AddWithValue("@CategoryID", ((Category)cmbCategories.SelectedItem).CategoryID);
                        command.Parameters.AddWithValue("@SupplierID", ((Supplier)cmbSuppliers.SelectedItem).SupplierID);
                        command.Parameters.AddWithValue("@UnitPrice", decimal.Parse(txtUnitPrice.Text));
                        command.Parameters.AddWithValue("@UnitOfMeasure", txtUnitOfMeasure.Text.Trim());
                        command.Parameters.AddWithValue("@MinStockLevel", int.Parse(txtMinStockLevel.Text));
                        command.Parameters.AddWithValue("@MaxStockLevel", int.Parse(txtMaxStockLevel.Text));

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Материал успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtMaterialName.Text))
            {
                ShowError("Введите название материала");
                return false;
            }

            if (cmbCategories.SelectedItem == null)
            {
                ShowError("Выберите категорию");
                return false;
            }

            if (cmbSuppliers.SelectedItem == null)
            {
                ShowError("Выберите поставщика");
                return false;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal price) || price < 0)
            {
                ShowError("Введите корректную цену");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUnitOfMeasure.Text))
            {
                ShowError("Введите единицу измерения");
                return false;
            }

            if (!int.TryParse(txtMinStockLevel.Text, out int minStock) || minStock < 0)
            {
                ShowError("Введите корректное минимальное количество");
                return false;
            }

            if (!int.TryParse(txtMaxStockLevel.Text, out int maxStock) || maxStock < 0)
            {
                ShowError("Введите корректное максимальное количество");
                return false;
            }

            HideError();
            return true;
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            txtError.Visibility = Visibility.Collapsed;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
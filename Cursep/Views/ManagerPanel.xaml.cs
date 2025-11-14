using System.Windows.Controls;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;
using Cursep.Models;
using Cursep.Services;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Cursep.Views
{
    public partial class ManagerPanel : UserControl
    {
        private User _currentUser;
        private DatabaseService _dbService;

        // Модальные панели
        private Border _modalOverlay;
        private Border _materialModalBorder;
        private Border _supplierModalBorder;

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

            // Инициализируем модальные панели
            InitializeModalPanels();
        }

        private void InitializeModalPanels()
        {
            // Создаем оверлей для модальных окон
            _modalOverlay = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Visibility = Visibility.Collapsed
            };

            // Обработчик клика по оверлею для закрытия
            _modalOverlay.MouseDown += (s, e) => {
                HideModalPanels();
            };

            // Панель редактирования материала
            _materialModalBorder = CreateMaterialModalPanel();

            // Панель редактирования поставщика
            _supplierModalBorder = CreateSupplierModalPanel();

            // Добавляем оверлей и панели в основной Grid
            var mainGrid = Content as Grid;
            if (mainGrid != null)
            {
                mainGrid.Children.Add(_modalOverlay);
                mainGrid.Children.Add(_materialModalBorder);
                mainGrid.Children.Add(_supplierModalBorder);
            }
        }

        private Border CreateMaterialModalPanel()
        {
            var stackPanel = new StackPanel
            {
                Background = Brushes.White,
                Width = 400,
                MaxHeight = 500,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var scrollViewer = new ScrollViewer
            {
                Content = stackPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Background = Brushes.White,
                Child = scrollViewer,
                Visibility = Visibility.Collapsed,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            return border;
        }

        private Border CreateSupplierModalPanel()
        {
            var stackPanel = new StackPanel
            {
                Background = Brushes.White,
                Width = 400,
                MaxHeight = 500,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var scrollViewer = new ScrollViewer
            {
                Content = stackPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Background = Brushes.White,
                Child = scrollViewer,
                Visibility = Visibility.Collapsed,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            return border;
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
                MessageBox.Show($"Ошибка загрузки материалов: {ex.Message}");
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
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}");
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
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}");
            }
        }

        private void BtnAddMaterial_Click(object sender, RoutedEventArgs e)
        {
            ShowMaterialEditPanel(null);
        }

        private void BtnEditMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterials.SelectedItem == null)
            {
                MessageBox.Show("Выберите материал для редактирования");
                return;
            }

            var selectedMaterial = dgMaterials.SelectedItem as MaterialDisplay;
            if (selectedMaterial != null)
            {
                try
                {
                    using (var connection = _dbService.GetConnection())
                    {
                        connection.Open();
                        string query = @"
                            SELECT m.*
                            FROM Materials m 
                            WHERE m.MaterialID = @MaterialID";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@MaterialID", selectedMaterial.MaterialID);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var material = new Material
                                    {
                                        MaterialID = (int)reader["MaterialID"],
                                        MaterialName = reader["MaterialName"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        CategoryID = reader["CategoryID"] != DBNull.Value ? (int)reader["CategoryID"] : 0,
                                        SupplierID = reader["SupplierID"] != DBNull.Value ? (int)reader["SupplierID"] : 0,
                                        UnitPrice = (decimal)reader["UnitPrice"],
                                        UnitOfMeasure = reader["UnitOfMeasure"].ToString(),
                                        MinStockLevel = (int)reader["MinStockLevel"],
                                        MaxStockLevel = (int)reader["MaxStockLevel"]
                                    };

                                    ShowMaterialEditPanel(material);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при редактировании материала: {ex.Message}");
                }
            }
        }

        private void ShowMaterialEditPanel(Material material)
        {
            var scrollViewer = _materialModalBorder.Child as ScrollViewer;
            var stackPanel = scrollViewer?.Content as StackPanel;
            if (stackPanel == null) return;

            stackPanel.Children.Clear();

            bool isEdit = material != null;
            var titleText = isEdit ? "Редактирование материала" : "Добавление материала";

            // Заголовок с кнопкой закрытия
            var titlePanel = new StackPanel { Orientation = Orientation.Horizontal };
            var title = new TextBlock
            {
                Text = titleText,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var btnClose = new Button
            {
                Content = "×",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                Padding = new Thickness(5, 0, 5, 2),
                Width = 25,
                Height = 25
            };
            btnClose.Click += (s, e) => HideModalPanels();

            titlePanel.Children.Add(title);
            titlePanel.Children.Add(btnClose);
            stackPanel.Children.Add(titlePanel);
            stackPanel.Children.Add(new Separator { Margin = new Thickness(0, 10, 0, 10) });

            // Поля формы
            var txtName = new TextBox
            {
                Text = material?.MaterialName ?? "",
                Margin = new Thickness(0, 0, 0, 10)
            };
            var txtDescription = new TextBox
            {
                Text = material?.Description ?? "",
                Margin = new Thickness(0, 0, 0, 10),
                Height = 60,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true
            };
            var txtPrice = new TextBox
            {
                Text = material?.UnitPrice.ToString() ?? "0",
                Margin = new Thickness(0, 0, 0, 10)
            };
            var txtUnit = new TextBox
            {
                Text = material?.UnitOfMeasure ?? "",
                Margin = new Thickness(0, 0, 0, 10)
            };
            var txtMinStock = new TextBox
            {
                Text = material?.MinStockLevel.ToString() ?? "0",
                Margin = new Thickness(0, 0, 0, 10)
            };
            var txtMaxStock = new TextBox
            {
                Text = material?.MaxStockLevel.ToString() ?? "0",
                Margin = new Thickness(0, 0, 0, 20)
            };

            stackPanel.Children.Add(new TextBlock { Text = "Название:*", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(txtName);
            stackPanel.Children.Add(new TextBlock { Text = "Описание:" });
            stackPanel.Children.Add(txtDescription);
            stackPanel.Children.Add(new TextBlock { Text = "Цена:*", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(txtPrice);
            stackPanel.Children.Add(new TextBlock { Text = "Единица измерения:*", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(txtUnit);
            stackPanel.Children.Add(new TextBlock { Text = "Мин. запас:*", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(txtMinStock);
            stackPanel.Children.Add(new TextBlock { Text = "Макс. запас:*", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(txtMaxStock);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            var btnSave = new Button
            {
                Content = "Сохранить",
                Margin = new Thickness(0, 0, 10, 0),
                Background = Brushes.Green,
                Foreground = Brushes.White,
                Padding = new Thickness(15, 5, 15, 5),
                MinWidth = 80
            };
            var btnCancel = new Button
            {
                Content = "Отмена",
                Background = Brushes.Red,
                Foreground = Brushes.White,
                Padding = new Thickness(15, 5, 15, 5),
                MinWidth = 80
            };

            btnSave.Click += (s, e) =>
            {
                SaveMaterial(material, txtName.Text, txtDescription.Text, txtPrice.Text, txtUnit.Text, txtMinStock.Text, txtMaxStock.Text);
                HideModalPanels();
            };

            btnCancel.Click += (s, e) => HideModalPanels();

            buttonPanel.Children.Add(btnSave);
            buttonPanel.Children.Add(btnCancel);
            stackPanel.Children.Add(buttonPanel);

            // Показываем панель
            ShowModalPanel(_materialModalBorder);
        }

        private void SaveMaterial(Material material, string name, string description, string price, string unit, string minStock, string maxStock)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Введите название материала");
                    return;
                }
                if (string.IsNullOrWhiteSpace(unit))
                {
                    MessageBox.Show("Введите единицу измерения");
                    return;
                }
                if (!decimal.TryParse(price, out decimal priceValue) || priceValue < 0)
                {
                    MessageBox.Show("Введите корректную цену");
                    return;
                }
                if (!int.TryParse(minStock, out int minStockValue) || minStockValue < 0)
                {
                    MessageBox.Show("Введите корректное значение минимального запаса");
                    return;
                }
                if (!int.TryParse(maxStock, out int maxStockValue) || maxStockValue < 0)
                {
                    MessageBox.Show("Введите корректное значение максимального запаса");
                    return;
                }

                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();

                    if (material == null)
                    {
                        // Добавление нового материала
                        string query = @"
                            INSERT INTO Materials (MaterialName, Description, CategoryID, SupplierID, UnitPrice, UnitOfMeasure, MinStockLevel, MaxStockLevel)
                            VALUES (@MaterialName, @Description, @CategoryID, @SupplierID, @UnitPrice, @UnitOfMeasure, @MinStockLevel, @MaxStockLevel)";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@MaterialName", name);
                            command.Parameters.AddWithValue("@Description", description);
                            command.Parameters.AddWithValue("@CategoryID", DBNull.Value);
                            command.Parameters.AddWithValue("@SupplierID", DBNull.Value);
                            command.Parameters.AddWithValue("@UnitPrice", priceValue);
                            command.Parameters.AddWithValue("@UnitOfMeasure", unit);
                            command.Parameters.AddWithValue("@MinStockLevel", minStockValue);
                            command.Parameters.AddWithValue("@MaxStockLevel", maxStockValue);

                            command.ExecuteNonQuery();
                            MessageBox.Show("Материал успешно добавлен");
                        }
                    }
                    else
                    {
                        // Редактирование существующего материала
                        string query = @"
                            UPDATE Materials 
                            SET MaterialName = @MaterialName,
                                Description = @Description,
                                UnitPrice = @UnitPrice,
                                UnitOfMeasure = @UnitOfMeasure,
                                MinStockLevel = @MinStockLevel,
                                MaxStockLevel = @MaxStockLevel
                            WHERE MaterialID = @MaterialID";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@MaterialID", material.MaterialID);
                            command.Parameters.AddWithValue("@MaterialName", name);
                            command.Parameters.AddWithValue("@Description", description);
                            command.Parameters.AddWithValue("@UnitPrice", priceValue);
                            command.Parameters.AddWithValue("@UnitOfMeasure", unit);
                            command.Parameters.AddWithValue("@MinStockLevel", minStockValue);
                            command.Parameters.AddWithValue("@MaxStockLevel", maxStockValue);

                            command.ExecuteNonQuery();
                            MessageBox.Show("Материал успешно обновлен");
                        }
                    }

                    LoadMaterials();
                    LoadStatistics();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения материала: {ex.Message}");
            }
        }

        private void BtnDeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            // Код удаления материала без изменений
            if (dgMaterials.SelectedItem == null)
            {
                MessageBox.Show("Выберите материал для удаления");
                return;
            }

            var selectedMaterial = dgMaterials.SelectedItem as MaterialDisplay;
            if (selectedMaterial != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить материал '{selectedMaterial.MaterialName}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var connection = _dbService.GetConnection())
                        {
                            connection.Open();

                            // Проверяем, есть ли связанные операции
                            string checkOperationsQuery = "SELECT COUNT(*) FROM Operations WHERE MaterialID = @MaterialID";
                            using (var checkCmd = new SqlCommand(checkOperationsQuery, connection))
                            {
                                checkCmd.Parameters.AddWithValue("@MaterialID", selectedMaterial.MaterialID);
                                int operationCount = (int)checkCmd.ExecuteScalar();

                                if (operationCount > 0)
                                {
                                    MessageBox.Show("Невозможно удалить материал, так как с ним связаны операции. Сначала удалите все связанные операции.");
                                    return;
                                }
                            }

                            // Удаляем материал
                            string deleteQuery = "DELETE FROM Materials WHERE MaterialID = @MaterialID";
                            using (var command = new SqlCommand(deleteQuery, connection))
                            {
                                command.Parameters.AddWithValue("@MaterialID", selectedMaterial.MaterialID);
                                int rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Материал успешно удален");
                                    LoadMaterials();
                                    LoadStatistics();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении материала: {ex.Message}");
                    }
                }
            }
        }

        private void BtnAddSupplier_Click(object sender, RoutedEventArgs e)
        {
            ShowSupplierEditPanel(null);
        }

        private void BtnEditSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (dgSuppliers.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика для редактирования");
                return;
            }

            var selectedSupplier = dgSuppliers.SelectedItem as Supplier;
            ShowSupplierEditPanel(selectedSupplier);
        }

        private void ShowSupplierEditPanel(Supplier supplier)
        {
            var scrollViewer = _supplierModalBorder.Child as ScrollViewer;
            var stackPanel = scrollViewer?.Content as StackPanel;
            if (stackPanel == null) return;

            stackPanel.Children.Clear();

            bool isEdit = supplier != null;
            var titleText = isEdit ? "Редактирование поставщика" : "Добавление поставщика";

            // Заголовок с кнопкой закрытия
            var titlePanel = new StackPanel { Orientation = Orientation.Horizontal };
            var title = new TextBlock
            {
                Text = titleText,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };

            var btnClose = new Button
            {
                Content = "×",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                Padding = new Thickness(5, 0, 5, 2),
                Width = 25,
                Height = 25
            };
            btnClose.Click += (s, e) => HideModalPanels();

            titlePanel.Children.Add(title);
            titlePanel.Children.Add(btnClose);
            stackPanel.Children.Add(titlePanel);
            stackPanel.Children.Add(new Separator { Margin = new Thickness(0, 10, 0, 10) });

            // Поля формы
            var txtName = new TextBox { Text = supplier?.SupplierName ?? "", Margin = new Thickness(0, 0, 0, 10) };
            var txtContact = new TextBox { Text = supplier?.ContactPerson ?? "", Margin = new Thickness(0, 0, 0, 10) };
            var txtPhone = new TextBox { Text = supplier?.Phone ?? "", Margin = new Thickness(0, 0, 0, 10) };
            var txtEmail = new TextBox { Text = supplier?.Email ?? "", Margin = new Thickness(0, 0, 0, 10) };
            var txtAddress = new TextBox
            {
                Text = supplier?.Address ?? "",
                Margin = new Thickness(0, 0, 0, 20),
                Height = 60,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true
            };

            stackPanel.Children.Add(new TextBlock { Text = "Название:*", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(txtName);
            stackPanel.Children.Add(new TextBlock { Text = "Контактное лицо:" });
            stackPanel.Children.Add(txtContact);
            stackPanel.Children.Add(new TextBlock { Text = "Телефон:" });
            stackPanel.Children.Add(txtPhone);
            stackPanel.Children.Add(new TextBlock { Text = "Email:" });
            stackPanel.Children.Add(txtEmail);
            stackPanel.Children.Add(new TextBlock { Text = "Адрес:" });
            stackPanel.Children.Add(txtAddress);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            var btnSave = new Button
            {
                Content = "Сохранить",
                Margin = new Thickness(0, 0, 10, 0),
                Background = Brushes.Green,
                Foreground = Brushes.White,
                Padding = new Thickness(15, 5, 15, 5),
                MinWidth = 80
            };
            var btnCancel = new Button
            {
                Content = "Отмена",
                Background = Brushes.Red,
                Foreground = Brushes.White,
                Padding = new Thickness(15, 5, 15, 5),
                MinWidth = 80
            };

            btnSave.Click += (s, e) =>
            {
                SaveSupplier(supplier, txtName.Text, txtContact.Text, txtPhone.Text, txtEmail.Text, txtAddress.Text);
                HideModalPanels();
            };

            btnCancel.Click += (s, e) => HideModalPanels();

            buttonPanel.Children.Add(btnSave);
            buttonPanel.Children.Add(btnCancel);
            stackPanel.Children.Add(buttonPanel);

            // Показываем панель
            ShowModalPanel(_supplierModalBorder);
        }

        private void SaveSupplier(Supplier supplier, string name, string contact, string phone, string email, string address)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Введите название поставщика");
                    return;
                }

                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();

                    if (supplier == null)
                    {
                        // Добавление нового поставщика
                        string query = @"
                            INSERT INTO Suppliers (SupplierName, ContactPerson, Phone, Email, Address)
                            VALUES (@SupplierName, @ContactPerson, @Phone, @Email, @Address)";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@SupplierName", name);
                            command.Parameters.AddWithValue("@ContactPerson", contact);
                            command.Parameters.AddWithValue("@Phone", phone);
                            command.Parameters.AddWithValue("@Email", email);
                            command.Parameters.AddWithValue("@Address", address);

                            command.ExecuteNonQuery();
                            MessageBox.Show("Поставщик успешно добавлен");
                        }
                    }
                    else
                    {
                        // Редактирование существующего поставщика
                        string query = @"
                            UPDATE Suppliers 
                            SET SupplierName = @SupplierName,
                                ContactPerson = @ContactPerson,
                                Phone = @Phone,
                                Email = @Email,
                                Address = @Address
                            WHERE SupplierID = @SupplierID";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@SupplierID", supplier.SupplierID);
                            command.Parameters.AddWithValue("@SupplierName", name);
                            command.Parameters.AddWithValue("@ContactPerson", contact);
                            command.Parameters.AddWithValue("@Phone", phone);
                            command.Parameters.AddWithValue("@Email", email);
                            command.Parameters.AddWithValue("@Address", address);

                            command.ExecuteNonQuery();
                            MessageBox.Show("Поставщик успешно обновлен");
                        }
                    }

                    LoadSuppliers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения поставщика: {ex.Message}");
            }
        }

        private void BtnDeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            // Код удаления поставщика без изменений
            if (dgSuppliers.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика для удаления");
                return;
            }

            var selectedSupplier = dgSuppliers.SelectedItem as Supplier;
            if (selectedSupplier != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить поставщика '{selectedSupplier.SupplierName}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var connection = _dbService.GetConnection())
                        {
                            connection.Open();

                            // Проверяем, есть ли связанные материалы
                            string checkMaterialsQuery = "SELECT COUNT(*) FROM Materials WHERE SupplierID = @SupplierID";
                            using (var checkCmd = new SqlCommand(checkMaterialsQuery, connection))
                            {
                                checkCmd.Parameters.AddWithValue("@SupplierID", selectedSupplier.SupplierID);
                                int materialsCount = (int)checkCmd.ExecuteScalar();

                                if (materialsCount > 0)
                                {
                                    MessageBox.Show("Невозможно удалить поставщика, так как с ним связаны материалы. Сначала удалите или измените поставщика у связанных материалов.");
                                    return;
                                }
                            }

                            // Удаляем поставщика
                            string deleteQuery = "DELETE FROM Suppliers WHERE SupplierID = @SupplierID";
                            using (var command = new SqlCommand(deleteQuery, connection))
                            {
                                command.Parameters.AddWithValue("@SupplierID", selectedSupplier.SupplierID);
                                int rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Поставщик успешно удален");
                                    LoadSuppliers();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении поставщика: {ex.Message}");
                    }
                }
            }
        }

        private void ShowModalPanel(Border panelBorder)
        {
            if (panelBorder == null) return;

            // Показываем оверлей
            _modalOverlay.Visibility = Visibility.Visible;

            // Показываем панель
            panelBorder.Visibility = Visibility.Visible;
        }

        private void HideModalPanels()
        {
            // Скрываем оверлей
            _modalOverlay.Visibility = Visibility.Collapsed;

            // Скрываем все модальные панели
            _materialModalBorder.Visibility = Visibility.Collapsed;
            _supplierModalBorder.Visibility = Visibility.Collapsed;
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            // Код генерации отчета без изменений
            if (dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите период для отчета");
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
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}");
            }
        }

        // Вспомогательные классы для отображения данных
        public class MaterialDisplay
        {
            public int MaterialID { get; set; }
            public string MaterialName { get; set; }
            public string Description { get; set; }
            public string CategoryName { get; set; }
            public string SupplierName { get; set; }
            public decimal UnitPrice { get; set; }
            public string UnitOfMeasure { get; set; }
            public int MinStockLevel { get; set; }
            public int MaxStockLevel { get; set; }
        }

        public class ReportDisplay
        {
            public string MaterialName { get; set; }
            public int Income { get; set; }
            public int Outcome { get; set; }
            public int Balance { get; set; }
            public decimal TotalValue { get; set; }
        }
    }
}
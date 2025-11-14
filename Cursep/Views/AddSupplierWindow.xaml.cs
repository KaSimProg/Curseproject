using System.Windows;
using System.Data.SqlClient;
using Cursep.Services;

namespace Cursep.Views
{
    public partial class AddSupplierWindow : Window
    {
        private DatabaseService _dbService;

        public AddSupplierWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
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
                    string query = @"
                        INSERT INTO Suppliers (SupplierName, ContactPerson, Phone, Email, Address)
                        VALUES (@SupplierName, @ContactPerson, @Phone, @Email, @Address)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SupplierName", txtSupplierName.Text.Trim());
                        command.Parameters.AddWithValue("@ContactPerson", txtContactPerson.Text.Trim());
                        command.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                        command.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        command.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Поставщик успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (string.IsNullOrWhiteSpace(txtSupplierName.Text))
            {
                ShowError("Введите название компании");
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
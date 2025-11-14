using System.Windows;
using Cursep.Services;
using Cursep.Models;
using System.Diagnostics;
using System;

namespace Cursep.Views
{
    public partial class LoginWindow : Window
    {
        private AuthService _authService;

        public LoginWindow()
        {
            InitializeComponent();
            _authService = new AuthService();

            Debug.WriteLine($"=== LoginWindow создан. ID: {this.GetHashCode()} ===");
            Debug.WriteLine($"Всего окон: {Application.Current.Windows.Count}");

            // Выведем список всех окон
            foreach (Window window in Application.Current.Windows)
            {
                Debug.WriteLine($"Окно: {window.GetType().Name}, ID: {window.GetHashCode()}");
            }

            // УБИРАЕМ эту строку - обработчик уже установлен в XAML
            // btnLogin.Click += BtnLogin_Click;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }

            Debug.WriteLine($"=== Попытка входа: {username} ===");

            User user = _authService.Login(username, password);

            if (user != null)
            {
                Debug.WriteLine($"=== Вход успешен. Создаем MainWindow ===");

                // СОЗДАЕМ ТОЛЬКО ОДИН MainWindow!
                MainWindow mainWindow = new MainWindow(user);
                mainWindow.Show();

                Debug.WriteLine($"=== Закрываем LoginWindow ===");
                this.Close();
            }
            else
            {
                ShowError("Неверный логин или пароль");
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Owner = this;
            registerWindow.ShowDialog();
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Debug.WriteLine($"=== LoginWindow закрыт. ID: {this.GetHashCode()} ===");
        }
    }
}
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dip
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=LAPTOP-V0AGQKUF\\SLAUUUIK;Database=GetDiplom;Trusted_Connection=True;";

        public MainWindow()
        {
            InitializeComponent();

            MouseDown += Window_MouseDown;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginTextBox.Text;
            string password = PasswordBox.Password;

            var button = (Button)sender;
            var scaleTransform = new ScaleTransform(1, 1);
            button.RenderTransform = scaleTransform;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShakeAnimation();
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (username.Equals("admin") && password.Equals("admin"))
            {
                Admin admin = new Admin();
                admin.Show();
                this.Close();
            }
            else if (ValidateUser(username, password))
            {
                Window1 secondPage = new Window1();
                secondPage.Show();
                this.Close();
            }
            else
            {
                ShakeAnimation();
                MessageBox.Show("Неверный логин или пароль.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShakeAnimation()
        {
            var transform = new TranslateTransform();
            this.RenderTransform = transform;

            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 10,
                Duration = TimeSpan.FromMilliseconds(50),
                AutoReverse = true,
                RepeatBehavior = new System.Windows.Media.Animation.RepeatBehavior(3)
            };

            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private bool ValidateUser(string username, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Users WHERE Login = @username AND Password = @password";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при подключении к базе данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
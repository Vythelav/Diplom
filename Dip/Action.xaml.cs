using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Dip
{
    public partial class Action : Window
    {
        private string connectionString = "Server=LAPTOP-V0AGQKUF\\SLAUUUIK;Database=GetDiplom;Trusted_Connection=True;";

        public Action()
        {
            InitializeComponent();
            LoadUserActions();
        }

        private void LoadUserActions()
        {
            UpdateStatus("Загрузка данных...");
            try
            {
                DataTable dataTable = new DataTable();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                                    ua.Id, 
                                    u.Login AS Username,
                                    ua.Action, 
                                    ua.ActionDate, 
                                    ua.Details 
                                  FROM UserActions ua
                                  LEFT JOIN Users u ON ua.UserId = u.Id
                                  ORDER BY ua.ActionDate DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dataTable);
                    }
                }

                UserActionsDataGrid.ItemsSource = dataTable.DefaultView;
                UpdateStatus($"Загружено {dataTable.Rows.Count} записей");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка: {ex.Message}");
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadUserActions();
                return;
            }

            UpdateStatus("Поиск данных...");
            try
            {
                DataTable dataTable = new DataTable();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                                    ua.Id, 
                                    u.Login AS Username,
                                    ua.Action, 
                                    ua.ActionDate, 
                                    ua.Details 
                                  FROM UserActions ua
                                  LEFT JOIN Users u ON ua.UserId = u.Id
                                  WHERE u.Login LIKE @search OR ua.Action LIKE @search OR ua.Details LIKE @search
                                  ORDER BY ua.ActionDate DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@search", $"%{searchText}%");
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dataTable);
                    }
                }

                UserActionsDataGrid.ItemsSource = dataTable.DefaultView;
                UpdateStatus($"Найдено {dataTable.Rows.Count} записей");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка поиска: {ex.Message}");
                MessageBox.Show($"Ошибка при поиске данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            LoadUserActions();
        }

        private void ExportToCsv_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Экспорт данных...");
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv",
                    DefaultExt = ".csv",
                    FileName = $"user_actions_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var dataView = UserActionsDataGrid.ItemsSource as DataView;
                    if (dataView != null && dataView.Count > 0)
                    {
                        StringBuilder csvContent = new StringBuilder();

                        var headers = UserActionsDataGrid.Columns
                            .Select(c => c.Header.ToString());
                        csvContent.AppendLine(string.Join(";", headers));

                        foreach (DataRowView row in dataView)
                        {
                            var values = UserActionsDataGrid.Columns
                                .Cast<DataGridTextColumn>()
                                .Select(col =>
                                {
                                    var val = row[col.SortMemberPath].ToString();
                                    return $"\"{val.Replace("\"", "\"\"")}\"";
                                });
                            csvContent.AppendLine(string.Join(";", values));
                        }

                        File.WriteAllText(saveFileDialog.FileName, csvContent.ToString(), Encoding.UTF8);
                        UpdateStatus($"Экспортировано {dataView.Count} записей");
                        MessageBox.Show("Данные успешно экспортированы", "Экспорт завершен",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        UpdateStatus("Нет данных для экспорта");
                        MessageBox.Show("Нет данных для экспорта", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    UpdateStatus("Экспорт отменен");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка экспорта: {ex.Message}");
                MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Admin adminWindow = new Admin();
            adminWindow.Show();
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UpdateStatus(string message)
        {
            StatusText.Text = message;

            if (message != "Готово" && !message.StartsWith("Всего записей:"))
            {
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3);
                timer.Tick += (s, args) =>
                {
                    StatusText.Text = "Готово";
                    timer.Stop();
                };
                timer.Start();
            }
        }
    }
}
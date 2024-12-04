using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Dip
{
    public partial class Window1 : Window
    {
        private string connectionString = "Server=LAPTOP-V0AGQKUF\\SLAUUUIK;Database=GetDiplom;Trusted_Connection=True;"; 
        private int currentUserId = 1;
        private List<Status> statuses;
        private List<Trolleybus> trolleybuses;


        public Window1()
        {
            InitializeComponent();
            LoadData();
            LoadStatuses();
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Id, Number, (SELECT Name FROM Status WHERE Id = T.StatusId) AS StatusName, Data FROM Trolleybuses T";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    TrolleybusDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Ошибка при выполнении SQL-запроса: {sqlEx.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LoadStatuses() 
            { 
                statuses = new List<Status>(); 
                try 
                { 
                    using (SqlConnection connection = new SqlConnection(connectionString)) 
                    { 
                        connection.Open(); 
                        string query = "SELECT Id, Name FROM Status"; 
                        using (SqlCommand command = new SqlCommand(query, connection)) 
                        { 
                            SqlDataReader reader = command.ExecuteReader(); 
                            while (reader.Read()) 
                            { 
                                statuses.Add(new Status 
                                { 
                                    Id = reader.GetInt32(0), 
                                    Name = reader.GetString(1) 
                                }); 
                            } 
                        } 
                    } 
 
                    StatusComboBox.ItemsSource = statuses;  
                    StatusComboBox.DisplayMemberPath = "Name";  
                    StatusComboBox.SelectedValuePath = "Id";  
                } 
                catch (Exception ex) 
                { 
                    MessageBox.Show($"Ошибка при загрузке статусов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); 
                } 
            } 

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NumberTextBox.Text) || StatusComboBox.SelectedIndex == -1 || !LastMaintenanceDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newTrolleybus = new
            {
                Number = NumberTextBox.Text,
                StatusId = (int)StatusComboBox.SelectedValue,
                Data = LastMaintenanceDatePicker.SelectedDate.Value
            };

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Trolleybuses (Number, StatusId, Data) VALUES (@Number, @StatusId, @Data)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Number", SqlDbType.NVarChar).Value = newTrolleybus.Number;
                        command.Parameters.Add("@StatusId", SqlDbType.Int).Value = newTrolleybus.StatusId;
                        command.Parameters.Add("@Data", SqlDbType.DateTime).Value = newTrolleybus.Data;

                        command.ExecuteNonQuery();
                    }
                }

                LogUserAction(currentUserId, "Добавление троллейбуса", $"Добавлен троллейбус с номером {newTrolleybus.Number}.");

                LoadData();

                NumberTextBox.Clear();
                StatusComboBox.SelectedIndex = -1;
                LastMaintenanceDatePicker.SelectedDate = null;

                MessageBox.Show("Троллейбус успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении троллейбуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrolleybusDataGrid.SelectedItem is DataRowView selectedRow)
            {
                int selectedTrolleybusId = Convert.ToInt32(selectedRow["Id"]);
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            string query = "DELETE FROM Trolleybuses WHERE Id = @Id";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Id", selectedTrolleybusId);
                                command.ExecuteNonQuery();
                            }
                        }

                        LogUserAction(currentUserId, "Удаление троллейбуса", $"Удален троллейбус с ID {selectedTrolleybusId}.");

                        LoadData();
                        MessageBox.Show("Запись успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите запись для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LogUserAction(int userId, string action, string details)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO UserActions (UserId, Action, Details) VALUES (@UserId, @Action, @Details)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Action", action);
                    command.Parameters.AddWithValue("@Details", details);
                    command.ExecuteNonQuery();
                }
            }
        }

    }

   
}

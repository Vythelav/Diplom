using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Dip
{
    /// <summary>
    /// Логика взаимодействия для Action.xaml
    /// </summary>
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
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM UserActions ORDER BY ActionDate DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dataTable);
                }
            }

            UserActionsDataGrid.ItemsSource = dataTable.DefaultView;
        }
    }
}

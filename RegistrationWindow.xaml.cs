using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.SqlClient;

namespace Kursova_Steam_Parser_2._0
{
    public partial class RegistrationWindow : Window
    {
        public readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Registred;Integrated Security=True";

        public delegate void RegistrationSuccessEventHandler(string username);
        public event RegistrationSuccessEventHandler RegistrationSuccess;

        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private async Task InsertUserAsync(string username, string password)
        {
            try
            {
                string hashedPassword = HashPassword(password);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = "INSERT INTO Regist (Username, Password) VALUES (@Username, @Password)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", hashedPassword);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                ShowErrorMessage("Помилка при вставці даних: " + ex.Message);
            }
        }





        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                foreach (byte b in hashedBytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowErrorMessage("Введіть email і пароль!");
            }
            else
            {
                Task.Run(async () =>
                {
                    await InsertUserAsync(username, password);
                }).Wait();

                MessageBox.Show("Реєстрація успішна!");
            }
        }

        public void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

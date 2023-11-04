using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kursova_Steam_Parser_2._0
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _gamesParsed;
        private List<Game> _games;
        private List<Game> _selectedGames;
        private List<string> _genres;
        private string _selectedGenre;
        public RegistrationWindow _registrationWindow;
        public List<Game> Games
        {
            get => _games;
            set
            {
                _games = value;
                OnPropertyChanged();
            }
        }

        public List<string> Genres
        {
            get => _genres;
            set
            {
                _genres = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _selectedGames = new List<Game>();
            _gamesParsed = false;
            ParseGamesAsync();


        }

        private int _currentPage = 1;
        private int _itemsPerPage = 25; // Кількість елементів на одній сторінці

        // Метод для парсингу ігор
        public async Task ParseGamesAsync()
        {
            var games = new List<Game>();
            var urlBase = "https://store.steampowered.com/search/?sort_by=_ASC&supportedlang=ukrainian&category1=1&page=";

            for (int page = 1; page <= 10; page++)
            {
                var url = urlBase + page;
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(url);
                var nodes = doc.DocumentNode.SelectNodes("//div[@id='search_resultsRows']/a");

                foreach (var node in nodes)
                {
                    var nameNode = node.SelectSingleNode(".//span[@class='title']");
                    var priceNode = node.SelectSingleNode(".//div[@class='col search_price_discount_combined responsive_secondrow']");
                    var imageNode = node.SelectSingleNode(".//img");

                    var game = new Game
                    {
                        Name = nameNode?.InnerText?.Trim(),
                        Price = priceNode?.InnerText?.Trim(),
                        ImageUrl = imageNode?.GetAttributeValue("src", "") ?? "Empty"
                    };

                    games.Add(game);
                }
            }
            Games = games;
            UpdateGamesListView();
        }

        private void GamesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedGames.Clear();
            foreach (var item in GamesListView.SelectedItems)
            {
                if (item is Game game)
                {
                    _selectedGames.Add(game);
                }
            }
        }

        private void PlaySelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedGames = SelectedGamesListView.SelectedItems.Cast<Game>().ToList();
            if (selectedGames.Any())
            {
                var gamesNames = string.Join(", ", selectedGames.Select(game => game.Name));
                MessageBox.Show($"Starting selected games: {gamesNames}", "Games Launch", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select games to play.", "Games Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var game in _selectedGames)
            {
                if (!SelectedGamesListView.Items.Contains(game))
                {
                    Games.Remove(game);
                    SelectedGamesListView.Items.Add(game);
                }
            }
        }

        private void SelectedGamesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedGame = SelectedGamesListView.SelectedItem as Game;
            if (selectedGame != null)
            {
                MessageBox.Show($"Name: {selectedGame.Name}\nPrice: {selectedGame.Price}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear your library?", "Delete confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SelectedGamesListView.Items.Clear();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedGamesListView.Items.Remove(SelectedGamesListView.SelectedItem);
        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.ShowDialog();
        }


        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchGameTextBox.Text.ToLower();
            var matchingGames = Games.Where(game => game.Name.ToLower().Contains(searchText)).ToList();
            GamesListView.ItemsSource = matchingGames;
        }

        private void TimesButton_Click(object sender, RoutedEventArgs e)
        {
            TimerWindows timerWindows = new TimerWindows();
            timerWindows.Show();
        }

        private void UpdatePageInfo()
        {
            int totalItems = Games.Count; // Загальна кількість ігор
            int totalPages = (int)Math.Ceiling((double)totalItems / _itemsPerPage);
            PageInfoTextBlock.Text = $"Page {_currentPage} of {totalPages}";
        }

        private void UpdateGamesListView()
        {
            // Відображення ігор на поточній сторінці
            var gamesOnCurrentPage = Games.Skip((_currentPage - 1) * _itemsPerPage).Take(_itemsPerPage).ToList();
            GamesListView.ItemsSource = gamesOnCurrentPage;
            UpdatePageInfo();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Перехід на наступну сторінку, якщо існують ще елементи
            int totalItems = Games.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / _itemsPerPage);
            if (_currentPage < totalPages)
            {
                _currentPage++;
                UpdateGamesListView();
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            // Перехід на попередню сторінку, якщо не перший рядок
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdateGamesListView();
            }
        }

        private async Task InsertSelectedGamesAsync(List<Game> selectedGames)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_registrationWindow.connectionString))
                {
                    await connection.OpenAsync();

                    foreach (var game in selectedGames)
                    {
                        string query = "INSERT INTO Games (Name, Price, ImageUrl) VALUES (@Name, @Price, @ImageUrl)";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Name", game.Name);
                            command.Parameters.AddWithValue("@Price", game.Price);
                            command.Parameters.AddWithValue("@ImageUrl", game.ImageUrl);

                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _registrationWindow.ShowErrorMessage("Помилка при вставці ігор: " + ex.Message);
            }
        }
        private async Task LoadUserGamesAsync(string username)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_registrationWindow.connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT Name, Price, ImageUrl FROM Games WHERE Username = @Username";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            var userGames = new List<Game>();
                            while (reader.Read())
                            {
                                var game = new Game
                                {
                                    Name = reader["Name"].ToString(),
                                    Price = reader["Price"].ToString(),
                                    ImageUrl = reader["ImageUrl"].ToString()
                                };
                                userGames.Add(game);
                            }
                            // Оновити інтерфейс з отриманими іграми користувача
                            GamesListView.ItemsSource = userGames;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _registrationWindow.ShowErrorMessage("Помилка при завантаженні ігор: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Game
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string ImageUrl { get; set; }
    }
}

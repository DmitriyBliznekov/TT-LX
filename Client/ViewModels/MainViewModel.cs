using Client.Commands;
using Client.Helpers;
using Client.Models;
using Client.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class MainViewModel : ObservableObject, ISendRow
    {
        private const string Server = "http://localhost:5038";

        private readonly HubConnection _connection; // need to dispose?!
        private bool _isFetchAllInProcess;
        private bool _isGenerateDataInProcess;
        private bool _isClearDbInProcess;
        private bool _signalRToggle;

        public MainViewModel()
        {
            FetchAllCommand = new AsyncRelayCommand(FetchAllAsync, () => !_isFetchAllInProcess);
            GenerateDataCommand = new AsyncRelayCommand(GenerateDataAsync, () => !_isGenerateDataInProcess);
            ClearDbCommand = new AsyncRelayCommand(ClearDbAsync, () => !_isClearDbInProcess && Products.Count != 0);
            ToggleSignalRCommand = new AsyncRelayCommand(ToggleSignalR);

            _connection = new HubConnectionBuilder()
                .WithUrl($"{Server}/send_row")
                .Build();
        }

        public ObservableCollection<Product> Products { get; set; } = [];

        public AsyncRelayCommand FetchAllCommand { get; set; }
        public AsyncRelayCommand GenerateDataCommand { get; set; }
        public AsyncRelayCommand ClearDbCommand { get; set; }
        public AsyncRelayCommand ToggleSignalRCommand { get; set; }

        public bool SignalRToggle
        {
            get => _signalRToggle;
            set => SetProperty(ref _signalRToggle, value);
        }

        public async Task FetchAllAsync()
        {
            try
            {
                _isFetchAllInProcess = true;

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{Server}/fetch_all");
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    var products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();

                    if (products == null)
                    {
                        Debug.WriteLine("Error parsing response: products is null");
                        return;
                    }

                    foreach (var product in products)
                        Products.Add(product);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error fetching products: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error: {ex.Message}");
            }
            finally
            {
                _isFetchAllInProcess = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public async Task GenerateDataAsync()
        {
            try
            {
                _isGenerateDataInProcess = true;

                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync($"{Server}/generate_data?count=10000", null);

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                    Debug.WriteLine("Products generated successfully!");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine("Error generating products: " + ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unexpected error: " + ex.Message);
            }
            finally
            {
                _isGenerateDataInProcess = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public async Task ClearDbAsync()
        {
            var dialog = MessageBox.Show("Do you really want to clear all data?", "Attention", MessageBoxButton.OKCancel, MessageBoxImage.None, MessageBoxResult.Cancel);

            if (dialog != MessageBoxResult.OK)
                return;

            try
            {
                _isClearDbInProcess = true;
                Products.Clear();

                using var httpClient = new HttpClient();

                var response = await httpClient.DeleteAsync($"{Server}/clear_db");
                response.EnsureSuccessStatusCode();

                Console.WriteLine("Database cleared successfully!");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Error clearing database: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
            }
            finally
            {
                _isClearDbInProcess = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public async Task ToggleSignalR()
        {
            if (SignalRToggle)
                await _connection.StartAsync();
            else
                await _connection.StopAsync();
        }

        public Task SendRow(string product)
        {
            var receivedProduct = JsonSerializer.Deserialize<Product>(product);
            Products.Add(receivedProduct);

            return Task.CompletedTask;
        }
    }
}

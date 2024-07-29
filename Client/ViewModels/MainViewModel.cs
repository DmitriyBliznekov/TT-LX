using Client.Commands;
using Client.Helpers;
using Client.SignalR;
using Domain.Models;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly ProductMonitor _productMonitor;
    private bool _isFetchAllInProcess;
    private bool _isGenerateDataInProcess;
    private bool _isClearDbInProcess;
    private bool _monitorToggle;

    public MainViewModel()
    {
        FetchAllCommand = new AsyncRelayCommand(FetchAllAsync, () => !_isFetchAllInProcess);
        GenerateDataCommand = new AsyncRelayCommand(GenerateDataAsync, () => !_isGenerateDataInProcess);
        ClearDbCommand = new AsyncRelayCommand(ClearDbAsync, () => !_isClearDbInProcess && Products.Count != 0);
        ToggleMonitorCommand = new AsyncRelayCommand(ToggleMonitor);

        _productMonitor = new(ConfigurationManager.AppSettings.Get("Server"));
        _productMonitor.ProductReceived += ProductMonitor_ProductReceived;
    }

    public ObservableCollection<Product> Products { get; set; } = [];

    public AsyncRelayCommand FetchAllCommand { get; set; }
    public AsyncRelayCommand GenerateDataCommand { get; set; }
    public AsyncRelayCommand ClearDbCommand { get; set; }
    public AsyncRelayCommand ToggleMonitorCommand { get; set; }

    public bool MonitorToggle
    {
        get => _monitorToggle;
        set => SetProperty(ref _monitorToggle, value);
    }

    public async Task FetchAllAsync()
    {
        try
        {
            _isFetchAllInProcess = true;

            var server = ConfigurationManager.AppSettings.Get("Server") ?? 
                         throw new InvalidOperationException("Unable to read server settings. Check the configuration file.");

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{server}/fetch_all");
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

            var server = ConfigurationManager.AppSettings.Get("Server") ??
                         throw new InvalidOperationException("Unable to read server settings. Check the configuration file.");

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync($"{server}/generate_data?count=10000", null);

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

            var server = ConfigurationManager.AppSettings.Get("Server") ??
                         throw new InvalidOperationException("Unable to read server settings. Check the configuration file.");

            using var httpClient = new HttpClient();

            var response = await httpClient.DeleteAsync($"{server}/clear_db");
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

    public async Task ToggleMonitor()
    {
        if (MonitorToggle)
            await _productMonitor.StartMonitoring();
        else
            await _productMonitor.StopMonitoring();
    }

    private void ProductMonitor_ProductReceived(Product receivedProduct) => 
        Application.Current.Dispatcher.Invoke(() => Products.Add(receivedProduct));
}
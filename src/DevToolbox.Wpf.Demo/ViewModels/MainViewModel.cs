using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevToolbox.Wpf.Demo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    #region Fields/Consts

    [ObservableProperty]
    private object? _selectedItem;

    #endregion

    #region Properties

    public ObservableCollection<ExampleGategory>? ExampleCategories { get; private set; }

    public bool IsSettingsSelected => SelectedItem is SettingsViewModel;

    #endregion

    public MainViewModel()
    {
    }

    #region Methods

    internal async Task Initialize()
    {
        try
        {
            var resourceName = "DevToolbox.Wpf.Demo.examples.json";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using var reader = new StreamReader(stream, Encoding.UTF8);
            var jsonString = await reader.ReadToEndAsync();

            ExampleCategories = JsonSerializer.Deserialize<ObservableCollection<ExampleGategory>>(jsonString);
            OnPropertyChanged(nameof(ExampleCategories));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    partial void OnSelectedItemChanged(object? value)
    {
        OnPropertyChanged(nameof(IsSettingsSelected));
    }

    #endregion

    #region Relay Commands

    [RelayCommand]
    private void OpenSettings()
    {
        if (IsSettingsSelected)
        {
            SelectedItem = ExampleCategories?.FirstOrDefault();
            return;
        }

        SelectedItem = ViewModelLocator.SettingsViewModel;
    }

    #endregion
}
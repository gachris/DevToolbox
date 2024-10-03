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

    #endregion

    #region Relay Commands

    [RelayCommand]
    private void OpenSettings()
    {
        foreach (var exampleGategory in ExampleCategories?.ToArray() ?? [])
        {
            if (exampleGategory.IsSelected)
            {
                exampleGategory.IsSelected = false;
                break;
            }

            foreach (var subGategory in exampleGategory.SubCategories?.ToArray() ?? [])
            {
                if (subGategory.IsSelected)
                {
                    subGategory.IsSelected = false;
                    break;
                }
            }
        }

        SelectedItem = ViewModelLocator.SettingsViewModel;
    }

    #endregion
}
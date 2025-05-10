using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Markup;
using System.Xml;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevToolbox.Wpf.Demo.Json;

namespace DevToolbox.Wpf.Demo.ViewModels;

public partial class Example : ObservableObject
{
    private string? _sourceCode;

    [ObservableProperty]
    private string? _header;
    private object? _view;

    [JsonConverter(typeof(XamlConverter))]
    public string? SourceCode
    {
        get => _sourceCode;
        set
        {
            SetProperty(ref _sourceCode, value);

            try
            {
                if (!string.IsNullOrEmpty(value))
                {
                    using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(value));
                    View = XamlReader.Load(stream);
                }
                else
                {
                    View = null;
                }
            }
            catch
            {
                View = null;
            }
        }
    }

    public object? View
    {
        get => _view;
        private set => SetProperty(ref _view, value);
    }

    public ObservableCollection<Property>? Properties { get; set; }

    [RelayCommand]
    private void InvokeProperty(Property property)
    {
        if (string.IsNullOrEmpty(SourceCode))
        {
            return;
        }

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(SourceCode);

        var element = xmlDoc.DocumentElement;

        if (element != null)
        {
            var value = property.Value?.ToString();

            if (string.IsNullOrEmpty(property.Namespace))
            {
                element.RemoveAttribute(property.Name!);
            }
            else
            {
                element.RemoveAttribute(property.Name!, property.Namespace);
            }

            if (!string.IsNullOrEmpty(value))
            {
                element.SetAttribute(property.Name!, property.Namespace, value);
            }

            var stringWriter = new StringWriter();
            var xmlTextWriter = new XmlTextWriter(stringWriter);
            xmlDoc.WriteTo(xmlTextWriter);

            SourceCode = stringWriter.ToString();
        }
    }
}

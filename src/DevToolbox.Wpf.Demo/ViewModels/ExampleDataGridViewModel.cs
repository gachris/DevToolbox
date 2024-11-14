using System.Collections.ObjectModel;
using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToolbox.Wpf.Demo.ViewModels;

public class ExampleDataGridViewModel : ObservableObject
{
    #region Fields/Consts

    private readonly Random _random = new();

    #endregion

    #region Properties

    public DataTable SourceTable { get; } = new DataTable();

    public ObservableCollection<DataGridRowItem> SourceCollection { get; } = [];

    #endregion

    public ExampleDataGridViewModel()
    {
        InitializeDataTable();
        InitializeSourceCollection();
    }

    #region Methods

    private void InitializeDataTable()
    {
        SourceTable.Columns.Add("GameName", typeof(string));
        SourceTable.Columns.Add("Creator", typeof(string));
        SourceTable.Columns.Add("Publisher", typeof(string));
        SourceTable.Columns.Add("Owner", typeof(string));
        SourceTable.Columns.Add("Points", typeof(int));

        var gameNames = new[] { "The Witcher 3", "Cyberpunk 2077", "Elden Ring", "Minecraft", "Fortnite", "Apex Legends", "Valorant", "Among Us", "Overwatch 2", "Stardew Valley", "World Of Warcraft", "Halo", "God Of War", "Gears Of War" };
        var creators = new[] { "CD Projekt Red", "FromSoftware", "Mojang", "Epic Games", "Respawn Entertainment", "Riot Games", "InnerSloth", "Blizzard Entertainment", "ConcernedApe", "Blizzard", "Bungie", "Santa Monica Studio" };
        var publishers = new[] { "CD Projekt", "Bandai Namco", "Microsoft", "Epic Games", "EA", "Riot Games", "InnerSloth", "Activision", "Sony Interactive", "Blizzard", "Sony" };
        var owners = new[] { "Alice", "Bob", "Charlie", "David", "Eve", "Frank", "Grace", "Hannah", "Isaac", "Julia", "Mark", "Bill", "Steve" };

        SourceTable.BeginLoadData();

        for (var i = 0; i < 200; i++)
        {
            var gameName = gameNames[_random.Next(gameNames.Length)];
            var creator = creators[_random.Next(creators.Length)];
            var publisher = publishers[_random.Next(publishers.Length)];
            var owner = owners[_random.Next(owners.Length)];
            var points = _random.Next(1, 101);

            var row = SourceTable.NewRow();
            row["GameName"] = gameName;
            row["Creator"] = creator;
            row["Publisher"] = publisher;
            row["Owner"] = owner;
            row["Points"] = points;

            SourceTable.Rows.Add(row);
        }

        SourceTable.EndLoadData();
    }

    private void InitializeSourceCollection()
    {
        var gameNames = new[] { "The Witcher 3", "Cyberpunk 2077", "Elden Ring", "Minecraft", "Fortnite", "Apex Legends", "Valorant", "Among Us", "Overwatch 2", "Stardew Valley", "World Of Warcraft", "Halo", "God Of War", "Gears Of War" };
        var creators = new[] { "CD Projekt Red", "FromSoftware", "Mojang", "Epic Games", "Respawn Entertainment", "Riot Games", "InnerSloth", "Blizzard Entertainment", "ConcernedApe", "Blizzard", "Bungie", "Santa Monica Studio" };
        var publishers = new[] { "CD Projekt", "Bandai Namco", "Microsoft", "Epic Games", "EA", "Riot Games", "InnerSloth", "Activision", "Sony Interactive", "Blizzard", "Sony" };
        var owners = new[] { "Alice", "Bob", "Charlie", "David", "Eve", "Frank", "Grace", "Hannah", "Isaac", "Julia", "Mark", "Bill", "Steve" };

        for (var i = 0; i < 200; i++)
        {
            var gameName = gameNames[_random.Next(gameNames.Length)];
            var creator = creators[_random.Next(creators.Length)];
            var publisher = publishers[_random.Next(publishers.Length)];
            var owner = owners[_random.Next(owners.Length)];
            var points = _random.Next(1, 101);

            SourceCollection.Add(new DataGridRowItem
            {
                GameName = gameName,
                Creator = creator,
                Publisher = publisher,
                Owner = owner,
                Points = points
            });
        }
    }

    #endregion
}
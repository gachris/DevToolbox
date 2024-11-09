using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DevToolbox.Wpf.Helpers;

/// <summary>
/// Provides helper methods for working with <see cref="DataTable"/> objects.
/// </summary>
internal class DataTableHelper
{
    /// <summary>
    /// Converts an <see cref="IEnumerable{T}"/> collection to a <see cref="DataTable"/>.
    /// The resulting <see cref="DataTable"/> has columns based on the properties of the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection.</typeparam>
    /// <param name="collection">The collection of objects to convert to a <see cref="DataTable"/>.</param>
    /// <returns>A <see cref="DataTable"/> representation of the input collection.</returns>
    public static DataTable ToDataTable<T>(IEnumerable<T> collection)
    {
        // Return an empty DataTable if the collection is null or empty.
        if (collection == null || !collection.Any())
            return new DataTable();

        var dt = new DataTable();
        var t = typeof(T);

        // Determine the type if it is 'object' and the collection is not empty.
        if (t == typeof(object))
        {
            var firstItem = collection.FirstOrDefault();
            if (firstItem != null)
            {
                t = firstItem.GetType();
            }
            else
            {
                return dt; // Return an empty DataTable if the collection is empty.
            }
        }

        // Get the properties of the type and create DataTable columns.
        var properties = t.GetProperties();
        foreach (var prop in properties)
        {
            var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            dt.Columns.Add(prop.Name, propertyType);
        }

        // Populate the DataTable with data from the collection.
        try
        {
            foreach (var item in collection)
            {
                var row = dt.NewRow();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item) ?? DBNull.Value;
                    // Handle default 'char' values as DBNull.
                    if (value is char charValue && charValue == '\0')
                    {
                        value = DBNull.Value;
                    }
                    row[prop.Name] = value;
                }
                dt.Rows.Add(row);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while converting the collection to DataTable.", ex);
        }

        return dt;
    }
}
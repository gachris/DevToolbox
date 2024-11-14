using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Extensions;

internal static class LinqExtensions
{
    public static void AddRange<T>(this ObservableCollection<T> list, IEnumerable<T> items)
    {
        items.ForEach(list.Add);
    }

    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        foreach (T item in items)
        {
            action(item);
        }
    }

    public static IEnumerable<T> Select<T>(this IList entities, Func<object, T> action)
    {
        foreach (var entity in entities)
            yield return action.Invoke(entity);
    }

    public static IEnumerable<T> Select<T>(this ItemCollection entities, Func<object, T> action)
    {
        foreach (var entity in entities)
            yield return action.Invoke(entity);
    }
}

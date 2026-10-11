using System.Reflection;

public static class EntityMapper
{
    private static TDestination CreateDestination<TDestination>()
    {
        TDestination? destination = Activator.CreateInstance<TDestination>();

        if (destination is null)
        {
            throw new InvalidOperationException(
                $"Cannot create instance of {typeof(TDestination).Name}.");
        }

        return destination;
    }

    private static TDestination MapCore<TSource, TDestination>(TSource source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        TDestination destination = CreateDestination<TDestination>();

        PropertyInfo[] sourceProperties = typeof(TSource).GetProperties();
        PropertyInfo[] destinationProperties = typeof(TDestination).GetProperties();

        foreach (PropertyInfo sourceProperty in sourceProperties)
        {
            PropertyInfo? destinationProperty = destinationProperties.FirstOrDefault(x =>
                x.Name == sourceProperty.Name &&
                x.PropertyType == sourceProperty.PropertyType &&
                x.CanWrite);

            if (destinationProperty is not null)
            {
                object? value = sourceProperty.GetValue(source);
                destinationProperty.SetValue(destination, value);
            }
        }

        return destination;
    }

    public static TDestination Map<TSource, TDestination>(
        TSource source,
        Action<TSource, TDestination>? afterMap = null)
    {
        TDestination destination = MapCore<TSource, TDestination>(source);

        afterMap?.Invoke(source, destination);

        return destination;
    }

    public static List<TDestination> Map<TSource, TDestination>(
        IEnumerable<TSource> source,
        Action<TSource, TDestination>? afterMap = null)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        return source
            .Select(item => Map<TSource, TDestination>(item, afterMap))
            .ToList();
    }
}
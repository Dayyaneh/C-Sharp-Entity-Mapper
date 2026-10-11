using FdoMapper;

List<SourceEntity> sourceEntities = new List<SourceEntity>
{
    new()
    {
        Id = 1,
        Name = "Azure Storage Account",
        Description = "Primary storage account used for application data.",
        Type = "Storage",
        Url = "https://contoso.blob.core.windows.net",
        CategoryId = 10
    },
    new()
    {
        Id = 2,
        Name = "SharePoint Site",
        Description = "Corporate document repository.",
        Type = "Document",
        Url = "https://contoso.sharepoint.com/sites/fdo",
        CategoryId = 20
    },
    new()
    {
        Id = 3,
        Name = "Nomination API",
        Description = "Backend API for nomination management.",
        Type = "API",
        Url = "https://api.contoso.com/nomination",
        CategoryId = 30
    },
    new()
    {
        Id = 4,
        Name = "Migration Dashboard",
        Description = null,
        Type = "Dashboard",
        Url = null,
        CategoryId = 40
    },
    new()
    {
        Id = 5,
        Name = "Customer Data Warehouse",
        Description = "Centralized reporting database.",
        Type = "Database",
        Url = "https://sql.contoso.com",
        CategoryId = 50
    }
};


List<CategoryEntity> categories = new List<CategoryEntity>
{
    new()
    {
        Id = 10,
        Name = "Storage"
    },
    new()
    {
        Id = 20,
        Name = "Documents"
    },
    new()
    {
        Id = 30,
        Name = "APIs"
    },
    new()
    {
        Id = 40,
        Name = "Dashboards"
    },
    new()
    {
        Id = 50,
        Name = "Databases"
    },
    new()
    {
        Id = 60,
        Name = "Applications"
    },
    new()
    {
        Id = 70,
        Name = "Reports"
    },
    new()
    {
        Id = 80,
        Name = "Integrations"
    }
};


List<DestinationEntity> result =
    EntityMapper.Map<SourceEntity, DestinationEntity>(
        sourceEntities,
        (source, destination) =>
        {
            destination.category = categories
                .FirstOrDefault(x => x.Id == source.CategoryId);
        });


Console.WriteLine(result.Count);
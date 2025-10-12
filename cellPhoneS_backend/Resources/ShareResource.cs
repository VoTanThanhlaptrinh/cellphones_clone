using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Localization;

namespace cellphones_backend.Resources;


public class ShareResource
{
}

public interface IResourceLocalizer
{
    string localize(string key);
}

public class ResourceLocalizer : IResourceLocalizer
{
    private readonly IStringLocalizer _localizer;

    public ResourceLocalizer(IStringLocalizerFactory factory)
    {
        var type = typeof(ShareResource);
        var assemblyName = new AssemblyName(type.Assembly.FullName!);
        _localizer = factory.Create("ShareResource", assemblyName.Name!);
    }

    public string localize(string key)
    {
        throw new NotImplementedException();
    }
}

using System.Text.Json.Serialization;
using Siwa.Core.Assets;

namespace Siwa.Core.Helper;

public abstract class Asset
{
    [JsonInclude]
    public Guid Id = Guid.NewGuid();
    [JsonInclude]
    public string Name = "undefined";
    
    [JsonIgnore] public RawHandle Handle;

    public abstract void OnLoad();
    protected abstract void OnUnload();
}
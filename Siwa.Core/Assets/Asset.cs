using System.Text.Json.Serialization;

namespace Siwa.Core.Assets;

public abstract class Asset
{
    [JsonIgnore] public bool Readonly = false;
    [JsonIgnore] public string FilePath;
    [JsonInclude] public RawHandle Handle;
    [JsonInclude] public string Name = "undefined";

    public virtual void OnRestore()
    {
    }

    public virtual void OnLoad()
    {
    }

    protected virtual void OnUnload()
    {
    }
}
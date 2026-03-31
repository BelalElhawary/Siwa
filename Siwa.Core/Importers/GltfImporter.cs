using SharpGLTF.Schema2;
using Siwa.Core.Assets.Kinds;

namespace Siwa.Core.Importers;

public static class GltfImporter
{
    public static Model LoadAsModel(string path)
    {
        var model = ModelRoot.Load(path);
        
        var scene = model.DefaultScene;
        
        return default;
    }
}
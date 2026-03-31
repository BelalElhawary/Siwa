using System.Text.Json;
using Siwa;
using Siwa.Core;
using Siwa.Core.Assets;

var options = new JsonSerializerOptions();
options.Converters.Add(new Vector4Converter());
options.Converters.Add(new Vector3Converter());
options.Converters.Add(new MaterialHandleConverter());

var unlitAssets = new UnlitAssets(options);
var litAssets = new LitAssets(options);
var engineAssets = new EngineAssets(options);
Game game = new Game();
game.Run();
using System.Text.Json;
using Siwa;
using Siwa.Core;
using Siwa.Core.Assets;
using Siwa.Core.Serialization;

SerializationManager.Initialize();

var unlitAssets = new UnlitAssets(SerializationManager.Options);
var litAssets = new LitAssets(SerializationManager.Options);
var engineAssets = new EngineAssets(SerializationManager.Options);
Game game = new Game();
game.Run();
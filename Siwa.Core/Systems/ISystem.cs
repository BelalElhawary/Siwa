namespace Siwa.Core.Systems;

public interface ISystem
{
    void Initialize();
    void Start();
    void Update(float dt);
}
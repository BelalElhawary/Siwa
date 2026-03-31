namespace Siwa.Core.Systems;

public class SystemCollection<T> where T : ISystem
{
    private readonly List<(int, T)> _systems = new();

    public void Add(int order, T system)
    {
        _systems.Add((order, system));
    }

    public T[] ToArray() => _systems.OrderBy(s => s.Item1).Select(s => s.Item2).ToArray();
}
using Siwa.Core.Helper;

namespace Siwa.Core.Assets;

public sealed class AssetPool<T> where T : Asset
{
    public static readonly AssetPool<T> Registry = new();
        
    private T?[] _assets = new T?[16];
    private uint[] _generations = new uint[16];
    private readonly Stack<uint> _freeIndices = new();
    private uint _nextIndex;
        
    private void EnsureCapacity(uint index)
    {
        if (index < _assets.Length) return;
        var newSize = _assets.Length * 2;
        Array.Resize(ref _assets, newSize);
        Array.Resize(ref _generations, newSize);
    }
        
    public void Unload(Handle<T> handle)
    {
        if (_generations[handle.Index] != handle.Generation) return;

        // Incrementing generation invalidates all existing handles to this slot
        _generations[handle.Index]++; 
        _assets[handle.Index] = null; // Help the GC
        _freeIndices.Push(handle.Index);
    }
        
    public Handle<T> Register(T asset)
    {
        var index = _freeIndices.Count > 0 ? _freeIndices.Pop() : _nextIndex++;
        EnsureCapacity(index);
        _assets[index] = asset;
        return new Handle<T>(index, _generations[index]);
    }
        
    public T? Get(Handle<T> handle)
    {
        if(_generations[handle.Index] != handle.Generation) throw new Exception("Invalid asset handle");
        return _assets[handle.Index];
    }
    
    public T[] GetAll() => _assets;
}


public readonly struct Handle<T>(uint index, uint generation) where T : Asset
{
    public readonly uint Index = index;
    public readonly uint Generation = generation;
    public RawHandle ToRaw() => new(Index, Generation);
}
public readonly struct RawHandle(uint index, uint generation)
{
    public Handle<T> ToHandle<T>() where T : Asset => new(index, generation);
}
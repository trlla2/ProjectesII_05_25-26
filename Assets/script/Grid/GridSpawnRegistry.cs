using System.Collections.Generic;

//static class to hold spawn weight for the grid spawner.
public static class GridSpawnRegistry
{
    private static Dictionary<ElementType, float> _overrides = new Dictionary<ElementType, float>();

    public static void SetWeightOverride(ElementType type, float weight)
    {
        _overrides[type] = weight;
    }

    public static void RemoveWeightOverride(ElementType type)
    {
        _overrides.Remove(type);
    }

    public static bool TryGetWeightOverride(ElementType type, out float weight)
    {
        return _overrides.TryGetValue(type, out weight);
    }

    //Called automatically before any scene loads to avoid stale
    // state leaking between editor play sessions
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Reset() => _overrides.Clear();
}
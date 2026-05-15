using UnityEngine;
using System.Collections.Generic;


public class CollectibleManager : MonoBehaviour
{
    private static CollectibleManager instance;
    private static List<InsanityCollectible> collectibles = new List<InsanityCollectible>();

    // -- Singleton setup ------------------------------------------------------

    private static void EnsureInstance()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("CollectibleManager");
            instance = obj.AddComponent<CollectibleManager>();
            DontDestroyOnLoad(obj);
        }
    }

    // -- Public API -----------------------------------------------------------

    public static void Register(InsanityCollectible collectible)
    {
        EnsureInstance();
        if (!collectibles.Contains(collectible))
            collectibles.Add(collectible);
    }

    public static void Unregister(InsanityCollectible collectible)
    {
        collectibles.Remove(collectible);
    }

    /// <summary>
    /// Resets all collectibles — makes them all reappear.
    /// Called automatically when player resets to checkpoint.
    /// </summary>
    public static void ResetAll()
    {
        foreach (var c in collectibles)
        {
            if (c != null)
                c.ResetCollectible();
        }

        Debug.Log("CollectibleManager: Reset " + collectibles.Count + " collectibles.");
    }
}
using UnityEngine;
using System.Collections;
using System.Reflection;

public class SingletonMonoBehaviourOnDemand<T>
    : MonoBehaviour
    where T : Component
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
                instance = Initialize();
            return instance;
        }
    }

    public static T Initialize()
    {
        var typename = typeof(T).Name;
        var instance = GameObject.Find(typename);
        if (instance == null)
            instance = new GameObject(typename, typeof(T));

        var component = instance.GetComponent<T>();
        if (component == null)
            component = instance.AddComponent<T>();

        var flags =
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.FlattenHierarchy;

        var type = typeof(T);
        var method = type.GetMethod("OnScriptReload", flags);

        // can be null
        if (method != null)
        {
            method.Invoke(component, null);
        }

        return component;
    }
}


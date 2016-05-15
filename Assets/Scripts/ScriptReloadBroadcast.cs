using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

// TODO: EditorApplication.heirarchyWindowChanged += Callback; // handle new objects, call Start()

#if UNITY_EDITOR
[InitializeOnLoad]
public class ScriptReloadBroadcastStartup
{
    static ScriptReloadBroadcastStartup()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        try
        {
            ScriptReloadBroadcast.OnStart();
        }
        catch
        {
            // ignore
        }

        EditorApplication.update -= Update;
    }
}
#endif

public class ScriptReloadBroadcast
{
#if UNITY_EDITOR 
    [UnityEditor.Callbacks.DidReloadScripts]
    public static void OnScriptsReloaded()
    {
        var flags =
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.FlattenHierarchy;

        var preFunc = Application.isPlaying ? "OnPreScriptReload" : "OnPreScriptReloadEditor";
        var preObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (var obj in preObjects)
        {
            var components = obj.GetComponents<Component>();

            foreach (var com in components)
            {
               if (com == null)
                    continue;

                var type = com.GetType();
                var method = type.GetMethod(preFunc, flags);

                if (method != null)
                {
                    Debug.LogFormat("Broadcast: pre-initializing {0}.{1}...", com.gameObject.name, type.ToString());

                    method.Invoke(com, null);
                }
            }
        }

        var reloadFunc = Application.isPlaying ? "OnScriptReload" : "OnScriptReloadEditor";
        var reloadObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (var obj in reloadObjects)
        {
            var components = obj.GetComponents<Component>();

            foreach (var com in components)
            {
                if (com == null)
                    continue;

                var type = com.GetType();
                var method = type.GetMethod(reloadFunc, flags);

                if (method != null)
                {
                    Debug.LogFormat("Broadcast: initializing {0}.{1}...", com.gameObject.name, type.ToString());

                    method.Invoke(com, null);
                }
            }
        }
    }

    public static void OnStart()
    {
        if (!Application.isPlaying)
            return;

        var reload_flags =
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.FlattenHierarchy;

        var start_flags =
            BindingFlags.Public |
            BindingFlags.Instance;

        var start_func = "Start";
        var reload_func = "OnScriptReload";

        var objects = GameObject.FindObjectsOfType<GameObject>();

        foreach (var obj in objects)
        {
            var components = obj.GetComponents<Component>();

            foreach (var com in components)
            {
                if (com == null)
                    continue;

                var type = com.GetType();
                var start_method = type.GetMethod(start_func, start_flags);
                var reload_method = type.GetMethod(reload_func, reload_flags);

                if (start_method == null && reload_method != null)
                {
                    //Debug.LogFormat("Broadcast: (startup) initializing {0}.{1}...", com.gameObject.name, type.ToString());

                    reload_method.Invoke(com, null);
                }
            }
        }
    }
#else
    public static void OnScriptsReloaded()
    {
    }
#endif
}

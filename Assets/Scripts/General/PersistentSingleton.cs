using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour 
{
    // a persistent singleton is a singleton attached to a prefab 
    // so its data is carried between scenes

    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (instance == null)
                {
                    // instantiate the prefab from the Resources/ folder, give its name
                    instance = Instantiate(Resources.Load<T>("Systems/" + typeof(T).Name));
                }

                DontDestroyOnLoad(instance.gameObject);
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnApplicationQuit()
    {
        instance = null; 
        Destroy(gameObject);
    }
} 

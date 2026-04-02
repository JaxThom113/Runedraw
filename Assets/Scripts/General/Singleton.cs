using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour 
//"Where T" sets a constraint on the type T to be a MonoBehaviour to allow for start and update functions
{
    // Makes Script Globally Accessible 
    // Only one instance of the script can exist 
    // Class holds a static instance of itself  
    // Allows you to access the instance from anywhere in the code
    public static T Instance { get; private set; } 
    // Virtual means it can be overridden by a subclass 
    // Protected means it can be accessed by the subclass but not outside of the class
    protected virtual void Awake() 
    {  
        //Check if an instance already exists, if so, destroy the new instance
        if (Instance != null)
        {
            Destroy(gameObject); 
            return;
        }  
        //If no instance exists, set the instance to the current instance
        Instance = this as T;
    } 

    protected virtual void OnApplicationQuit()
    {
        Instance = null; 
        Destroy(gameObject);
    }
} 

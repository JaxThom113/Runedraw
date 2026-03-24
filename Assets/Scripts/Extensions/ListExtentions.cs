using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtentions 
{
    public static T Draw<T>(this List<T> list) 
    { 
        if (list.Count == 0) return default; 
        int r = Random.Range(0, list.Count); //Random Drawing!!
        T t =list[r]; 
        list.Remove(t); 
        return t; 
    }
    public static T DrawFront<T>(this List<T> list) 
    { 
        if (list.Count == 0) return default; 
        T t = list[0]; 
        list.Remove(t); 
        return t; 
    }
}

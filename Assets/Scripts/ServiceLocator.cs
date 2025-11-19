using System;
using System.Collections.Generic;
using UnityEngine; //PARA LOS DEBUGS

public sealed class ServiceLocator 
{
    public static ServiceLocator Instance => _instance ??= new ServiceLocator();
    private static ServiceLocator _instance;
    private static Dictionary<Type, object> _dependency = new Dictionary<Type, object>();

    public void RegisterDependency<T>(T inter)
    {
        var type = typeof(T); 

        _dependency.TryAdd(type, inter);
        
        //Debug.Log($"{inter} typeof({type}) REGISTRADO");
    }

    public T GetDependency<T>()
    {
        var type = typeof(T);

        if( _dependency.ContainsKey(type) )
            return (T)_dependency[type];

        return default;
    }

    public bool TryGetDependency<T>(out T inter)
    {
        var type = typeof(T);

        if(_dependency.ContainsKey(type))
        {
            inter = (T)_dependency[type];
            return true;
        }

        inter = default;
        return false;
    }

    public void RemoveDependency<T>()
    {
        var type = typeof(T);

        if(_dependency.ContainsKey(type))
            _dependency.Remove(type);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public static class AppContainer
{
    // Diccionario de los servicios registrados
    private static Dictionary<Type, Func<object>> _servicesRegistered = new Dictionary<Type, Func<object>>();

    // Diccionario de los servicios instanciados 
    private static Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static void Register<T>(Func<object> function)
    {
        _servicesRegistered.Add(typeof(T), function);
    }

    public static T Get<T>()
    {
        Type type = typeof(T);

        //Buscamos en el listado de los objetos instanciados
        if(_services.TryGetValue(type, out object service))
            return (T)service;
        
        //Si tenemos registrado el Type la instanciamos y la registramos
        if(_servicesRegistered.TryGetValue(type, out var serviceRegistered))
        {
            var newService = serviceRegistered();
            _services.Add(type, newService);
            return (T)newService;
        }

        Debug.LogError($"No se ha registrado el servicio {type}");
        return default(T);
    }
}

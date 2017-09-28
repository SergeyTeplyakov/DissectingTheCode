using System;
using System.Collections.Generic;

namespace LocalFunctions
{
    public class RecursiveLocalFunction
    {
public static List<Type> BaseTypesAndSelf2(Type type)
{
    Action<List<Type>, Type> addBaseType = null;
    addBaseType = (lst, t) =>
    {
        lst.Add(t);
        if (t.BaseType != null)
        {
            addBaseType(lst, t.BaseType);
        }
    };

    var result = new List<Type>();
    addBaseType(result, type);
    return result;
}

public static List<Type> BaseTypesAndSelf(Type type)
{
    List<Type> AddBaseType(List<Type> lst, Type t)
    {
        lst.Add(t);
        if (t.BaseType != null)
        {
            AddBaseType(lst, t.BaseType);
        }
        return lst;
    }

    return AddBaseType(new List<Type>(), type);
}
    }
}
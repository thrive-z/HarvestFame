using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件中心
/// </summary>
public static class EventBus 
{
    //维护一个字典 事件
    private static readonly Dictionary<Type, Delegate> eventTable = new();

    //订阅
    public static void Subscribe<T>(Action<T> callback)
    {
        //如果没有 则添加
        if ( !eventTable.ContainsKey(typeof(T)))
        {
            eventTable[typeof(T)] = null;
        }

        eventTable[typeof(T)] = (Action<T>)eventTable[typeof(T)] + callback;
    }

    //取消订阅
    public static void Unsubscribe<T>(Action<T> callback)
    {
        if (eventTable.TryGetValue(typeof(T), out var del))
        {
            eventTable[typeof(T)] = (Action<T>)del - callback;
        }
    }

    //发布事件
    public static void Publish<T>(T evenData)
    {
        if (eventTable.TryGetValue(typeof(T),out var del))
        {
            (del as  Action<T>).Invoke(evenData);
        }
    }

    //清空事件
    public static void Clear() => eventTable.Clear();

}


// public class SwitchToolBarEvent
// {
//         
// }
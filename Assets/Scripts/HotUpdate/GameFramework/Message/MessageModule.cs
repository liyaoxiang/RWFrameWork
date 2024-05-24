using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class MessageModule : BaseGameModule
{
    public delegate Task MessageHandlerEventArgs<T>(T arg);

    private Dictionary<Type, List<object>> globalMessageHandlers;//全局消息
    private Dictionary<Type, List<object>> localMessageHandlers;//本地消息
    public Monitor Monitor { get; private set; }

    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        localMessageHandlers= new Dictionary<Type, List<object>>();
        Monitor=new Monitor();
        LoadAllMessageHandlers();
    }
    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        globalMessageHandlers = null;
        localMessageHandlers = null;
    }
    private void LoadAllMessageHandlers()
    {
        globalMessageHandlers = new Dictionary<Type, List<object>>();
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            if (type.IsAbstract)
                continue;
            //从当前类型中获取MessageHandlerAttribute特性。
            //如果特性不存在于当前类型，但存在于任何基类或接口上（由于true参数），则也会获取它
            MessageHandlerAttribute messageHandlerAttribute = type.GetCustomAttribute<MessageHandlerAttribute>(true);
            if (messageHandlerAttribute != null)
            {
                //使用Activator.CreateInstance创建当前类型的实例，并尝试将其转换为IMessageHander接口
                IMessageHander messageHandler = Activator.CreateInstance(type) as IMessageHander;
                if (!globalMessageHandlers.ContainsKey(messageHandler.GetHandlerType()))
                {
                    globalMessageHandlers.Add(messageHandler.GetHandlerType(), new List<object>());
                }
                //将当前消息处理器实例添加到与其处理类型对应的列表中
                globalMessageHandlers[messageHandler.GetHandlerType()].Add(messageHandler);
            }
        }
    }
    public void Subscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        Type argType=typeof(T);
        if (!localMessageHandlers.TryGetValue(argType,out var handlerList))
        {
            handlerList = new List<object>();
            localMessageHandlers.Add(argType, handlerList);
        }
        handlerList.Add(handler);
    }
    public void Unsubscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        if (!localMessageHandlers.TryGetValue(typeof(T), out var handlerList))
            return;
        handlerList.Remove(handler);
    }
    public async Task Post<T>(T arg) where T : struct
    {
        if (globalMessageHandlers.TryGetValue(typeof(T), out List<object> globalHandlerList))
        {
            foreach (var handler in globalHandlerList)
            {
                if (!(handler is MessageHandler<T> messageHandler))
                    continue;
                await messageHandler.HandleMessage(arg);
            }
        }
        if (localMessageHandlers.TryGetValue(typeof(T), out List<object> localHandlerList))
        {
            List<object> list = ListPool<object>.Obtain();
            list.AddRangeNonAlloc(localHandlerList);
            foreach (var handler in list)
            {
                if(!(handler is MessageHandlerEventArgs<T> messageHandler))
                    continue;
                await messageHandler(arg);
            }
            ListPool<object>.Release(list);
        }
    }
}

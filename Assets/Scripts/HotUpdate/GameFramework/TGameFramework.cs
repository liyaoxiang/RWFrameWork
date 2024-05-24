using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class TGameFramework
{
    public static TGameFramework Instance { get;private set; }
    public static bool Initialized { get; private set; }
    private Dictionary<Type, BaseGameModule> m_modules = new Dictionary<Type, BaseGameModule>();

    public static void Initialize()
    {
        Instance = new TGameFramework();
    }

    public T GetModule<T>() where T : BaseGameModule
    {
        if (m_modules.TryGetValue(typeof(T),out BaseGameModule module))
        {
            return module as T;
        }
        return default(T);
    }
    public void AddModule(BaseGameModule module)
    {
        Type moduleType = module.GetType();
        if (m_modules.ContainsKey(moduleType))
        {
            Debug.Log($"Module���ʧ�ܣ��ظ�:{moduleType.Name}");
            return;
        }
        m_modules.Add(moduleType, module);
    }
    public void Update()
    {
        if (!Initialized)
            return;
        if (m_modules == null)
            return;
        if (!Initialized)
            return;
        float deltaTime = Time.deltaTime;
        foreach (var module in m_modules.Values) 
        {
            module.OnModuleUpdate(deltaTime);
        }
    }
    public void LateUpdate()
    {
        if (!Initialized)
            return;

        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleLateUpdate(deltaTime);
        }
    }

    public void FixedUpdate()
    {
        if (!Initialized)
            return;

        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        float deltaTime = UnityEngine.Time.fixedDeltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleFixedUpdate(deltaTime);
        }
    }
    public void InitModules()
    {
        if (Initialized)
            return;
        Initialized = true;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleInit();
        }
    }
    public void StartModules()
    {
        if (m_modules == null)
            return;
        if (!Initialized)
            return;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleStart();
        }
    }
    public void Destroy()
    {
        if (!Initialized)
            return;

        if (Instance != this)
            return;

        if (Instance.m_modules == null)
            return;
        foreach (var module in Instance.m_modules.Values)
        {
            module.OnModuleStop();
        }
        Instance = null;
        Initialized = false;
    }
}

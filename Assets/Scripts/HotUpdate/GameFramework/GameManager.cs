using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static GameManager;

public class GameManager : MonoBehaviour
{
    [Module(6)]
    public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }
    private bool activing;
    private void Awake()
    {
        
        if (TGameFramework.Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        activing = true;
        DontDestroyOnLoad(gameObject);

        Application.logMessageReceived += OnReceiveLog;
        TGameFramework.Initialize();
        StartupModules();
        TGameFramework.Instance.InitModules();
    }
    // Start is called before the first frame update
    void Start()
    {
        TGameFramework.Instance.StartModules();
        
    }

    // Update is called once per frame
    void Update()
    {
        TGameFramework.Instance.Update();
    }
    private void LateUpdate()
    {
        TGameFramework.Instance.LateUpdate();
    }
    private void FixedUpdate()
    {
        TGameFramework.Instance.FixedUpdate();
    }
    private void OnDestroy()
    {
        if (activing)
        {
            Application.logMessageReceived -= OnReceiveLog;
            TGameFramework.Instance.Destroy();
        }
    }
    /// <summary>
    /// ��ʼ��ģ��
    /// </summary>
    public void StartupModules()
    {
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();
        //ʹ�÷�������ȡ��ǰʵ��������(�����κ�Type���������)�����й�����Public�����ǹ�����NonPublic���;�̬��Static�����Եķ���
        PropertyInfo[] propertyInfos=GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        Type baseCompType = typeof(BaseGameModule);
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            PropertyInfo property = propertyInfos[i];
            //���ڼ��ĳ�����Ե������Ƿ���Դ�һ���������ͣ�baseCompType��������ֵ
            if (!baseCompType.IsAssignableFrom(property.PropertyType))
                continue;
            //����inherit������ָ���Ƿ������˳�Ա�ļ̳����Բ������ԡ�
            //���Ϊtrue���������ó�Ա�ļ̳��������Ϊfalse���������ֱ�Ӹ��ӵ��ó�Ա������
            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);
            if (attrs.Length == 0)
                continue;
            Component comp= GetComponentInChildren(property.PropertyType);
            if (comp == null)
            {
                Debug.LogError($"Can't Find GameModule:{property.PropertyType}");
                continue;
            }
            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;
            moduleAttr.Module = comp as BaseGameModule;
            moduleAttrs.Add(moduleAttr);
        }
        moduleAttrs.Sort((a,b) => 
        {
            return a.Priority-b.Priority;
        });
        for (int i = 0; i < moduleAttrs.Count; i++)
        {
            TGameFramework.Instance.AddModule(moduleAttrs[i].Module);
        }
    }
    [AttributeUsage(AttributeTargets.Property,Inherited =false,AllowMultiple =false)]
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>
    {
        /// <summary>
        /// ���ȼ�
        /// </summary>
        public int Priority { get;private set; }
        /// <summary>
        /// ģ��
        /// </summary>
        public BaseGameModule Module { get; set; }
        /// <summary>
        /// ���Ӹ����ԲŻᱻ����ģ��
        /// </summary>
        /// <param name="priority">���������ȼ�,��ֵԽСԽ��ִ��</param>
        public ModuleAttribute(int priority)
        {
            Priority = priority;
        }

        int IComparable<ModuleAttribute>.CompareTo(ModuleAttribute other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }
    private void OnReceiveLog(string condition, string stackTrace, LogType type)
    {
#if !UNITY_EDITOR
            if (type == LogType.Exception)
            {
                UnityLog.Fatal($"{condition}\n{stackTrace}");
            }
#endif
    }
}
using Koakuma.Game.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using TGame.Asset;
using TGame.ECS;
using TGame.UI;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Button testBtn;
    /// <summary>
    /// 资源组件
    /// </summary>
    [Module(1)]
    public static AssetModule Asset { get => TGameFramework.Instance.GetModule<AssetModule>(); }
    /// <summary>
    /// 流程组件
    /// </summary>
    [Module(2)]
    public static ProcedureModule Procedure { get => TGameFramework.Instance.GetModule<ProcedureModule>(); }
    [Module(3)]
    public static UIModule UI { get => TGameFramework.Instance.GetModule<UIModule>(); }
    [Module(6)]
    public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }
    [Module(7)]
    public static ECSModule ECS { get => TGameFramework.Instance.GetModule<ECSModule>(); }
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
    async void Start()
    {
        TGameFramework.Instance.StartModules();
        testBtn.onClick.AddListener(async () => 
        {
            GameManager.Message.Subscribe<MessageType.Login>( async (arg)=> 
            {
                Debug.Log("本地消息进行了触发");
            });
           await GameManager.Message.Post<MessageType.Login>(new MessageType.Login());
        });
       await Procedure.StartProcedure();
        await GameManager.Procedure.ChangeProcedure<InitProcedure>();
        GameManager.UI.OpenUI(UIViewID.LoginUI);
        //await Task.Delay(400);
        //GameManager.UI.CloseUI(UIViewID.LoginUI);
        //await Task.Delay(800);
        //GameManager.UI.OpenUI(UIViewID.LoginUI);

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
    /// 初始化模块
    /// </summary>
    public void StartupModules()
    {
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();
        //使用反射来获取当前实例的类型(或者任何Type对象的引用)的所有公共（Public）、非公共（NonPublic）和静态（Static）属性的方法
        PropertyInfo[] propertyInfos=GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        Type baseCompType = typeof(BaseGameModule);
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            PropertyInfo property = propertyInfos[i];
            //用于检查某个属性的类型是否可以从一个基础类型（baseCompType）派生或赋值
            if (!baseCompType.IsAssignableFrom(property.PropertyType))
                continue;
            //这是inherit参数，指定是否搜索此成员的继承链以查找属性。
            //如果为true，则搜索该成员的继承链；如果为false，则仅搜索直接附加到该成员的属性
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
        /// 优先级
        /// </summary>
        public int Priority { get;private set; }
        /// <summary>
        /// 模块
        /// </summary>
        public BaseGameModule Module { get; set; }
        /// <summary>
        /// 添加该特性才会被当作模块
        /// </summary>
        /// <param name="priority">控制器优先级,数值越小越先执行</param>
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

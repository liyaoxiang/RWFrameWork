using Config;
using Koakuma.Game.UI;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using TGame.Asset;
using UnityEngine;
using UnityEngine.UI;

namespace TGame.UI
{
    public partial class UIModule : BaseGameModule
    {
        public Transform normalUIRoot;
        public Transform modalUIRoot;
        public Transform closeUIRoot;
        public Image imgMask;
        //public QuantumConsole prefabQuantumConsole;

        private static Dictionary<UIViewID, Type> MEDIATOR_MAPPING;
        private static Dictionary<UIViewID, Type> ASSET_MAPPING;

        private readonly List<UIMediator> usingMediators = new List<UIMediator>();
        private readonly Dictionary<Type, Queue<UIMediator>> freeMediators = new Dictionary<Type, Queue<UIMediator>>();
        private readonly GameObjectPool<GameObjectAsset> uiObjectPool = new GameObjectPool<GameObjectAsset>();
        private QuantumConsole quantumConsole;

        protected internal override void OnModuleInit()
        {
            base.OnModuleInit();
            //quantumConsole = Instantiate(prefabQuantumConsole);
            //quantumConsole.transform.SetParentAndResetAll(transform);
            //quantumConsole.OnActivate += OnConsoleActive;
            //quantumConsole.OnDeactivate += OnConsoleDeactive;
            
        }

        protected internal override void OnModuleStop()
        {
            base.OnModuleStop();
            //quantumConsole.OnActivate -= OnConsoleActive;
            //quantumConsole.OnDeactivate -= OnConsoleDeactive;
        }

        private static void CacheUIMapping()
        {
            // 如果 MEDIATOR_MAPPING 已经被初始化，则直接返回
            if (MEDIATOR_MAPPING != null)
                return;

            // 初始化 MEDIATOR_MAPPING 和 ASSET_MAPPING 字典
            MEDIATOR_MAPPING = new Dictionary<UIViewID, Type>();
            ASSET_MAPPING = new Dictionary<UIViewID, Type>();

            // 获取 UIView 类型
            Type baseViewType = typeof(UIView);
            // 遍历程序集中的所有类型
            foreach (var type in baseViewType.Assembly.GetTypes())
            {
                // 如果类型是抽象的，则跳过
                if (type.IsAbstract)
                    continue;

                // 如果类型是 UIView 的派生类型
                if (baseViewType.IsAssignableFrom(type))
                {
                    // 获取类型上的 UIViewAttribute 特性
                    object[] attrs = type.GetCustomAttributes(typeof(UIViewAttribute), false);
                    // 如果没有找到特性，则记录错误日志并跳过
                    if (attrs.Length == 0)
                    {
                        Debug.LogError($"{type.FullName} 没有绑定 Mediator，请使用 UIMediatorAttribute 绑定一个 Mediator 以正确使用");
                        continue;
                    }

                    // 遍历所有找到的 UIViewAttribute 特性
                    foreach (UIViewAttribute attr in attrs)
                    {
                        // 将特性的 ID 和 MediatorType 添加到 MEDIATOR_MAPPING 中
                        MEDIATOR_MAPPING.Add(attr.ID, attr.MediatorType);
                        // 将特性的 ID 和类型添加到 ASSET_MAPPING 中
                        ASSET_MAPPING.Add(attr.ID, type);
                        // 只处理第一个特性，跳出循环
                        break;
                    }
                }
            }
        }


        protected internal override void OnModuleUpdate(float deltaTime)
        {
            base.OnModuleUpdate(deltaTime);
            uiObjectPool.UpdateLoadRequests();
            foreach (var mediator in usingMediators)
            {
                mediator.Update(deltaTime);
            }
            UpdateMask(deltaTime);
        }

        private void OnConsoleActive()
        {
            //GameManager.Input.SetEnable(false);
        }

        private void OnConsoleDeactive()
        {
            //GameManager.Input.SetEnable(true);
        }

        private int GetTopMediatorSortingOrder(UIMode mode)
        {
            // 初始化最后一个指定模式的中介索引为 -1
            int lastIndexMediatorOfMode = -1;

            // 从后向前遍历 usingMediators 列表
            for (int i = usingMediators.Count - 1; i >= 0; i--)
            {
                // 获取当前遍历的中介对象
                UIMediator mediator = usingMediators[i];

                // 如果中介对象的 UIMode 与指定模式不匹配，则继续下一次循环
                if (mediator.UIMode != mode)
                    continue;

                // 更新最后一个指定模式的中介索引
                lastIndexMediatorOfMode = i;
                // 找到符合条件的中介后，跳出循环
                break;
            }

            // 如果未找到符合条件的中介
            if (lastIndexMediatorOfMode == -1)
                // 如果模式为 Normal，返回排序值 0，否则返回 1000
                return mode == UIMode.Normal ? 0 : 1000;

            // 返回找到的最后一个指定模式中介的排序值
            return usingMediators[lastIndexMediatorOfMode].SortingOrder;
        }


        private UIMediator GetMediator(UIViewID id)
        {
            // 调用 CacheUIMapping 方法以确保映射已缓存
            CacheUIMapping();

            // 尝试从 MEDIATOR_MAPPING 中获取指定 id 对应的 Mediator 类型
            if (!MEDIATOR_MAPPING.TryGetValue(id, out Type mediatorType))
            {
                // 如果找不到对应的 Mediator 类型，记录错误日志并返回 null
                Debug.LogError($"找不到 {id} 对应的 Mediator");
                return null;
            }

            // 尝试从 freeMediators 中获取指定 Mediator 类型的队列
            if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ))
            {
                // 如果找不到对应的队列，则创建一个新的队列并添加到 freeMediators 中
                mediatorQ = new Queue<UIMediator>();
                freeMediators.Add(mediatorType, mediatorQ);
            }

            UIMediator mediator;
            // 如果队列为空，创建一个新的 Mediator 实例
            if (mediatorQ.Count == 0)
            {
                mediator = Activator.CreateInstance(mediatorType) as UIMediator;
            }
            else
            {
                // 否则，从队列中取出一个 Mediator 实例
                mediator = mediatorQ.Dequeue();
            }

            // 返回获取到的 Mediator 实例
            return mediator;
        }


        private void RecycleMediator(UIMediator mediator)
        {
            // 如果 mediator 为 null，则直接返回
            if (mediator == null)
                return;

            // 获取 mediator 的类型
            Type mediatorType = mediator.GetType();
            // 尝试从 freeMediators 中获取指定类型的队列
            if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ))
            {
                // 如果找不到对应的队列，则创建一个新的队列并添加到 freeMediators 中
                mediatorQ = new Queue<UIMediator>();
                freeMediators.Add(mediatorType, mediatorQ);
            }
            // 将 mediator 添加到队列中
            mediatorQ.Enqueue(mediator);
        }


        public UIMediator GetOpeningUIMediator(UIViewID id)
        {
            UIConfig uiConfig = UIConfig.ByID((int)id);
            if (uiConfig.IsNull)
                return null;

            UIMediator mediator = GetMediator(id);
            if (mediator == null)
                return null;

            Type requiredMediatorType = mediator.GetType();
            foreach (var item in usingMediators)
            {
                if (item.GetType() == requiredMediatorType)
                    return item;
            }
            return null;
        }

        public void BringToTop(UIViewID id)
        {
            // 获取正在打开的指定 id 对应的 UIMediator
            UIMediator mediator = GetOpeningUIMediator(id);
            // 如果 mediator 为空，则直接返回
            if (mediator == null)
                return;

            // 获取指定 UIMode 的最高排序顺序
            int topSortingOrder = GetTopMediatorSortingOrder(mediator.UIMode);
            // 如果 mediator 的排序顺序已经是最高，则直接返回
            if (mediator.SortingOrder == topSortingOrder)
                return;

            // 设置新的排序顺序，比当前最高排序顺序高 10
            int sortingOrder = topSortingOrder + 10;
            mediator.SortingOrder = sortingOrder;

            // 从 usingMediators 中移除 mediator 并重新添加到末尾
            usingMediators.Remove(mediator);
            usingMediators.Add(mediator);

            // 获取 mediator 的视图对象的 Canvas 组件
            Canvas canvas = mediator.ViewObject.GetComponent<Canvas>();
            // 如果 canvas 不为空，则更新其排序顺序
            if (canvas != null)
            {
                canvas.sortingOrder = sortingOrder;
            }
        }


        public bool IsUIOpened(UIViewID id)
        {
            return GetOpeningUIMediator(id) != null;
        }

        public UIMediator OpenUISingle(UIViewID id, object arg = null)
        {
            UIMediator mediator = GetOpeningUIMediator(id);
            if (mediator != null)
                return mediator;

            return OpenUI(id, arg);
        }

        public UIMediator OpenUI(UIViewID id, object arg = null)
        {
            // 通过 id 获取 UI 配置
            UIConfig uiConfig = UIConfig.ByID((int)id);
            // 如果 UI 配置为空，则返回 null
            if (uiConfig.IsNull)
                return null;

            // 获取指定 id 对应的 UIMediator
            UIMediator mediator = GetMediator(id);
            // 如果 mediator 为空，则返回 null
            if (mediator == null)
                return null;

            // 从对象池中加载 UI 对象
            GameObject uiObject = (uiObjectPool.LoadGameObject(uiConfig.Asset, (obj) =>
            {
                // 获取加载的对象的 UIView 组件
                UIView newView = obj.GetComponent<UIView>();
                // 初始化 mediator
                mediator.InitMediator(newView);
            })).gameObject;

            // 调用 OnUIObjectLoaded 方法处理加载完成的 UI 对象，并返回 mediator
            return OnUIObjectLoaded(mediator, uiConfig, uiObject, arg);
        }


        public IEnumerator OpenUISingleAsync(UIViewID id, object arg = null)
        {
            if (!IsUIOpened(id))
            {
                yield return OpenUIAsync(id, arg);
            }
        }

        public IEnumerator OpenUIAsync(UIViewID id, object arg = null)
        {
            UIConfig uiConfig = UIConfig.ByID((int)id);
            if (uiConfig.IsNull)
                yield break;

            UIMediator mediator = GetMediator(id);
            if (mediator == null)
                yield break;

            bool loadFinish = false;
            uiObjectPool.LoadGameObjectAsync(uiConfig.Asset, (asset) =>
            {
                GameObject uiObject = asset.gameObject;
                OnUIObjectLoaded(mediator, uiConfig, uiObject, arg);
                loadFinish = true;
            }, (obj) =>
            {
                UIView newView = obj.GetComponent<UIView>();
                mediator.InitMediator(newView);
            });
            while (!loadFinish)
            {
                yield return null;
            }
            yield return null;
            yield return null;
        }

        private UIMediator OnUIObjectLoaded(UIMediator mediator, UIConfig uiConfig, GameObject uiObject, object obj)
        {
            // 如果 uiObject 为空，记录错误日志并回收 mediator，返回 null
            if (uiObject == null)
            {
                Debug.LogError($"加载UI失败: {uiConfig.Asset}");
                RecycleMediator(mediator);
                return null;
            }

            // 获取 uiObject 上的 UIView 组件
            UIView view = uiObject.GetComponent<UIView>();
            // 如果 view 为空，记录错误日志，回收 mediator，并卸载 uiObject，返回 null
            if (view == null)
            {
                Debug.LogError($"UI Prefab不包含UIView脚本: {uiConfig.Asset}");
                RecycleMediator(mediator);
                uiObjectPool.UnloadGameObject(view.gameObject);
                return null;
            }

            // 设置 mediator 的 UIMode 为 uiConfig 中的模式
            mediator.UIMode = uiConfig.Mode;
            // 获取当前模式的最高排序顺序，并加 10
            int sortingOrder = GetTopMediatorSortingOrder(uiConfig.Mode) + 10;

            // 将 mediator 添加到使用中的 mediator 列表中
            usingMediators.Add(mediator);

            // 获取 uiObject 上的 Canvas 组件
            Canvas canvas = uiObject.GetComponent<Canvas>();
            // 设置 Canvas 的渲染模式为屏幕空间摄像机
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            //canvas.worldCamera = GameManager.Camera.uiCamera; // 设置世界摄像机

            // 根据 UI 模式设置父对象和排序层
            if (uiConfig.Mode == UIMode.Normal)
            {
                uiObject.transform.SetParentAndResetAll(normalUIRoot);
                canvas.sortingLayerName = "NormalUI";
            }
            else
            {
                uiObject.transform.SetParentAndResetAll(modalUIRoot);
                canvas.sortingLayerName = "ModalUI";
            }

            // 设置 mediator 和 Canvas 的排序顺序
            mediator.SortingOrder = sortingOrder;
            canvas.sortingOrder = sortingOrder;

            // 激活 uiObject
            uiObject.SetActive(true);
            // 显示 mediator，并传递参数
            mediator.Show(uiObject, obj);

            // 返回 mediator
            return mediator;
        }


        public void CloseUI(UIMediator mediator)
        {
            // 如果 mediator 不为 null
            if (mediator != null)
            {
                // 回收视图对象
                uiObjectPool.UnloadGameObject(mediator.ViewObject);
                // 将视图对象的父级设置为 closeUIRoot，并重置其所有变换属性
                mediator.ViewObject.transform.SetParentAndResetAll(closeUIRoot);

                // 隐藏 mediator
                mediator.Hide();
                // 回收 mediator
                RecycleMediator(mediator);

                // 从使用中的 mediator 列表中移除该 mediator
                usingMediators.Remove(mediator);
            }
        }


        public void CloseAllUI()
        {
            for (int i = usingMediators.Count - 1; i >= 0; i--)
            {
                CloseUI(usingMediators[i]);
            }
        }

        public void CloseUI(UIViewID id)
        {
            UIMediator mediator = GetOpeningUIMediator(id);
            if (mediator == null)
                return;

            CloseUI(mediator);
        }

        public void SetAllNormalUIVisibility(bool visible)
        {
            normalUIRoot.gameObject.SetActive(visible);
        }

        public void SetAllModalUIVisibility(bool visible)
        {
            modalUIRoot.gameObject.SetActive(visible);
        }

        public void ShowMask(float duration = 0.5f)
        {
            destMaskAlpha = 1;
            maskDuration = duration;
        }

        public void HideMask(float? duration = null)
        {
            destMaskAlpha = 0;
            if (duration.HasValue)
            {
                maskDuration = duration.Value;
            }
        }

        private float destMaskAlpha = 0;
        private float maskDuration = 0;
        private void UpdateMask(float deltaTime)
        {
            Color c = imgMask.color;
            c.a = maskDuration > 0 ? Mathf.MoveTowards(c.a, destMaskAlpha, 1f / maskDuration * deltaTime) : destMaskAlpha;
            c.a = Mathf.Clamp01(c.a);
            imgMask.color = c;
            imgMask.enabled = imgMask.color.a > 0;
        }

        public void ShowConsole()
        {
            quantumConsole.Activate();
        }
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class UIViewAttribute : Attribute
{
    public UIViewID ID { get; }
    public Type MediatorType { get; }

    public UIViewAttribute(Type mediatorType, UIViewID id)
    {
        ID = id;
        MediatorType = mediatorType;
    }
}

using System;
using System.Collections.Generic;

namespace TGame.ECS
{
    //IDisposable:用于显式释放非托管资源的接口
    public class ECSEntity : IDisposable
    {
        public long InstanceID { get; private set; }
        public long ParentID { get; private set; }
        public bool Disposed { get; private set; }

        public ECSEntity Parent
        {
            get
            {
                if (ParentID == 0)
                    return default;

                return TGameFramework.Instance.GetModule<ECSModule>().FindEntity(ParentID);
            }
        }

        public long SceneID { get; set; }
        public ECSScene Scene
        {
            get
            {
                if (SceneID == 0)
                    return default;

                return TGameFramework.Instance.GetModule<ECSModule>().FindEntity(SceneID) as ECSScene;
            }
        }

        private List<ECSEntity> children = new List<ECSEntity>();
        private Dictionary<Type, ECSComponent> componentMap = new Dictionary<Type, ECSComponent>();

        public ECSEntity()
        {
            InstanceID = IDGenerator.NewInstanceID();
            TGameFramework.Instance.GetModule<ECSModule>().AddEntity(this);
        }

        public virtual void Dispose()
        {
            if (Disposed)
                return;

            Disposed = true;
            // 销毁Child
            for (int i = children.Count - 1; i >= 0; i--)
            {
                ECSEntity child = children[i];
                children.RemoveAt(i);
                child?.Dispose();
            }

            // 销毁Component
            List<ECSComponent> componentList = ListPool<ECSComponent>.Obtain();
            foreach (var component in componentMap.Values)
            {
                componentList.Add(component);
            }

            foreach (var component in componentList)
            {
                componentMap.Remove(component.GetType());
                TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent(component);
            }
            ListPool<ECSComponent>.Release(componentList);

            // 从父节点移除
            Parent?.RemoveChild(this);
            // 从世界中移除
            TGameFramework.Instance.GetModule<ECSModule>().RemoveEntity(this);
        }

        public bool HasComponent<C>() where C : ECSComponent
        {
            // 检查 componentMap 是否包含类型 C 的组件
            return componentMap.ContainsKey(typeof(C));
        }

        public C GetComponent<C>() where C : ECSComponent
        {
            // 尝试从 componentMap 中获取类型为 C 的组件
            componentMap.TryGetValue(typeof(C), out var component);
            // 将获取到的组件转换为 C 类型并返回
            return component as C;
        }

        public C AddNewComponent<C>() where C : ECSComponent, new()
        {
            // 如果已经存在类型为 C 的组件，则移除它
            if (HasComponent<C>())
            {
                RemoveComponent<C>();
            }

            // 创建一个新的 C 类型组件
            C component = new C();
            // 设置组件的实体ID为当前实例ID
            component.EntityID = InstanceID;
            // 将组件添加到 componentMap 中
            componentMap.Add(typeof(C), component);
            // 唤醒组件，进行初始化操作
            TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component);
            // 返回新创建的组件
            return component;
        }

        public C AddNewComponent<C, P1>(P1 p1) where C : ECSComponent, new()
        {
            // 如果已经存在类型为 C 的组件，则移除它
            if (HasComponent<C>())
            {
                RemoveComponent<C>();
            }

            // 创建一个新的 C 类型组件
            C component = new C();
            // 设置组件的实体ID为当前实例ID
            component.EntityID = InstanceID;
            // 将组件添加到 componentMap 中
            componentMap.Add(typeof(C), component);
            // 唤醒组件，使用参数 p1 进行初始化操作
            TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1);
            // 返回新创建的组件
            return component;
        }


        public C AddNewComponent<C, P1, P2>(P1 p1, P2 p2) where C : ECSComponent, new()
        {
            // 如果已经存在类型为 C 的组件，则移除它
            if (HasComponent<C>())
            {
                RemoveComponent<C>();
            }

            // 创建一个新的 C 类型组件
            C component = new C();
            // 设置组件的实体ID为当前实例ID
            component.EntityID = InstanceID;
            // 将组件添加到 componentMap 中
            componentMap.Add(typeof(C), component);
            // 唤醒组件，使用参数 p1 和 p2 进行初始化操作
            TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1, p2);
            // 返回新创建的组件
            return component;
        }

        public C AddComponent<C>() where C : ECSComponent, new()
        {
            // 如果已经存在类型为 C 的组件，记录错误日志并返回默认值
            if (HasComponent<C>())
            {
                UnityLog.Error($"Duplicated Component:{typeof(C).FullName}");
                return default;
            }

            // 创建一个新的 C 类型组件
            C component = new C();
            // 设置组件的实体ID为当前实例ID
            component.EntityID = InstanceID;
            // 将组件添加到 componentMap 中
            componentMap.Add(typeof(C), component);
            // 唤醒组件，进行初始化操作
            TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component);
            // 返回新创建的组件
            return component;
        }


        public C AddComponent<C, P1>(P1 p1) where C : ECSComponent, new()
        {
            if (HasComponent<C>())
            {
                UnityLog.Error($"Duplicated Component:{typeof(C).FullName}");
                return default;
            }

            C component = new C();
            component.EntityID = InstanceID;
            componentMap.Add(typeof(C), component);
            TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1);
            return component;
        }

        public C AddComponent<C, P1, P2>(P1 p1, P2 p2) where C : ECSComponent, new()
        {
            if (HasComponent<C>())
            {
                UnityLog.Error($"Duplicated Component:{typeof(C).FullName}");
                return default;
            }

            C component = new C();
            component.EntityID = InstanceID;
            componentMap.Add(typeof(C), component);
            TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1, p2);
            return component;
        }

        public void RemoveComponent<C>() where C : ECSComponent, new()
        {
            // 获取组件类型
            Type componentType = typeof(C);
            // 尝试从 componentMap 中获取类型为 C 的组件，如果不存在则直接返回
            if (!componentMap.TryGetValue(componentType, out var component))
                return;

            // 从 componentMap 中移除该组件
            componentMap.Remove(componentType);
            // 调用模块中的 DestroyComponent 方法销毁该组件
            TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent((C)component);
        }

        public void RemoveComponent<C, P1>(P1 p1) where C : ECSComponent, new()
        {
            // 获取组件类型
            Type componentType = typeof(C);
            // 尝试从 componentMap 中获取类型为 C 的组件，如果不存在则直接返回
            if (!componentMap.TryGetValue(componentType, out var component))
                return;

            // 从 componentMap 中移除该组件
            componentMap.Remove(componentType);
            // 调用模块中的 DestroyComponent 方法销毁该组件，使用参数 p1
            TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent((C)component, p1);
        }


        public void RemoveComponent<C, P1, P2>(P1 p1, P2 p2) where C : ECSComponent, new()
        {
            Type componentType = typeof(C);
            if (!componentMap.TryGetValue(componentType, out var component))
                return;

            componentMap.Remove(componentType);
            TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent((C)component, p1, p2);
        }

        public void AddChild(ECSEntity child)
        {
            // 如果子实体为 null，则直接返回
            if (child == null)
                return;

            // 如果子实体已被释放，则直接返回
            if (child.Disposed)
                return;

            // 获取子实体的旧父实体
            ECSEntity oldParent = child.Parent;
            // 如果旧父实体不为 null，则从旧父实体中移除该子实体
            if (oldParent != null)
            {
                oldParent.RemoveChild(child);
            }

            // 将子实体添加到当前实体的子实体列表中
            children.Add(child);
            // 设置子实体的父实体ID为当前实例的ID
            child.ParentID = InstanceID;
        }

        public void RemoveChild(ECSEntity child)
        {
            // 如果子实体为 null，则直接返回
            if (child == null)
                return;

            // 从子实体列表中移除该子实体
            children.Remove(child);
            // 将子实体的父实体ID重置为0
            child.ParentID = 0;
        }


        public T FindChild<T>(long id) where T : ECSEntity
        {
            // 遍历所有子实体
            foreach (var child in children)
            {
                // 如果子实体的实例ID与给定ID匹配，则返回该子实体
                if (child.InstanceID == id)
                    return child as T;
            }

            // 如果没有找到匹配的子实体，则返回默认值
            return default;
        }

        public T FindChild<T>(Predicate<T> predicate) where T : ECSEntity
        {
            // 遍历所有子实体
            foreach (var child in children)
            {
                // 将子实体转换为 T 类型
                T c = child as T;
                // 如果转换结果为 null，则继续下一次循环
                if (c == null)
                    continue;

                // 如果子实体满足给定的条件，则返回该子实体
                if (predicate.Invoke(c))
                {
                    return c;
                }
            }

            // 如果没有找到满足条件的子实体，则返回默认值
            return default;
        }


        public void FindChildren<T>(List<T> list) where T : ECSEntity
        {
            // 遍历所有子实体
            foreach (var child in children)
            {
                // 如果子实体是 T 类型
                if (child is T)
                {
                    // 将子实体转换为 T 类型并添加到列表中
                    list.Add(child as T);
                }
            }
        }

    }
}

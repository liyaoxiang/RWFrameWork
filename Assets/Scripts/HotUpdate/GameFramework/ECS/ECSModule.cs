using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace TGame.ECS
{
    public class ECSModule : BaseGameModule
    {
        public ECSWorld World { get; private set; }

        private Dictionary<Type, IAwakeSystem> awakeSystemMap;
        private Dictionary<Type, IDestroySystem> destroySystemMap;

        private Dictionary<Type, IUpdateSystem> updateSystemMap;
        private Dictionary<IUpdateSystem, List<ECSEntity>> updateSystemRelatedEntityMap;

        private Dictionary<Type, ILateUpdateSystem> lateUpdateSystemMap;
        private Dictionary<ILateUpdateSystem, List<ECSEntity>> lateUpdateSystemRelatedEntityMap;

        private Dictionary<Type, IFixedUpdateSystem> fixedUpdateSystemMap;
        private Dictionary<IFixedUpdateSystem, List<ECSEntity>> fixedUpdateSystemRelatedEntityMap;

        private Dictionary<long, ECSEntity> entities = new Dictionary<long, ECSEntity>();
        private Dictionary<Type, List<IEntityMessageHandler>> entityMessageHandlerMap;
        private Dictionary<Type, IEntityRpcHandler> entityRpcHandlerMap;

        protected internal override void OnModuleInit()
        {
            base.OnModuleInit();
            LoadAllSystems();
            World = new ECSWorld();
        }

        protected internal override void OnModuleUpdate(float deltaTime)
        {
            base.OnModuleUpdate(deltaTime);
            DriveUpdateSystem();
        }

        protected internal override void OnModuleLateUpdate(float deltaTime)
        {
            base.OnModuleLateUpdate(deltaTime);
            DriveLateUpdateSystem();
        }

        protected internal override void OnModuleFixedUpdate(float deltaTime)
        {
            base.OnModuleFixedUpdate(deltaTime);
            DriveFixedUpdateSystem();
        }

        public void LoadAllSystems()
        {
            awakeSystemMap = new Dictionary<Type, IAwakeSystem>();
            destroySystemMap = new Dictionary<Type, IDestroySystem>();

            updateSystemMap = new Dictionary<Type, IUpdateSystem>();
            updateSystemRelatedEntityMap = new Dictionary<IUpdateSystem, List<ECSEntity>>();

            lateUpdateSystemMap = new Dictionary<Type, ILateUpdateSystem>();
            lateUpdateSystemRelatedEntityMap = new Dictionary<ILateUpdateSystem, List<ECSEntity>>();

            fixedUpdateSystemMap = new Dictionary<Type, IFixedUpdateSystem>();
            fixedUpdateSystemRelatedEntityMap = new Dictionary<IFixedUpdateSystem, List<ECSEntity>>();

            entityMessageHandlerMap = new Dictionary<Type, List<IEntityMessageHandler>>();
            entityRpcHandlerMap = new Dictionary<Type, IEntityRpcHandler>();

            foreach (var type in Assembly.GetCallingAssembly().GetTypes())
            {
                if (type.IsAbstract)
                    continue;

                if (type.GetCustomAttribute<ECSSystemAttribute>(true) != null)
                {
                    // AwakeSystem
                    // 定义IAwakeSystem接口的类型
                    Type awakeSystemType = typeof(IAwakeSystem);
                    // 检查当前类型是否实现了IAwakeSystem接口
                    if (awakeSystemType.IsAssignableFrom(type))
                    {
                        // 检查字典中是否已经包含该类型的实例
                        if (awakeSystemMap.ContainsKey(type))
                        {
                            // 如果包含，记录错误信息并继续下一个类型
                            UnityLog.Error($"Duplicated Awake System: {type.FullName}");
                            continue;
                        }
                        // 使用反射创建该类型的实例，并强制转换为IAwakeSystem
                        IAwakeSystem awakeSystem = Activator.CreateInstance(type) as IAwakeSystem;
                       // 将类型和实例添加到字典中
                        awakeSystemMap.Add(type, awakeSystem);
                    }

                    // DestroySystem
                    Type destroySystemType = typeof(IDestroySystem);
                    if (destroySystemType.IsAssignableFrom(type))
                    {
                        if (destroySystemMap.ContainsKey(type))
                        {
                            UnityLog.Error($"Duplicated Destroy System:{type.FullName}");
                            continue;
                        }

                        IDestroySystem destroySytem = Activator.CreateInstance(type) as IDestroySystem;
                        destroySystemMap.Add(type, destroySytem);
                    }

                    // UpdateSystem
                    Type updateSystemType = typeof(IUpdateSystem);
                    if (updateSystemType.IsAssignableFrom(type))
                    {
                        if (updateSystemMap.ContainsKey(type))
                        {
                            UnityLog.Error($"Duplicated Update System:{type.FullName}");
                            continue;
                        }

                        IUpdateSystem updateSystem = Activator.CreateInstance(type) as IUpdateSystem;
                        updateSystemMap.Add(type, updateSystem);

                        updateSystemRelatedEntityMap.Add(updateSystem, new List<ECSEntity>());
                    }

                    // LateUpdateSystem
                    Type lateUpdateSystemType = typeof(ILateUpdateSystem);
                    if (lateUpdateSystemType.IsAssignableFrom(type))
                    {
                        if (lateUpdateSystemMap.ContainsKey(type))
                        {
                            UnityLog.Error($"Duplicated Late update System:{type.FullName}");
                            continue;
                        }

                        ILateUpdateSystem lateUpdateSystem = Activator.CreateInstance(type) as ILateUpdateSystem;
                        lateUpdateSystemMap.Add(type, lateUpdateSystem);

                        lateUpdateSystemRelatedEntityMap.Add(lateUpdateSystem, new List<ECSEntity>());
                    }

                    // FixedUpdateSystem
                    Type fixedUpdateSystemType = typeof(IFixedUpdateSystem);
                    if (fixedUpdateSystemType.IsAssignableFrom(type))
                    {
                        if (fixedUpdateSystemMap.ContainsKey(type))
                        {
                            UnityLog.Error($"Duplicated Late update System:{type.FullName}");
                            continue;
                        }

                        IFixedUpdateSystem fixedUpdateSystem = Activator.CreateInstance(type) as IFixedUpdateSystem;
                        fixedUpdateSystemMap.Add(type, fixedUpdateSystem);

                        fixedUpdateSystemRelatedEntityMap.Add(fixedUpdateSystem, new List<ECSEntity>());
                    }
                }

                if (type.GetCustomAttribute<EntityMessageHandlerAttribute>(true) != null)
                {
                    // EntityMessage
                    Type entityMessageType = typeof(IEntityMessageHandler);
                    if (entityMessageType.IsAssignableFrom(type))
                    {
                        // 使用反射创建该类型的实例，并强制转换为IEntityMessageHandler接口
                        IEntityMessageHandler entityMessageHandler = Activator.CreateInstance(type) as IEntityMessageHandler;
                        // 尝试从字典中获取与消息类型对应的处理程序列表
                        if (!entityMessageHandlerMap.TryGetValue(entityMessageHandler.MessageType(), out List<IEntityMessageHandler> list))
                        {
                            // 如果字典中不存在该消息类型的处理程序列表，则创建一个新的列表
                            list = new List<IEntityMessageHandler>();
                            // 将新的处理程序列表添加到字典中，键为消息类型
                            entityMessageHandlerMap.Add(entityMessageHandler.MessageType(), list);
                        }
                        list.Add(entityMessageHandler);
                    }
                }

                if (type.GetCustomAttribute<EntityRpcHandlerAttribute>(true) != null)
                {
                    // EntityRPC
                    // 定义IEntityRpcHandler接口的类型
                    Type entityMessageType = typeof(IEntityRpcHandler);
                    // 检查当前类型是否实现了IEntityRpcHandler接口
                    if (entityMessageType.IsAssignableFrom(type))
                    {
                        // 使用反射创建该类型的实例，并强制转换为IEntityRpcHandler接口
                        IEntityRpcHandler entityRpcHandler = Activator.CreateInstance(type) as IEntityRpcHandler;
                        // 检查字典中是否已经包含该RPC类型的处理程序实例
                        if (entityRpcHandlerMap.ContainsKey(entityRpcHandler.RpcType()))
                        {
                            // 如果字典中已包含该RPC类型的处理程序实例，记录错误信息并继续下一个类型
                            UnityLog.Error($"Duplicate Entity Rpc, type: {entityRpcHandler.RpcType().FullName}");
                            continue;
                        }
                        // 将RPC类型和处理程序实例添加到字典中
                        entityRpcHandlerMap.Add(entityRpcHandler.RpcType(), entityRpcHandler);
                    }

                }
            }
        }

        private void DriveUpdateSystem()
        {
            foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
            {
                // 从字典updateSystemRelatedEntityMap中获取与updateSystem相关的实体列表
                List<ECSEntity> updateSystemRelatedEntities = updateSystemRelatedEntityMap[updateSystem];

                // 如果相关实体列表为空，则跳过本次循环
                if (updateSystemRelatedEntities.Count == 0)
                    continue;
                // 从对象池中获取一个新的实体列表
                List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
                // 将相关实体列表中的所有实体添加到新获取的实体列表中
                entityList.AddRangeNonAlloc(updateSystemRelatedEntities);
                // 遍历新获取的实体列表中的每个实体
                foreach (var entity in entityList)
                {
                    // 如果updateSystem不观察该实体，则跳过当前实体
                    if (!updateSystem.ObservingEntity(entity))
                        continue;
                    // 调用updateSystem的Update方法更新当前实体
                    updateSystem.Update(entity);
                }


                ListPool<ECSEntity>.Release(entityList);
            }
        }

        private void DriveLateUpdateSystem()
        {
            foreach (ILateUpdateSystem lateUpdateSystem in lateUpdateSystemMap.Values)
            {
                List<ECSEntity> lateUpdateSystemRelatedEntities = lateUpdateSystemRelatedEntityMap[lateUpdateSystem];
                if (lateUpdateSystemRelatedEntities.Count == 0)
                    continue;

                List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
                entityList.AddRangeNonAlloc(lateUpdateSystemRelatedEntities);
                foreach (var entity in entityList)
                {
                    if (!lateUpdateSystem.ObservingEntity(entity))
                        continue;

                    lateUpdateSystem.LateUpdate(entity);
                }

                ListPool<ECSEntity>.Release(entityList);
            }
        }

        private void DriveFixedUpdateSystem()
        {
            foreach (IFixedUpdateSystem fixedUpdateSystem in fixedUpdateSystemMap.Values)
            {
                List<ECSEntity> fixedUpdateSystemRelatedEntities = fixedUpdateSystemRelatedEntityMap[fixedUpdateSystem];
                if (fixedUpdateSystemRelatedEntities.Count == 0)
                    continue;

                List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
                entityList.AddRangeNonAlloc(fixedUpdateSystemRelatedEntities);
                foreach (var entity in entityList)
                {
                    if (!fixedUpdateSystem.ObservingEntity(entity))
                        continue;

                    fixedUpdateSystem.FixedUpdate(entity);
                }

                ListPool<ECSEntity>.Release(entityList);
            }
        }

        private void GetAwakeSystems<C>(List<IAwakeSystem> list) where C : ECSComponent
        {
            foreach (var awakeSystem in awakeSystemMap.Values)
            {
                if (awakeSystem.ComponentType() == typeof(C))
                {
                    list.Add(awakeSystem);
                }
            }
        }

        public void AwakeComponent<C>(C component) where C : ECSComponent
        {
            
            UpdateSystemEntityList(component.Entity);

            List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
            GetAwakeSystems<C>(list);

            bool found = false;
            foreach (var item in list)
            {
                AwakeSystem<C> awakeSystem = item as AwakeSystem<C>;
                if (awakeSystem == null)
                    continue;

                awakeSystem.Awake(component);
                found = true;
            }

            ListPool<IAwakeSystem>.Release(list);
            if (!found)
            {
                UnityLog.Warn($"Not found awake system:<{typeof(C).Name}>");
            }
        }

        public void AwakeComponent<C, P1>(C component, P1 p1) where C : ECSComponent
        {
            UpdateSystemEntityList(component.Entity);

            List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
            TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

            bool found = false;
            foreach (var item in list)
            {
                AwakeSystem<C, P1> awakeSystem = item as AwakeSystem<C, P1>;
                if (awakeSystem == null)
                    continue;

                awakeSystem.Awake(component, p1);
                found = true;
            }

            ListPool<IAwakeSystem>.Release(list);
            if (!found)
            {
                UnityLog.Warn($"Not found awake system:<{typeof(C).Name}, {typeof(P1).Name}>");
            }
        }

        public void AwakeComponent<C, P1, P2>(C component, P1 p1, P2 p2) where C : ECSComponent
        {
            UpdateSystemEntityList(component.Entity);

            List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
            TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

            bool found = false;
            foreach (var item in list)
            {
                AwakeSystem<C, P1, P2> awakeSystem = item as AwakeSystem<C, P1, P2>;
                if (awakeSystem == null)
                    continue;

                awakeSystem.Awake(component, p1, p2);
                found = true;
            }

            ListPool<IAwakeSystem>.Release(list);
            if (!found)
            {
                UnityLog.Warn($"Not found awake system:<{typeof(C).Name}, {typeof(P1).Name}, {typeof(P2).Name}>");
            }
        }

        private void GetDestroySystems<C>(List<IDestroySystem> list) where C : ECSComponent
        {
            foreach (var destroySystem in destroySystemMap.Values)
            {
                if (destroySystem.ComponentType() == typeof(C))
                {
                    list.Add(destroySystem);
                }
            }
        }
        private void GetDestroySystems(Type componentType, List<IDestroySystem> list)
        {
            foreach (var destroySystem in destroySystemMap.Values)
            {
                if (destroySystem.ComponentType() == componentType)
                {
                    list.Add(destroySystem);
                }
            }
        }
        public void DestroyComponent<C>(C component) where C : ECSComponent
        {
            UpdateSystemEntityList(component.Entity);

            List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
            GetDestroySystems<C>(list);
            foreach (var item in list)
            {
                DestroySystem<C> destroySystem = item as DestroySystem<C>;
                if (destroySystem == null)
                    continue;

                destroySystem.Destroy(component);
                component.Disposed = true;
            }

            ListPool<IDestroySystem>.Release(list);
        }

        public void DestroyComponent(ECSComponent component)
        {
            UpdateSystemEntityList(component.Entity);

            List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
            GetDestroySystems(component.GetType(), list);
            foreach (var item in list)
            {
                item.Destroy(component);
                component.Disposed = true;
            }

            ListPool<IDestroySystem>.Release(list);
        }

        public void DestroyComponent<C, P1>(C component, P1 p1) where C : ECSComponent
        {
            UpdateSystemEntityList(component.Entity);

            List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
            GetDestroySystems<C>(list);
            foreach (var item in list)
            {
                DestroySystem<C, P1> destroySystem = item as DestroySystem<C, P1>;
                if (destroySystem == null)
                    continue;

                destroySystem.Destroy(component, p1);
                component.Disposed = true;
            }

            ListPool<IDestroySystem>.Release(list);
        }

        public void DestroyComponent<C, P1, P2>(C component, P1 p1, P2 p2) where C : ECSComponent
        {
            UpdateSystemEntityList(component.Entity);

            List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
            GetDestroySystems<C>(list);
            foreach (var item in list)
            {
                DestroySystem<C, P1, P2> destroySystem = item as DestroySystem<C, P1, P2>;
                if (destroySystem == null)
                    continue;

                destroySystem.Destroy(component, p1, p2);
                component.Disposed = true;
            }

            ListPool<IDestroySystem>.Release(list);
        }

        private void UpdateSystemEntityList(ECSEntity entity)
        {
            foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
            {
                // update entity list
                // 从字典updateSystemRelatedEntityMap中获取与updateSystem相关的实体列表
                List<ECSEntity> entityList = updateSystemRelatedEntityMap[updateSystem];
                // 检查实体列表是否包含指定的实体
                if (!entityList.Contains(entity))
                {
                    // 如果实体列表不包含该实体，检查含有IUpdateSystem组件
                    if (updateSystem.ObservingEntity(entity))
                    {
                        // 如果含有将其添加到实体列表中
                        entityList.Add(entity);
                    }
                }
                else
                {
                    // 如果实体列表包含该实体，检查含有IUpdateSystem组件
                    if (!updateSystem.ObservingEntity(entity))
                    {
                        // 如果没有，将其从实体列表中移除
                        entityList.Remove(entity);
                    }
                }

            }

            foreach (ILateUpdateSystem lateUpdateSystem in lateUpdateSystemMap.Values)
            {
                // update entity list
                List<ECSEntity> entityList = lateUpdateSystemRelatedEntityMap[lateUpdateSystem];
                if (!entityList.Contains(entity))
                {
                    if (lateUpdateSystem.ObservingEntity(entity))
                    {
                        entityList.Add(entity);
                    }
                }
                else
                {
                    if (!lateUpdateSystem.ObservingEntity(entity))
                    {
                        entityList.Remove(entity);
                    }
                }
            }

            foreach (IFixedUpdateSystem fixedUpdateSystem in fixedUpdateSystemMap.Values)
            {
                // update entity list
                List<ECSEntity> entityList = fixedUpdateSystemRelatedEntityMap[fixedUpdateSystem];
                if (!entityList.Contains(entity))
                {
                    if (fixedUpdateSystem.ObservingEntity(entity))
                    {
                        entityList.Add(entity);
                    }
                }
                else
                {
                    if (!fixedUpdateSystem.ObservingEntity(entity))
                    {
                        entityList.Remove(entity);
                    }
                }
            }
        }

        public void AddEntity(ECSEntity entity)
        {
            entities.Add(entity.InstanceID, entity);
        }

        public void RemoveEntity(ECSEntity entity)
        {
            if (entity == null)
                return;

            entities.Remove(entity.InstanceID);
            ECSScene scene = entity.Scene;
            //将实体从场景中移除
            scene?.RemoveEntity(entity.InstanceID);
        }

        public ECSEntity FindEntity(long id)
        {
            return FindEntity<ECSEntity>(id);
        }

        public T FindEntity<T>(long id) where T : ECSEntity
        {
            entities.TryGetValue(id, out ECSEntity entity);
            return entity as T;
        }

        public T FindComponentOfEntity<T>(long entityID) where T : ECSComponent
        {
            return FindEntity(entityID)?.GetComponent<T>();
        }

        public async Task SendMessageToEntity<M>(long id, M m)
        {
            if (id == 0)
                return;

            ECSEntity entity = FindEntity(id);
            if (entity == null)
                return;

            Type messageType = m.GetType();
            if (!entityMessageHandlerMap.TryGetValue(messageType, out List<IEntityMessageHandler> list))
                return;

            List<IEntityMessageHandler> entityMessageHandlers = ListPool<IEntityMessageHandler>.Obtain();
            entityMessageHandlers.AddRangeNonAlloc(list);
            foreach (IEntityMessageHandler<M> handler in entityMessageHandlers)
            {
                await handler.Post(entity, m);
            }

            ListPool<IEntityMessageHandler>.Release(entityMessageHandlers);
        }

        public async Task<Response> SendRpcToEntity<Request, Response>(long entityID, Request request) where Response : IEntityRpcResponse, new()
        {
            if (entityID == 0)
                return new Response() { Error = true };

            ECSEntity entity = FindEntity(entityID);
            if (entity == null)
                return new Response() { Error = true };

            Type messageType = request.GetType();
            if (!entityRpcHandlerMap.TryGetValue(messageType, out IEntityRpcHandler entityRpcHandler))
                return new Response() { Error = true };

            // 将entityRpcHandler强制转换为IEntityRpcHandler<Request, Response>类型的处理程序
            IEntityRpcHandler<Request, Response> handler = entityRpcHandler as IEntityRpcHandler<Request, Response>;
            if (handler == null)
                return new Response() { Error = true };

            return await handler.Post(entity, request);
        }
    }
}

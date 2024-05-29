using System.Collections.Generic;

namespace TGame.ECS
{
    public class ECSScene : ECSEntity
    {
        private Dictionary<long, ECSEntity> entities;

        public ECSScene()
        {
            entities = new Dictionary<long, ECSEntity>();
        }

        public override void Dispose()
        {
            // 如果已经被释放，则直接返回
            if (Disposed)
                return;

            // 从对象池中获取一个新的 long 类型列表
            List<long> entityIDList = ListPool<long>.Obtain();

            // 遍历所有实体的键（ID）
            foreach (var entityID in entities.Keys)
            {
                // 将每个实体ID添加到列表中
                entityIDList.Add(entityID);
            }

            // 遍历所有收集到的实体ID
            foreach (var entityID in entityIDList)
            {
                // 获取实体对象
                ECSEntity entity = entities[entityID];
                // 释放实体
                entity.Dispose();
            }

            // 将使用过的列表释放回对象池
            ListPool<long>.Release(entityIDList);

            // 调用基类的 Dispose 方法
            base.Dispose();
        }


        public void AddEntity(ECSEntity entity)
        {
            // 如果实体为 null，则直接返回
            if (entity == null)
                return;

            // 获取实体的旧场景
            ECSScene oldScene = entity.Scene;
            // 如果旧场景不为 null，则从旧场景中移除该实体
            if (oldScene != null)
            {
                oldScene.RemoveEntity(entity.InstanceID);
            }

            // 将实体添加到当前场景的实体集合中
            entities.Add(entity.InstanceID, entity);
            // 设置实体的场景ID为当前实例的ID
            entity.SceneID = InstanceID;
            // 记录日志，输出当前实体数量
            UnityLog.Info($"Scene Add Entity, Current Count:{entities.Count}");
        }

        public void RemoveEntity(long entityID)
        {
            // 如果成功从实体集合中移除该实体
            if (entities.Remove(entityID))
            {
                // 记录日志，输出当前实体数量
                UnityLog.Info($"Scene Remove Entity, Current Count:{entities.Count}");
            }
        }


        public void FindEntities<T>(List<long> list) where T : ECSEntity
        {
            // 遍历所有实体
            foreach (var item in entities)
            {
                // 如果实体是 T 类型
                if (item.Value is T)
                {
                    // 将实体的键（ID）添加到列表中
                    list.Add(item.Key);
                }
            }
        }

        public void FindEntitiesWithComponent<T>(List<long> list) where T : ECSComponent
        {
            // 遍历所有实体
            foreach (var item in entities)
            {
                // 如果实体包含 T 类型的组件
                if (item.Value.HasComponent<T>())
                {
                    // 将实体的键（ID）添加到列表中
                    list.Add(item.Key);
                }
            }
        }


        public void GetAllEntities(List<long> list)
        {
            foreach (var item in entities)
            {
                list.Add(item.Key);
            }
        }
    }
}
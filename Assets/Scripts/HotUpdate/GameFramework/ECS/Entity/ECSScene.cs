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
            // ����Ѿ����ͷţ���ֱ�ӷ���
            if (Disposed)
                return;

            // �Ӷ�����л�ȡһ���µ� long �����б�
            List<long> entityIDList = ListPool<long>.Obtain();

            // ��������ʵ��ļ���ID��
            foreach (var entityID in entities.Keys)
            {
                // ��ÿ��ʵ��ID��ӵ��б���
                entityIDList.Add(entityID);
            }

            // ���������ռ�����ʵ��ID
            foreach (var entityID in entityIDList)
            {
                // ��ȡʵ�����
                ECSEntity entity = entities[entityID];
                // �ͷ�ʵ��
                entity.Dispose();
            }

            // ��ʹ�ù����б��ͷŻض����
            ListPool<long>.Release(entityIDList);

            // ���û���� Dispose ����
            base.Dispose();
        }


        public void AddEntity(ECSEntity entity)
        {
            // ���ʵ��Ϊ null����ֱ�ӷ���
            if (entity == null)
                return;

            // ��ȡʵ��ľɳ���
            ECSScene oldScene = entity.Scene;
            // ����ɳ�����Ϊ null����Ӿɳ������Ƴ���ʵ��
            if (oldScene != null)
            {
                oldScene.RemoveEntity(entity.InstanceID);
            }

            // ��ʵ����ӵ���ǰ������ʵ�弯����
            entities.Add(entity.InstanceID, entity);
            // ����ʵ��ĳ���IDΪ��ǰʵ����ID
            entity.SceneID = InstanceID;
            // ��¼��־�������ǰʵ������
            UnityLog.Info($"Scene Add Entity, Current Count:{entities.Count}");
        }

        public void RemoveEntity(long entityID)
        {
            // ����ɹ���ʵ�弯�����Ƴ���ʵ��
            if (entities.Remove(entityID))
            {
                // ��¼��־�������ǰʵ������
                UnityLog.Info($"Scene Remove Entity, Current Count:{entities.Count}");
            }
        }


        public void FindEntities<T>(List<long> list) where T : ECSEntity
        {
            // ��������ʵ��
            foreach (var item in entities)
            {
                // ���ʵ���� T ����
                if (item.Value is T)
                {
                    // ��ʵ��ļ���ID����ӵ��б���
                    list.Add(item.Key);
                }
            }
        }

        public void FindEntitiesWithComponent<T>(List<long> list) where T : ECSComponent
        {
            // ��������ʵ��
            foreach (var item in entities)
            {
                // ���ʵ����� T ���͵����
                if (item.Value.HasComponent<T>())
                {
                    // ��ʵ��ļ���ID����ӵ��б���
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
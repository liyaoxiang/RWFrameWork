using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
namespace Config
{
    public partial struct EquipConfig
    {
        public static void DeserializeByAddressable(string directory)
        {
            string path = $"{directory}/EquipConfig.json";
            UnityEngine.TextAsset ta = Addressables.LoadAssetAsync<UnityEngine.TextAsset>(path).WaitForCompletion();
            string json = ta.text;
            datas = new List<EquipConfig>();
            indexMap = new Dictionary<int, int>();
            JArray array = JArray.Parse(json);
            Count = array.Count;
            for (int i = 0; i < array.Count; i++)
            {
                JObject dataObject = array[i] as JObject;
                EquipConfig data = (EquipConfig)dataObject.ToObject(typeof(EquipConfig));
                datas.Add(data);
                indexMap.Add(data.ID, i);
            }
        }
        public static void DeserializeByFile(string directory)
        {
            string path = $"{directory}/EquipConfig.json";
            using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(fs))
                {
                    datas = new List<EquipConfig>();
                    indexMap = new Dictionary<int, int>();
                    string json = reader.ReadToEnd();
                    JArray array = JArray.Parse(json);
                    Count = array.Count;
                    for (int i = 0; i < array.Count; i++)
                    {
                        JObject dataObject = array[i] as JObject;
                        EquipConfig data = (EquipConfig)dataObject.ToObject(typeof(EquipConfig));
                        datas.Add(data);
                        indexMap.Add(data.ID, i);
                    }
                }
            }
        }
        public static System.Collections.IEnumerator DeserializeByBundle(string directory, string subFolder)
        {
            string bundleName = $"{subFolder}/EquipConfig.bytes".ToLower();
            string fullBundleName = $"{directory}/{bundleName}";
            string assetName = $"assets/{bundleName}";
            #if UNITY_WEBGL && !UNITY_EDITOR
            UnityEngine.AssetBundle bundle = null;
            UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(fullBundleName);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                UnityEngine.Debug.LogError(request.error);
            }
            else
            {
                bundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
            }
            #else
            yield return null;
            UnityEngine.AssetBundle bundle = UnityEngine.AssetBundle.LoadFromFile($"{fullBundleName}", 0, 0);
            #endif
            UnityEngine.TextAsset ta = bundle.LoadAsset<UnityEngine.TextAsset>($"{assetName}");
            string json = ta.text;
            datas = new List<EquipConfig>();
            indexMap = new Dictionary<int, int>();
            JArray array = JArray.Parse(json);
            Count = array.Count;
            for (int i = 0; i < array.Count; i++)
            {
                JObject dataObject = array[i] as JObject;
                EquipConfig data = (EquipConfig)dataObject.ToObject(typeof(EquipConfig));
                datas.Add(data);
                indexMap.Add(data.ID, i);
            }
        }
        public static int Count;
        private static List<EquipConfig> datas;
        private static Dictionary<int, int> indexMap;
        public static EquipConfig ByID(int id)
        {
            if (id <= 0)
            {
                return Null;
            }
            if (!indexMap.TryGetValue(id, out int index))
            {
                throw new System.Exception($"EquipConfig找不到ID:{id}");
            }
            return ByIndex(index);
        }
        public static EquipConfig ByIndex(int index)
        {
            return datas[index];
        }
        public bool IsNull { get; private set; }
        public static EquipConfig Null { get; } = new EquipConfig() { IsNull = true }; 
        public System.Int32 ID { get; set; }
        public string Description { get; set; }
        public string name { get; set; }
        public System.Int32 search_type { get; set; }
        public System.Int32 color { get; set; }
        public System.Int32 isbind { get; set; }
        public System.Int32 candiscard { get; set; }
        public System.Int32 cansell { get; set; }
        public System.Int32 market_cansell { get; set; }
        public System.Int32 sellprice { get; set; }
        public System.Int32 recycltype { get; set; }
        public System.Int32 recyclget { get; set; }
        public System.Int32 isbroadcast { get; set; }
        public System.Int32 pile_limit { get; set; }
        public System.Int32 isdroprecord { get; set; }
        public System.Int32 time_length { get; set; }
        public System.Int32 invalid_time { get; set; }
        public System.Int32 limit_prof { get; set; }
        public System.Int32 limit_sex { get; set; }
        public System.Int32 limit_level { get; set; }
        public System.Int32 equip_level { get; set; }
        public System.Int32 order { get; set; }
        public System.Int32 quality { get; set; }
        public System.Int32 mp { get; set; }
        public System.Int32 hp { get; set; }
        public System.Int32 attack { get; set; }
        public System.Int32 fangyu { get; set; }
        public System.Int32 mingzhong { get; set; }
        public System.Int32 shanbi { get; set; }
        public System.Int32 baoji { get; set; }
        public string jianren { get; set; }
        public string ignore_fangyu { get; set; }
        public System.Int32 hurt_increase { get; set; }
        public System.Int32 hurt_reduce { get; set; }
        public System.Int32 per_jingzhun { get; set; }
        public System.Int32 per_baoji { get; set; }
        public string per_pofang { get; set; }
        public System.Int32 per_mianshang { get; set; }
        public System.Int32 per_pvp_hurt_increase { get; set; }
        public System.Int32 per_pvp_hurt_reduce { get; set; }
        public System.Int32 can_strengthen { get; set; }
        public System.Int32 can_flush { get; set; }
        public System.Int32 hole_num { get; set; }
        public System.Int32 suit_id { get; set; }
        public System.Int32 value5 { get; set; }
        public System.Int32 shen { get; set; }
        public System.Int32 other_sex_itemid { get; set; }
        public System.Int32 appe_type { get; set; }
        public System.Int32 icon_id { get; set; }
        public System.Int32 is_tip_use { get; set; }
        public System.Int32 show_id { get; set; }
        public System.Int32 click_use { get; set; }
        public System.Int32 sub_type { get; set; }
        public System.Int32 drop_icon { get; set; }
        public System.Int32 bag_type { get; set; }
        public System.Int32 is_display_role { get; set; }
        public System.Int32 guild_storage_score { get; set; }
        public System.Int32 is_rare { get; set; }
        public System.Int32 show_level { get; set; }
        public System.Int32 special_show { get; set; }
        public System.Int32 is_texiao { get; set; }
        public System.Int32 rarefloating { get; set; }
        public System.Int32 shield_button { get; set; }
    }
}

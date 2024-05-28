using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
namespace Config
{
    public partial struct InitiativeConfig
    {
        public static void DeserializeByAddressable(string directory)
        {
            string path = $"{directory}/InitiativeConfig.json";
            UnityEngine.TextAsset ta = Addressables.LoadAssetAsync<UnityEngine.TextAsset>(path).WaitForCompletion();
            string json = ta.text;
            datas = new List<InitiativeConfig>();
            indexMap = new Dictionary<int, int>();
            JArray array = JArray.Parse(json);
            Count = array.Count;
            for (int i = 0; i < array.Count; i++)
            {
                JObject dataObject = array[i] as JObject;
                InitiativeConfig data = (InitiativeConfig)dataObject.ToObject(typeof(InitiativeConfig));
                datas.Add(data);
                indexMap.Add(data.ID, i);
            }
        }
        public static void DeserializeByFile(string directory)
        {
            string path = $"{directory}/InitiativeConfig.json";
            using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(fs))
                {
                    datas = new List<InitiativeConfig>();
                    indexMap = new Dictionary<int, int>();
                    string json = reader.ReadToEnd();
                    JArray array = JArray.Parse(json);
                    Count = array.Count;
                    for (int i = 0; i < array.Count; i++)
                    {
                        JObject dataObject = array[i] as JObject;
                        InitiativeConfig data = (InitiativeConfig)dataObject.ToObject(typeof(InitiativeConfig));
                        datas.Add(data);
                        indexMap.Add(data.ID, i);
                    }
                }
            }
        }
        public static System.Collections.IEnumerator DeserializeByBundle(string directory, string subFolder)
        {
            string bundleName = $"{subFolder}/InitiativeConfig.bytes".ToLower();
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
            datas = new List<InitiativeConfig>();
            indexMap = new Dictionary<int, int>();
            JArray array = JArray.Parse(json);
            Count = array.Count;
            for (int i = 0; i < array.Count; i++)
            {
                JObject dataObject = array[i] as JObject;
                InitiativeConfig data = (InitiativeConfig)dataObject.ToObject(typeof(InitiativeConfig));
                datas.Add(data);
                indexMap.Add(data.ID, i);
            }
        }
        public static int Count;
        private static List<InitiativeConfig> datas;
        private static Dictionary<int, int> indexMap;
        public static InitiativeConfig ByID(int id)
        {
            if (id <= 0)
            {
                return Null;
            }
            if (!indexMap.TryGetValue(id, out int index))
            {
                throw new System.Exception($"InitiativeConfig找不到ID:{id}");
            }
            return ByIndex(index);
        }
        public static InitiativeConfig ByIndex(int index)
        {
            return datas[index];
        }
        public bool IsNull { get; private set; }
        public static InitiativeConfig Null { get; } = new InitiativeConfig() { IsNull = true }; 
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
        public System.Int32 pile_limit { get; set; }
        public System.Int32 isdroprecord { get; set; }
        public System.Int32 isbroadcast { get; set; }
        public System.Int32 time_length { get; set; }
        public System.Int32 invalid_time { get; set; }
        public System.Int32 colddown_id { get; set; }
        public System.Int32 server_colddown { get; set; }
        public System.Int32 client_colddown { get; set; }
        public System.Int32 limit_prof { get; set; }
        public System.Int32 limit_sex { get; set; }
        public System.Int32 limit_level { get; set; }
        public System.Int32 use_daytimes { get; set; }
        public System.Int32 use_type { get; set; }
        public System.Int32 param1 { get; set; }
        public System.Int32 param2 { get; set; }
        public System.Int32 param3 { get; set; }
        public System.Int32 param4 { get; set; }
        public System.Int32 other_sex_itemid { get; set; }
        public string use_msg { get; set; }
        public string get_msg { get; set; }
        public System.Int32 appe_type { get; set; }
        public System.Int32 icon_id { get; set; }
        public System.Int32 show_id { get; set; }
        public System.Int32 click_use { get; set; }
        public string open_panel { get; set; }
        public System.Int32 drop_icon { get; set; }
        public System.Int32 bag_type { get; set; }
        public System.Int32 is_tip_use { get; set; }
        public System.Int32 choose_use { get; set; }
        public System.Int32 auto_show_id { get; set; }
        public System.Int32 power { get; set; }
        public string get_way { get; set; }
        public System.Int32 is_display_role { get; set; }
        public System.Int32 special_show { get; set; }
        public System.Int32 is_diruse { get; set; }
        public System.Int32 is_texiao { get; set; }
        public System.Int32 is_tip_power { get; set; }
        public System.Int32 rarefloating { get; set; }
    }
}

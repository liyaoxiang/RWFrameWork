using Config;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LaunchProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        Debug.Log("进入了Launch流程");
        ConfigManager.LoadAllConfigsByAddressable("Assets/BundleAssets/Config");
        await Task.Yield();
    }
}

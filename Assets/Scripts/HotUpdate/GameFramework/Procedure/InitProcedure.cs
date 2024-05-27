using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class InitProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        Debug.Log("进入了Init流程");
        await Task.Yield();
    }
}

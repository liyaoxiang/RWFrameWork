using System.Collections;
using System.Collections.Generic;
using TGame.UI;
using UnityEngine;

public class LoginUIMediator :UIMediator<LoginUIView>
{
    public override void InitMediator(UIView view)
    {
        base.InitMediator(view);
    }
    protected override void OnShow(object arg)
    {
        base.OnShow(arg);
    }
    protected override void OnInit(LoginUIView view)
    {
        base.OnInit(view);
    }
    protected override void OnHide()
    {
        base.OnHide();
    }
}

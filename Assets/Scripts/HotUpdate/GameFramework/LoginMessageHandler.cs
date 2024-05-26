using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoginMessageHandler : MessageHandler<MessageType.Login>
{
    public override async Task HandleMessage(MessageType.Login arg)
    {
        Debug.Log("全局消息进行了触发");
        await Task.Yield();
    }
}

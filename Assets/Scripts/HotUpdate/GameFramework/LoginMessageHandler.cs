using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoginMessageHandler : MessageHandler<MessageType.Login>
{
    public override async Task HandleMessage(MessageType.Login arg)
    {
        Debug.Log("ȫ����Ϣ�����˴���");
        await Task.Yield();
    }
}

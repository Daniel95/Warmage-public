using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class ChannelBarReceiverClientAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(ChannelBarReceiverClient); }
}

[DisableAutoCreation]
public class ChannelBarReceiverClient : NetworkClientMessageSystem<ChannelBarMessage>
{
    //NativeHashMap<ulong, ChannelBarMessage> messages;

    protected override void OnCreate()
    {
        base.OnCreate();

        //messages = new NativeHashMap<ulong, ChannelBarMessage>(1000, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        //messages.Dispose(Dependency);

        base.OnDestroy();
    }

    protected override void OnMessage(ChannelBarMessage message)
    {
        //messages[message.netId] = message;

        //if(message.messageType != ChannelBarMessage.MessageType.UpdateTime)
        //{

        bool isPlayer = message.netId == PlayerLocalInfo.netId;
        bool isTarget = message.netId == PlayerLocalInfo.targetInfo.netId;

        if(isPlayer || isTarget)
        {
            UiChannelBar uiChannelBar;
            if (isPlayer) 
            {
                uiChannelBar = UiGame.GetInstance().playerStatusBar.channelBar;
            } 
            else 
            {
                uiChannelBar = UiWorldTargetStatusBar.GetInstance().channelBar;
            }

            if (message.messageType == ChannelBarMessage.MessageType.Start)
            {
                uiChannelBar.StartChanneling(message.time);
            }
            else if (message.messageType == ChannelBarMessage.MessageType.UpdateTime)
            {
                uiChannelBar.UpdateChannelTime(message.time);
            }
            else if (message.messageType == ChannelBarMessage.MessageType.Interrupt)
            {
                uiChannelBar.InterruptChanneling();
            }
        }
        //}

        //if (message.netId != PlayerLocalInfo.netId) 
        //{
        //    UiChannelBar uiChannelBar = UiGame.GetInstance().playerStatusBar.channelBar;

        //    if (message.messageType == ChannelBarMessage.MessageType.Start)
        //    {
        //        uiChannelBar.StartChanneling(message.time);
        //    }
        //    else if (message.messageType == ChannelBarMessage.MessageType.UpdateTime)
        //    {
        //        uiChannelBar.UpdateChannelTime(message.time);
        //    }
        //    else if(message.messageType == ChannelBarMessage.MessageType.Interrupt)
        //    {
        //        uiChannelBar.InterruptChanneling();
        //    }
        //}
    }

    protected override void OnUpdate() 
    {
        //NativeHashMap<ulong, ChannelBarMessage> _messages = messages;

        //Entities.ForEach((ref ChannelingComponent channelingComponent,
        //            in NetworkEntity networkEntity) =>
        //{
        //    if (_messages.ContainsKey(networkEntity.netId))
        //    {
        //        ChannelBarMessage message = _messages[networkEntity.netId];

        //        channelingComponent.active = message.active;
        //        channelingComponent.timeLeft = message.timeLeft;
        //        channelingComponent.totalTime = message.totalTime;
        //    }
        //})
        //.Run();

        //messages.Clear();
    }
}

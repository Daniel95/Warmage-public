using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

public class ChatMessageDispatcherClientAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    // add system if Authoring is used
    public Type GetSystemType() => typeof(ChatMessageDispatcherClient);
}

// use SelectiveSystemAuthoring to create it selectively
[DisableAutoCreation]
public class ChatMessageDispatcherClient : NetworkClientMessageSystem<ChatMessage>
{
    protected override void OnUpdate() {}
    protected override void OnMessage(ChatMessage message)
    {
        // convert to the actual message type
        Debug.Log("Client message: " + message.sender + ": " + message.text);

        // add message
        GameClientSystem chatClient = (GameClientSystem)client;
        chatClient.messages.Enqueue(message);

        // respect max entries
        if (chatClient.messages.Count > chatClient.keepMessages)
            chatClient.messages.Dequeue();
    }
}

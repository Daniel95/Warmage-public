using DOTSNET;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class NameSelection : MonoBehaviour
{
    // ECS world systems
    GameClientSystem client =>
        Bootstrap.ClientWorld.GetExistingSystem<GameClientSystem>();

    PrefabSystem prefabSystem =>
        Bootstrap.ClientWorld.GetExistingSystem<PrefabSystem>();

    public int width = 200;
    public int height = 80;

    string input = "";

    // hello old friend
    void OnGUI()
    {
        // only while client connected & not joined yet
        if (client.state == ClientState.CONNECTED && !client.joined)
        {
            // GUI area
            float x = Screen.width / 2 - width / 2;
            float y = Screen.height / 2 - height / 2;
            GUILayout.BeginArea(new Rect(x, y, width, height));
            GUILayout.BeginVertical("Box");

            // input field with max length = 32 because the JoinWorldMessage
            // uses a NativeString32
            GUILayout.Label("Enter Nickname:");
            input = GUILayout.TextField(input, 32);

            // join button
            GUI.enabled = !string.IsNullOrWhiteSpace(input);
            if (GUILayout.Button("Join"))
            {
                Debug.Log("Joining as: " + input + "...");

                // our example only has 1 spawnable prefab. let's use that for the
                // player.
                if (FindFirstRegisteredPrefab(out Bytes16 prefabId, out Entity prefab))
                {
                    JoinWorldMessage message = new JoinWorldMessage(prefabId, input);
                    client.Send(message);
                    Debug.Log("AutoJoinWorldSystem: requesting to spawn player with prefabId=" + Conversion.Bytes16ToGuid(prefabId));
                }
                else Debug.LogError("AutoJoinWorldSystem: no registered prefab found to join with.");


                //client.Send(new JoinWorldMessage(input));
            }
            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    bool FindFirstRegisteredPrefab(out Bytes16 prefabId, out Entity prefab)
    {
        foreach (KeyValuePair<Bytes16, Entity> kvp in prefabSystem.prefabs)
        {
            prefabId = kvp.Key;
            prefab = kvp.Value;
            return true;
        }
        prefabId = new Bytes16();
        prefab = new Entity();
        return false;
    }
}

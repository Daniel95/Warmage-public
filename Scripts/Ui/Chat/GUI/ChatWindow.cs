using DOTSNET;
using UnityEngine;

public class ChatWindow : MonoBehaviour
{
    // ECS world systems
    GameClientSystem client =>
        Bootstrap.ClientWorld.GetExistingSystem<GameClientSystem>();

    public int width = 400;
    public int height = 300;

    public int marginX = 20;
    public int marginY = 20;

    Vector2 scroll = Vector2.zero;
    string input = "";

    // hello old friend
    void OnGUI()
    {
        // only while client connected & joined
        if (client.state == ClientState.CONNECTED && client.joined)
        {
            // GUI area
            //float x = Screen.width / 2 - width / 2;
            //float y = Screen.height / 2 - height / 2;

            if (ControlModeManager.mode != ControlMode.MouseControl)
            {
                GUI.FocusControl(null);
            }

            float x = width / 2 + marginX;
            float y = height / 2 + marginY;

            GUILayout.BeginArea(new Rect(0, Screen.height - height, width, height));
            GUILayout.BeginVertical("Box");

                // display all messages
                scroll = GUILayout.BeginScrollView(scroll, "Box");
                    foreach (ChatMessage message in client.messages)
                    {
                        GUILayout.Label("<b>" + message.sender + ":</b> " + message.text);
                    }
                    GUILayout.FlexibleSpace();
                GUILayout.EndScrollView();


                // send area
                GUILayout.BeginHorizontal();
                    // input field with max length = 128 because the ChatMessages
                    // text uses NativeString128
                    input = GUILayout.TextField(input, 128);


                    // send button
                    GUI.enabled = !string.IsNullOrWhiteSpace(input);
                    if (GUILayout.Button("Send", GUILayout.Width(80)))
                    {
                        // send chat message without sender.
                        // server already knows our name.
                        Debug.Log("Sending: " + input);
                        client.Send(new ChatMessage("", input));
                        input = "";
                    }
                    GUI.enabled = true;
                GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}

using UnityEngine;

public class PlayerNameHelper : MonoBehaviour
{
    #region Singleton
    public static PlayerNameHelper GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<PlayerNameHelper>();
        }
        return instance;
    }

    private static PlayerNameHelper instance;
    #endregion

    [SerializeField] private PlayerName playerNamePrefab = null;

    public void SpawnPlayerName(ulong playerNetId)
    {
        GameObject gameObject = Instantiate(playerNamePrefab.gameObject);
        gameObject.GetComponent<PlayerName>().Init(playerNetId);
    }
}

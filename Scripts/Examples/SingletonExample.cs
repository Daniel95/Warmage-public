using UnityEngine;

public class SingletonExample : MonoBehaviour
{
    #region Singleton
    public static SingletonExample GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<SingletonExample>();
        }
        return instance;
    }

    private static SingletonExample instance;
    #endregion
}

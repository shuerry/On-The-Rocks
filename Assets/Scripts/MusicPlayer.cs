using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer instance;

    void Awake()
    {
        if (instance != null && instance != this && instance.name == this.name)
        {
            Destroy(gameObject); // prevent duplicates
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // stay across scenes
        }
    }
}

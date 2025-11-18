using UnityEngine;

public class SupabaseReadyManager : MonoBehaviour
{
    public static SupabaseReadyManager Instance { get; private set; }
    
    private static bool supabaseReady = false;
    private static bool notificationReceived = false;

    private void Awake()
    {
     if (Instance == null)
        {
  Instance = this;
     DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnSupabaseReady()
    {
        supabaseReady = true;
        notificationReceived = true;
    }

    public static bool IsSupabaseReady()
    {
        return supabaseReady;
    }

    public static bool HasReceivedNotification()
    {
        return notificationReceived;
    }
}

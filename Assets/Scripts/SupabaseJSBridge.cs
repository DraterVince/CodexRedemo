using System.Runtime.InteropServices;
using UnityEngine;

public static class SupabaseJSBridge
{
    [DllImport("__Internal")]
    private static extern void SupabaseRegister(string email, string password);

    [DllImport("__Internal")]
    private static extern void SupabaseLogin(string email, string password);

    [DllImport("__Internal")]
    private static extern void SupabaseLogout();

    public static void Register(string email, string password)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SupabaseRegister(email, password);
#else
        Debug.Log("Supabase.Register only works in WebGL build.");
#endif
    }

    public static void Login(string email, string password)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SupabaseLogin(email, password);
#else
        Debug.Log("Supabase.Login only works in WebGL build.");
#endif
    }

    public static void Logout()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SupabaseLogout();
#else
        Debug.Log("Supabase.Logout only works in WebGL build.");
#endif
    }
}
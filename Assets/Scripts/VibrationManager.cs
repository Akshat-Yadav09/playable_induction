using UnityEngine;
using System.Runtime.InteropServices;

public static class VibrationManager
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void Vibrate_WebGL(int patternMs);
#endif

    /// <summary>
    /// Triggers a vibration. On WebGL (mobile browsers), it uses the HTML5 Vibration API.
    /// On Android/iOS native builds, it falls back to Handheld.Vibrate.
    /// </summary>
    /// <param name="durationMs">Duration in milliseconds (WebGL only)</param>
    public static void Vibrate(int durationMs = 150)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Vibrate_WebGL(durationMs);
#elif UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
}

using UnityEngine;

namespace Plugins.Unity.Editor
{
    public static class UnityLogger
    {
        public static void Log(string text)
        {
            Debug.Log(text);
        }
    }
}
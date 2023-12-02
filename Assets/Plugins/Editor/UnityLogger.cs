using UnityEngine;

namespace Plugins.Editor
{
    public static class UnityLogger
    {
        public static void Log(string text)
        {
            Debug.Log(text);
        }
    }
}
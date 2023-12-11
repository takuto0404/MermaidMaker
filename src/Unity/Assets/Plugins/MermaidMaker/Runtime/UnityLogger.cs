using UnityEngine;

namespace Plugins.MermaidMaker.Runtime
{
    public static class UnityLogger
    {
        public static void Log(string text)
        {
            Debug.Log(text);
        }
    }
}
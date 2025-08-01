using System;
using System.Threading;

namespace Mymmorpg.tools
{
    public static class MainThreadUtil
    {
        private static SynchronizationContext unityContext;

        /// <summary>
        /// 在 Unity 主线程中初始化（例如在 Game 启动类中调用一次）
        /// </summary>
        public static void Init()
        {
            unityContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// 在线程中安全调用主线程方法（必须先 Init）
        /// </summary>
        public static void RunOnMainThread(Action action)
        {
            if (unityContext == null)
            {
                UnityEngine.Debug.LogError("MainThreadUtil 未初始化，必须先在主线程调用 MainThreadUtil.Init()");
                return;
            }

            unityContext.Post(_ => action?.Invoke(), null);
        }
    }
}
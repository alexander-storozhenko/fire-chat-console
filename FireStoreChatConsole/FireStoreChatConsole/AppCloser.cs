using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FireStoreChatConsole
{
    internal enum ConsoleSignal
    {
        CtrlC = 0,
        CtrlBreak = 1,
        Close = 2,
        LogOff = 5,
        Shutdown = 6
    }
    internal delegate void SignalHandler(ConsoleSignal consoleSignal);
    internal static class ConsoleHelper
    {
        [DllImport("Kernel32", EntryPoint = "SetConsoleCtrlHandler")]
        public static extern bool SetSignalHandler(SignalHandler handler, bool add);
    }
    static class AppCloser
    {
        static SignalHandler signalHandler;

        public static void SetCloseHandler(SignalHandler handler)
        {
            signalHandler += handler;
            ConsoleHelper.SetSignalHandler(signalHandler, true);
        }
       
    }
}

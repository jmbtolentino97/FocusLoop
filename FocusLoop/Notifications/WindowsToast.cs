using System;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using PomodoroTimer.States;

namespace FocusLoop.Notifications;

public static class WindowsToast
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appID);

    public const string AppUserModelID = "FocusLoop.FocusLoop";

    public static void EnsureRegistration()
    {
        try { SetCurrentProcessExplicitAppUserModelID(AppUserModelID); } catch { }
        try { WindowsShortcut.EnsureStartMenuShortcut(AppUserModelID, "Focus Loop"); } catch { }
    }

    public static void ShowStateChangeToast(State oldState, State newState)
    {
        var title = "Focus Loop";
        var msg = Utilities.StateNameHelper.GetFriendlyName(oldState) + " -> " + Utilities.StateNameHelper.GetFriendlyName(newState);
        try
        {
            var content = new ToastContentBuilder()
                .AddText(title)
                .AddText(msg)
                .GetToastContent();
            var toast = new ToastNotification(content.GetXml());
            ToastNotificationManager.CreateToastNotifier(AppUserModelID).Show(toast);
        }
        catch { }
    }
}
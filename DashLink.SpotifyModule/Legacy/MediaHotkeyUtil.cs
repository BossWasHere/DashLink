using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Spotify.Legacy
{

    /*
     * Adapted from: https://stackoverflow.com/questions/15013582/send-multimedia-commands
     * For volume alternatives, see: https://stackoverflow.com/questions/13139181/how-to-programmatically-set-the-system-volume
     */
    public static class MediaHotkeyUtil
    {
        [DllImport("user32.dll")]
#pragma warning disable IDE1006 // Naming Style Checking
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
#pragma warning restore IDE1006

        public const int KEYEVENTF_EXTENTEDKEY = 1;
        public const int KEYEVENTF_KEYUP = 0;

        public const int VK_VOLUME_MUTE = 0xAD;
        public const int VK_VOLUME_DOWN = 0xAE;
        public const int VK_VOLUME_UP = 0xAF;

        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;

        public static void TogglePlaybackState(bool releaseKey = false)
        {
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            if (releaseKey) keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }

        public static void SkipNextTrack(bool releaseKey = false)
        {
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            if (releaseKey) keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }

        public static void SkipPreviousTrack(bool releaseKey = false)
        {
            keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            if (releaseKey) keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }

        //[Obsolete("SendMessageW from user32.dll is probably better")]
        public static void ToggleGlobalVolumeMute(bool releaseKey = false)
        {
            keybd_event(VK_VOLUME_MUTE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            if (releaseKey) keybd_event(VK_VOLUME_MUTE, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }

        //[Obsolete("SendMessageW from user32.dll is probably better")]
        public static void DecreaseGlobalVolume(bool releaseKey = false)
        {
            keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            if (releaseKey) keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }

        //[Obsolete("SendMessageW from user32.dll is probably better")]
        public static void IncreaseGlobalVolume(bool releaseKey = false)
        {
            keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            if (releaseKey) keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }
    }
}

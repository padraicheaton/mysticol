using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Heaton.Input
{
    public static class MouseHandler
    {
        public static bool Locked { get; private set; }

        public static void Lock() => SetLockState(true);
        public static void Unlock() => SetLockState(false);

        private static void SetLockState(bool val)
        {
            Cursor.lockState = val ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !val;

            Locked = val;
        }
    }
}
using System;
using UnityEngine;

namespace Assets.Scripts.Delegates
{
    public delegate void OnCharacterSelection(GameObject g);
    public delegate void OnSetCharacterInTurn(GameObject g);
    public delegate void OnAnimationEnd();
}

namespace Assets.Scripts.Utils
{
   
    public static class EventManager<T> where T : System.Delegate
    {
        private static T _handle;

        public static void Register(T callback)
        {
            _handle = Delegate.Combine(_handle, callback) as T;
        }

        public static void Unregister(T callback)
        {
            _handle = Delegate.Remove(_handle, callback) as T;
        }

        public static T Trigger
        {
            get { return _handle; }
        }

    }
}

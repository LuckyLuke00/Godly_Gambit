using UnityEngine;

namespace GodlyGambit
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null && !IsQuitting)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        _instance = obj.AddComponent<T>();

                        DontDestroyOnLoad(obj);
                    }
                }

                return _instance;
            }
        }

        private static bool IsQuitting { get; set; }

        private void OnApplicationQuit()
        {
            IsQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}

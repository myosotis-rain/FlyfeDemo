using UnityEngine;
using System;

namespace Flyfe.Core
{
    /// <summary>
    /// Tracks global gem collection. 
    /// Professional Practice: Separates data tracking (Manager) from visual objects (Gems).
    /// </summary>
    public class GemManager : MonoBehaviour
    {
        public static GemManager Instance { get; private set; }

        public static event Action<int> OnGemsChanged;

        private int _totalGems = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddGem()
        {
            _totalGems++;
            Debug.Log($"<color=yellow>[GemManager]</color> Gems: {_totalGems}");
            OnGemsChanged?.Invoke(_totalGems);
        }

        public int GetGemCount() => _totalGems;

        // Reset if starting a new game or restarting a level
        public void ResetGems()
        {
            _totalGems = 0;
            OnGemsChanged?.Invoke(_totalGems);
        }
    }
}

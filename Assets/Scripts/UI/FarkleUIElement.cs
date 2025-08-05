using System;
using UnityEngine;

namespace UI
{
    public class FarkleUIElement : MonoBehaviour
    {
        /// <summary>
        /// Unique name for the UI element, used for identification in the Farkle UI system.
        /// This name should be unique across all UI elements to avoid conflicts.
        /// </summary>
        [SerializeField] public string ElementName = "FarkleUIElement";

        private void Awake()
        {
            Init();
        }

        protected virtual void Init()
        {
            // This method can be overridden in derived classes if needed
        }

        public virtual void Show()
        {
            SetActive(true);
        }

        public virtual void Hide()
        {
            SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}
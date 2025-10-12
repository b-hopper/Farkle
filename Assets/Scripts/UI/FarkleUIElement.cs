using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Farkle.UI
{
    public abstract class FarkleUIElement : MonoBehaviour
    {
        /// <summary>
        /// Unique name for the UI element, used for identification in the Farkle UI system.
        /// This name should be unique across all UI elements to avoid conflicts.
        /// </summary>
        [SerializeField] public string ElementName = "FarkleUIElement";

        [SerializeField] public TMP_Text AlertText;
        
        static FarkleUIElement instance;
        public static FarkleUIElement Instance => instance ? instance : instance = FindFirstObjectByType<FarkleUIElement>(FindObjectsInactive.Include);
        
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
        
        public void Alert(string message)
        {
            if (AlertText != null)
            {
                AlertText.text = message;
                AlertText.gameObject.SetActive(true);
                
                // DOTween fade out after 3 seconds
                float displayDuration = 3f;
                float fadeDuration = 1f;
                AlertText.DOFade(1f, 0f).SetUpdate(true);
                AlertText.DOFade(0f, fadeDuration).SetDelay(displayDuration).SetUpdate(true)
                    .OnComplete(() => AlertText.gameObject.SetActive(false));
                
            }
            else
            {
                FarkleLogger.LogWarning($"(FarkleUIElement::Alert) AlertText is not assigned in {ElementName}. Message: {message}");
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlimeTime.UI
{
    /// <summary>
    /// Adds a shared hover bounce effect to a list of UI buttons in the current scene.
    /// Put one manager in each scene that needs the effect.
    /// </summary>
    public class ButtonBounceManager : MonoBehaviour
    {
        [Serializable]
        private class ButtonEntry
        {
            [Tooltip("The Button that receives pointer events. Its RectTransform is animated.")]
            public Button button;

            [Tooltip("The larger white image shown behind the button visual while hovered.")]
            public GameObject whiteBorder;

            [NonSerialized] public RectTransform animatedRect;
            [NonSerialized] public Vector3 originalScale;
            [NonSerialized] public Quaternion originalRotation;
            [NonSerialized] public Coroutine animation;
        }

        [Header("Buttons")]
        [Tooltip("Add every button in this scene that should use the hover effect.")]
        [SerializeField] private List<ButtonEntry> buttons = new List<ButtonEntry>();

        [Header("Bounce")]
        [Min(1f)]
        [SerializeField] private float maximumScale = 1.12f;

        [Min(0.01f)]
        [SerializeField] private float duration = 0.18f;

        [Min(0f)]
        [SerializeField] private float shakeAngle = 5f;

        [Min(0f)]
        [SerializeField] private float shakeCount = 2f;

        private void Awake()
        {
            var registeredButtons = new HashSet<Button>();

            foreach (ButtonEntry entry in buttons)
            {
                if (entry == null || entry.button == null)
                {
                    Debug.LogWarning($"{nameof(ButtonBounceManager)} on '{name}' has an empty button entry.", this);
                    continue;
                }

                if (!registeredButtons.Add(entry.button))
                {
                    Debug.LogWarning(
                        $"Button '{entry.button.name}' is listed more than once in {nameof(ButtonBounceManager)}.",
                        this);
                    continue;
                }

                entry.animatedRect = entry.button.transform as RectTransform;
                entry.originalScale = entry.animatedRect.localScale;
                entry.originalRotation = entry.animatedRect.localRotation;

                SetBorderVisible(entry, false);
                RegisterPointerEvents(entry);
            }
        }

        private void RegisterPointerEvents(ButtonEntry entry)
        {
            EventTrigger trigger = entry.button.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = entry.button.gameObject.AddComponent<EventTrigger>();
            }

            if (trigger.triggers == null)
            {
                trigger.triggers = new List<EventTrigger.Entry>();
            }

            AddTrigger(trigger, EventTriggerType.PointerEnter, _ => HandlePointerEnter(entry));
            AddTrigger(trigger, EventTriggerType.PointerExit, _ => HandlePointerExit(entry));
        }

        private static void AddTrigger(
            EventTrigger trigger,
            EventTriggerType eventType,
            UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            var triggerEntry = new EventTrigger.Entry { eventID = eventType };
            triggerEntry.callback.AddListener(callback);
            trigger.triggers.Add(triggerEntry);
        }

        private void HandlePointerEnter(ButtonEntry entry)
        {
            if (!entry.button.IsInteractable())
            {
                return;
            }

            SetBorderVisible(entry, true);
            PlayBounce(entry);
        }

        private void HandlePointerExit(ButtonEntry entry)
        {
            SetBorderVisible(entry, false);
        }

        private void PlayBounce(ButtonEntry entry)
        {
            if (entry.animation != null)
            {
                StopCoroutine(entry.animation);
            }

            entry.animatedRect.localScale = entry.originalScale;
            entry.animatedRect.localRotation = entry.originalRotation;
            entry.animation = StartCoroutine(Bounce(entry));
        }

        private IEnumerator Bounce(ButtonEntry entry)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float bounce = Mathf.Sin(progress * Mathf.PI);
                float scale = Mathf.Lerp(1f, maximumScale, bounce);
                float angle = Mathf.Sin(progress * Mathf.PI * 2f * shakeCount)
                              * shakeAngle
                              * (1f - progress);

                entry.animatedRect.localScale = entry.originalScale * scale;
                entry.animatedRect.localRotation =
                    entry.originalRotation * Quaternion.Euler(0f, 0f, angle);

                yield return null;
            }

            RestoreTransform(entry);
            entry.animation = null;
        }

        private static void SetBorderVisible(ButtonEntry entry, bool visible)
        {
            if (entry.whiteBorder != null && entry.whiteBorder.activeSelf != visible)
            {
                entry.whiteBorder.SetActive(visible);
            }
        }

        private static void RestoreTransform(ButtonEntry entry)
        {
            if (entry.animatedRect == null)
            {
                return;
            }

            entry.animatedRect.localScale = entry.originalScale;
            entry.animatedRect.localRotation = entry.originalRotation;
        }

        private void OnDisable()
        {
            foreach (ButtonEntry entry in buttons)
            {
                if (entry == null)
                {
                    continue;
                }

                if (entry.animation != null)
                {
                    StopCoroutine(entry.animation);
                    entry.animation = null;
                }

                RestoreTransform(entry);
                SetBorderVisible(entry, false);
            }
        }
    }
}

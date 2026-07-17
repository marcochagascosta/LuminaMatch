using System;
using System.Collections;
using UnityEngine;

namespace LuminaMatch.UI
{
    /// <summary>Lightweight UI juice without external tween libs.</summary>
    public class BoardJuice : MonoBehaviour
    {
        public static BoardJuice Ensure(Transform host)
        {
            var existing = host.GetComponent<BoardJuice>();
            if (existing != null) return existing;
            return host.gameObject.AddComponent<BoardJuice>();
        }

        public void Punch(RectTransform target, Action onDone = null)
            => StartCoroutine(PunchRoutine(target, onDone));

        public void PopOut(RectTransform target, Action onDone = null)
            => StartCoroutine(PopRoutine(target, onDone));

        public void DropIn(RectTransform target, float fromYOffset, float duration = 0.14f, Action onDone = null)
            => StartCoroutine(DropRoutine(target, fromYOffset, duration, onDone));

        public void FlashScore(RectTransform target, Action onDone = null)
            => StartCoroutine(FlashRoutine(target, onDone));

        IEnumerator PunchRoutine(RectTransform target, Action onDone)
        {
            if (target == null)
            {
                onDone?.Invoke();
                yield break;
            }

            Vector3 baseScale = Vector3.one;
            float t = 0f;
            while (t < 0.12f)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Sin(t / 0.12f * Mathf.PI);
                target.localScale = baseScale * (1f + 0.12f * k);
                yield return null;
            }
            target.localScale = baseScale;
            onDone?.Invoke();
        }

        IEnumerator PopRoutine(RectTransform target, Action onDone)
        {
            if (target == null)
            {
                onDone?.Invoke();
                yield break;
            }

            float t = 0f;
            while (t < 0.1f)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - (t / 0.1f);
                target.localScale = Vector3.one * Mathf.Max(0.01f, k);
                yield return null;
            }
            onDone?.Invoke();
        }

        IEnumerator DropRoutine(RectTransform target, float fromYOffset, float duration, Action onDone)
        {
            if (target == null)
            {
                onDone?.Invoke();
                yield break;
            }

            Vector2 end = target.anchoredPosition;
            Vector2 start = end + new Vector2(0f, fromYOffset);
            target.anchoredPosition = start;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                float ease = 1f - Mathf.Pow(1f - k, 3f);
                target.anchoredPosition = Vector2.Lerp(start, end, ease);
                yield return null;
            }
            target.anchoredPosition = end;
            onDone?.Invoke();
        }

        IEnumerator FlashRoutine(RectTransform target, Action onDone)
        {
            if (target == null)
            {
                onDone?.Invoke();
                yield break;
            }

            Vector3 baseScale = target.localScale;
            float t = 0f;
            while (t < 0.18f)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Sin(t / 0.18f * Mathf.PI);
                target.localScale = baseScale * (1f + 0.18f * k);
                yield return null;
            }
            target.localScale = baseScale;
            onDone?.Invoke();
        }
    }
}

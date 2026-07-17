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
        {
            StopAllCoroutines();
            StartCoroutine(PunchRoutine(target, onDone));
        }

        public void PopOut(RectTransform target, Action onDone = null)
        {
            StopAllCoroutines();
            StartCoroutine(PopRoutine(target, onDone));
        }

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
    }
}

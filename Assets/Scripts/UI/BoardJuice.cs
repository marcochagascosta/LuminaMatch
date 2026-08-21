using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        public void DropIn(RectTransform target, float fromYOffset, float duration = 0.22f, Action onDone = null)
            => StartCoroutine(DropRoutine(target, fromYOffset, duration, onDone));

        public void FlashScore(RectTransform target, Action onDone = null)
            => StartCoroutine(FlashRoutine(target, onDone));

        public void SwapSlide(RectTransform a, RectTransform b, float duration = 0.16f, Action onDone = null)
            => StartCoroutine(SwapRoutine(a, b, duration, onDone));

        /// <summary>
        /// Rocket flies across a row or column while cells pop in cascade.
        /// horizontal=true → row (left→right); false → column (top→bottom in UI = high y → low y).
        /// </summary>
        public void RocketBlast(
            RectTransform boardRoot,
            Sprite rocketSprite,
            Sprite whiteSprite,
            IList<RectTransform> cellsInOrder,
            bool horizontal,
            Action onDone = null)
            => StartCoroutine(RocketBlastRoutine(boardRoot, rocketSprite, whiteSprite, cellsInOrder, horizontal, onDone));

        public void BombBlast(
            RectTransform boardRoot,
            Sprite bombSprite,
            Sprite whiteSprite,
            RectTransform originCell,
            IList<RectTransform> cells,
            Action onDone = null)
            => StartCoroutine(BombBlastRoutine(boardRoot, bombSprite, whiteSprite, originCell, cells, onDone));

        public void ColorDiskBlast(
            RectTransform boardRoot,
            Sprite diskSprite,
            Sprite whiteSprite,
            Color tint,
            RectTransform originCell,
            IList<RectTransform> cells,
            Action onDone = null)
            => StartCoroutine(ColorDiskBlastRoutine(boardRoot, diskSprite, whiteSprite, tint, originCell, cells, onDone));

        public void PalaceReveal(RectTransform target, Action onDone = null)
            => StartCoroutine(PalaceRevealRoutine(target, onDone));

        public void ComboPulse(RectTransform boardRoot, Sprite whiteSprite, Action onDone = null)
            => StartCoroutine(ComboPulseRoutine(boardRoot, whiteSprite, onDone));

        public void SparkleBurst(RectTransform parent, Sprite whiteSprite, Vector2 center, int count = 8)
            => StartCoroutine(SparkleBurstRoutine(parent, whiteSprite, center, count));

        IEnumerator RocketBlastRoutine(
            RectTransform boardRoot,
            Sprite rocketSprite,
            Sprite whiteSprite,
            IList<RectTransform> cellsInOrder,
            bool horizontal,
            Action onDone)
        {
            if (boardRoot == null || cellsInOrder == null || cellsInOrder.Count == 0)
            {
                onDone?.Invoke();
                yield break;
            }

            // Use cell roots' board-local positions (gems themselves sit at 0,0 inside cells).
            RectTransform first = null, last = null;
            for (int i = 0; i < cellsInOrder.Count; i++)
            {
                if (cellsInOrder[i] == null) continue;
                if (first == null) first = cellsInOrder[i];
                last = cellsInOrder[i];
            }
            if (first == null || last == null)
            {
                onDone?.Invoke();
                yield break;
            }

            var flyGo = new GameObject("RocketFly", typeof(RectTransform), typeof(Image));
            flyGo.transform.SetParent(boardRoot, false);
            var flyRt = flyGo.GetComponent<RectTransform>();
            float size = Mathf.Max(56f, first.rect.width * 0.95f);
            flyRt.sizeDelta = new Vector2(size, size);
            var flyImg = flyGo.GetComponent<Image>();
            flyImg.sprite = rocketSprite != null ? rocketSprite : whiteSprite;
            flyImg.color = Color.white;
            flyImg.preserveAspect = true;
            flyImg.raycastTarget = false;
            flyRt.SetAsLastSibling();

            Vector2 start = first.anchoredPosition;
            Vector2 end = last.anchoredPosition;
            Vector2 dir = end - start;
            if (dir.sqrMagnitude < 1f)
                dir = horizontal ? Vector2.right * size * 2f : Vector2.down * size * 2f;
            else
                dir = dir.normalized * (dir.magnitude + size * 0.8f);
            end = start + dir;

            flyRt.anchoredPosition = start;
            flyRt.localEulerAngles = new Vector3(0f, 0f, horizontal ? 0f : -90f);

            var trailGo = new GameObject("RocketTrail", typeof(RectTransform), typeof(Image));
            trailGo.transform.SetParent(boardRoot, false);
            var trailRt = trailGo.GetComponent<RectTransform>();
            trailRt.SetSiblingIndex(flyRt.GetSiblingIndex());
            var trailImg = trailGo.GetComponent<Image>();
            trailImg.sprite = whiteSprite;
            trailImg.color = new Color(1f, 0.8f, 0.35f, 0.5f);
            trailImg.raycastTarget = false;
            trailRt.pivot = new Vector2(0f, 0.5f);
            trailRt.localEulerAngles = horizontal ? Vector3.zero : new Vector3(0f, 0f, -90f);

            const float flyDur = 0.42f;
            int popsLeft = 0;
            int nextPop = 0;
            float t = 0f;
            while (t < flyDur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / flyDur);
                float ease = k * k * (3f - 2f * k);
                flyRt.anchoredPosition = Vector2.Lerp(start, end, ease);

                float traveled = Vector2.Distance(start, flyRt.anchoredPosition);
                trailRt.anchoredPosition = start;
                trailRt.sizeDelta = new Vector2(Mathf.Max(4f, traveled), size * 0.2f);

                int shouldPop = Mathf.Clamp(Mathf.FloorToInt(k * cellsInOrder.Count + 0.001f) + 1, 0, cellsInOrder.Count);
                while (nextPop < shouldPop)
                {
                    var cell = cellsInOrder[nextPop++];
                    if (cell == null) continue;
                    popsLeft++;
                    StartCoroutine(FlashThenPop(cell, () => popsLeft--));
                }

                yield return null;
            }

            while (nextPop < cellsInOrder.Count)
            {
                var cell = cellsInOrder[nextPop++];
                if (cell == null) continue;
                popsLeft++;
                StartCoroutine(FlashThenPop(cell, () => popsLeft--));
            }

            float wait = 0f;
            while (popsLeft > 0 && wait < 0.55f)
            {
                wait += Time.unscaledDeltaTime;
                yield return null;
            }

            if (flyGo != null) Destroy(flyGo);
            if (trailGo != null) Destroy(trailGo);
            onDone?.Invoke();
        }

        IEnumerator BombBlastRoutine(
            RectTransform boardRoot,
            Sprite bombSprite,
            Sprite whiteSprite,
            RectTransform originCell,
            IList<RectTransform> cells,
            Action onDone)
        {
            if (boardRoot == null || originCell == null)
            {
                onDone?.Invoke();
                yield break;
            }

            var ringGo = new GameObject("BombRing", typeof(RectTransform), typeof(Image));
            ringGo.transform.SetParent(boardRoot, false);
            var ringRt = ringGo.GetComponent<RectTransform>();
            ringRt.anchoredPosition = originCell.anchoredPosition;
            float baseSize = Mathf.Max(48f, originCell.rect.width);
            ringRt.sizeDelta = new Vector2(baseSize, baseSize);
            var ringImg = ringGo.GetComponent<Image>();
            ringImg.sprite = bombSprite != null ? bombSprite : whiteSprite;
            ringImg.color = new Color(1f, 0.55f, 0.25f, 0.95f);
            ringImg.preserveAspect = true;
            ringImg.raycastTarget = false;
            ringRt.SetAsLastSibling();

            float t = 0f;
            const float dur = 0.28f;
            int popsLeft = 0;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                float scale = 1f + 2.2f * k;
                ringRt.localScale = Vector3.one * scale;
                var c = ringImg.color;
                c.a = 0.95f * (1f - k);
                ringImg.color = c;
                yield return null;
            }

            if (cells != null)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    var cell = cells[i];
                    if (cell == null) continue;
                    popsLeft++;
                    StartCoroutine(FlashThenPop(cell, () => popsLeft--));
                }
            }

            float wait = 0f;
            while (popsLeft > 0 && wait < 0.5f)
            {
                wait += Time.unscaledDeltaTime;
                yield return null;
            }

            if (ringGo != null) Destroy(ringGo);
            onDone?.Invoke();
        }

        IEnumerator ColorDiskBlastRoutine(
            RectTransform boardRoot,
            Sprite diskSprite,
            Sprite whiteSprite,
            Color tint,
            RectTransform originCell,
            IList<RectTransform> cells,
            Action onDone)
        {
            if (boardRoot == null || originCell == null)
            {
                onDone?.Invoke();
                yield break;
            }

            var diskGo = new GameObject("ColorDiskFx", typeof(RectTransform), typeof(Image));
            diskGo.transform.SetParent(boardRoot, false);
            var diskRt = diskGo.GetComponent<RectTransform>();
            diskRt.anchoredPosition = originCell.anchoredPosition;
            float size = Mathf.Max(56f, originCell.rect.width * 1.1f);
            diskRt.sizeDelta = new Vector2(size, size);
            var diskImg = diskGo.GetComponent<Image>();
            diskImg.sprite = diskSprite != null ? diskSprite : whiteSprite;
            diskImg.color = tint;
            diskImg.preserveAspect = true;
            diskImg.raycastTarget = false;
            diskRt.SetAsLastSibling();

            float t = 0f;
            const float spinDur = 0.36f;
            int popsLeft = 0;
            int nextPop = 0;
            while (t < spinDur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / spinDur);
                diskRt.localEulerAngles = new Vector3(0f, 0f, k * 360f);
                diskRt.localScale = Vector3.one * (1f + 0.55f * Mathf.Sin(k * Mathf.PI));
                var c = diskImg.color;
                c.a = 1f - 0.35f * k;
                diskImg.color = c;

                if (cells != null)
                {
                    int shouldPop = Mathf.Clamp(Mathf.FloorToInt(k * cells.Count + 0.001f) + 1, 0, cells.Count);
                    while (nextPop < shouldPop)
                    {
                        var cell = cells[nextPop++];
                        if (cell == null) continue;
                        popsLeft++;
                        StartCoroutine(FlashThenPop(cell, () => popsLeft--));
                    }
                }
                yield return null;
            }

            if (cells != null)
            {
                while (nextPop < cells.Count)
                {
                    var cell = cells[nextPop++];
                    if (cell == null) continue;
                    popsLeft++;
                    StartCoroutine(FlashThenPop(cell, () => popsLeft--));
                }
            }

            float wait = 0f;
            while (popsLeft > 0 && wait < 0.55f)
            {
                wait += Time.unscaledDeltaTime;
                yield return null;
            }

            if (diskGo != null) Destroy(diskGo);
            onDone?.Invoke();
        }

        IEnumerator SparkleBurstRoutine(RectTransform parent, Sprite whiteSprite, Vector2 center, int count)
        {
            if (parent == null)
                yield break;

            var sparks = new List<(RectTransform rt, Vector2 end, Image img)>();
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Spark", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(parent, false);
                var rt = go.GetComponent<RectTransform>();
                float size = UnityEngine.Random.Range(10f, 22f);
                rt.sizeDelta = new Vector2(size, size);
                rt.anchoredPosition = center;
                var img = go.GetComponent<Image>();
                img.sprite = whiteSprite;
                img.color = new Color(1f, UnityEngine.Random.Range(0.75f, 1f), UnityEngine.Random.Range(0.35f, 0.7f), 0.95f);
                img.raycastTarget = false;
                float ang = (i / (float)count) * Mathf.PI * 2f + UnityEngine.Random.Range(-0.2f, 0.2f);
                float dist = UnityEngine.Random.Range(80f, 220f);
                sparks.Add((rt, center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * dist, img));
            }

            float t = 0f;
            const float dur = 0.55f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                float ease = 1f - Mathf.Pow(1f - k, 2.2f);
                for (int i = 0; i < sparks.Count; i++)
                {
                    var s = sparks[i];
                    if (s.rt == null) continue;
                    s.rt.anchoredPosition = Vector2.Lerp(center, s.end, ease);
                    s.rt.localScale = Vector3.one * (1f - 0.4f * k);
                    var c = s.img.color;
                    c.a = 0.95f * (1f - k);
                    s.img.color = c;
                }
                yield return null;
            }

            for (int i = 0; i < sparks.Count; i++)
                if (sparks[i].rt != null)
                    Destroy(sparks[i].rt.gameObject);
        }

        IEnumerator ComboPulseRoutine(RectTransform boardRoot, Sprite whiteSprite, Action onDone)
        {
            if (boardRoot == null)
            {
                onDone?.Invoke();
                yield break;
            }

            var flashGo = new GameObject("ComboFlash", typeof(RectTransform), typeof(Image));
            flashGo.transform.SetParent(boardRoot, false);
            var flashRt = flashGo.GetComponent<RectTransform>();
            StretchFull(flashRt);
            var flashImg = flashGo.GetComponent<Image>();
            flashImg.sprite = whiteSprite;
            flashImg.color = new Color(1f, 0.92f, 0.55f, 0.35f);
            flashImg.raycastTarget = false;
            flashRt.SetAsLastSibling();

            Vector3 baseScale = boardRoot.localScale;
            float t = 0f;
            const float dur = 0.28f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                float wave = Mathf.Sin(k * Mathf.PI);
                boardRoot.localScale = baseScale * (1f + 0.04f * wave);
                var c = flashImg.color;
                c.a = 0.35f * (1f - k);
                flashImg.color = c;
                yield return null;
            }

            boardRoot.localScale = baseScale;
            if (flashGo != null) Destroy(flashGo);
            onDone?.Invoke();
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        IEnumerator PalaceRevealRoutine(RectTransform target, Action onDone)
        {
            if (target == null)
            {
                onDone?.Invoke();
                yield break;
            }

            Vector3 endScale = Vector3.one;
            target.localScale = Vector3.one * 0.55f;
            var img = target.GetComponent<Image>();
            Color baseColor = img != null ? img.color : Color.white;
            if (img != null)
            {
                var c = baseColor;
                c.a = 0.2f;
                img.color = c;
            }

            float t = 0f;
            const float dur = 0.55f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                float ease = 1f - Mathf.Pow(1f - k, 3f);
                target.localScale = Vector3.Lerp(Vector3.one * 0.55f, endScale * 1.08f, ease);
                if (img != null)
                {
                    var c = baseColor;
                    c.a = Mathf.Lerp(0.2f, 1f, ease);
                    img.color = c;
                }
                yield return null;
            }

            // settle + glow pulse
            t = 0f;
            while (t < 0.28f)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Sin(t / 0.28f * Mathf.PI);
                target.localScale = endScale * (1.08f - 0.08f * (t / 0.28f) + 0.04f * k);
                yield return null;
            }
            target.localScale = endScale;
            if (img != null) img.color = baseColor;
            onDone?.Invoke();
        }

        IEnumerator FlashThenPop(RectTransform target, Action onDone)
        {
            if (target == null)
            {
                onDone?.Invoke();
                yield break;
            }

            var img = target.GetComponent<Image>();
            Color baseColor = img != null ? img.color : Color.white;
            if (img != null) img.color = Color.Lerp(baseColor, Color.white, 0.7f);

            float t = 0f;
            Vector3 baseScale = target.localScale;
            while (t < 0.06f)
            {
                t += Time.unscaledDeltaTime;
                target.localScale = baseScale * (1f + 0.25f * (t / 0.06f));
                yield return null;
            }

            bool done = false;
            PopOut(target, () => done = true);
            while (!done) yield return null;
            onDone?.Invoke();
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
            const float dur = 0.14f;
            Vector3 baseScale = target.localScale;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                float ease = k * k;
                target.localScale = baseScale * Mathf.Max(0.01f, 1f - ease);
                yield return null;
            }
            target.localScale = baseScale * 0.01f;
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
            target.localScale = Vector3.one;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                // ease-in then small settle
                float ease = 1f - Mathf.Pow(1f - k, 2.6f);
                target.anchoredPosition = Vector2.Lerp(start, end, ease);
                yield return null;
            }
            target.anchoredPosition = end;

            // tiny bounce
            float b = 0f;
            while (b < 0.08f)
            {
                b += Time.unscaledDeltaTime;
                float k = Mathf.Sin(b / 0.08f * Mathf.PI) * 0.06f;
                target.localScale = Vector3.one * (1f + k);
                yield return null;
            }
            target.localScale = Vector3.one;
            onDone?.Invoke();
        }

        IEnumerator SwapRoutine(RectTransform a, RectTransform b, float duration, Action onDone)
        {
            if (a == null || b == null)
            {
                onDone?.Invoke();
                yield break;
            }

            Vector2 a0 = a.anchoredPosition;
            Vector2 b0 = b.anchoredPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                float ease = k * k * (3f - 2f * k);
                a.anchoredPosition = Vector2.Lerp(a0, b0, ease);
                b.anchoredPosition = Vector2.Lerp(b0, a0, ease);
                yield return null;
            }
            a.anchoredPosition = a0;
            b.anchoredPosition = b0;
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

using UnityEngine;
using UnityEngine.EventSystems;

namespace LuminaMatch.UI
{
    /// <summary>Drag a gem toward a neighbor to swap (Royal Match style). Tap still works for boosters / fallback.</summary>
    public class GemCellInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerClickHandler
    {
        public int X;
        public int Y;
        public BoardPresenter Owner;

        bool _dragging;
        bool _swapFired;
        Vector2 _pressScreen;

        public void OnPointerDown(PointerEventData eventData)
        {
            _dragging = true;
            _swapFired = false;
            _pressScreen = eventData.position;
            Owner?.NotifyPointerDown(X, Y);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || _swapFired || Owner == null) return;

            Vector2 delta = eventData.position - _pressScreen;
            float threshold = Mathf.Max(28f, Owner.CellSize * 0.28f);
            if (delta.magnitude < threshold) return;

            int dx = 0, dy = 0;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                dx = delta.x > 0 ? 1 : -1;
            else
                dy = delta.y > 0 ? 1 : -1;

            _swapFired = Owner.TryRequestDragSwap(X, Y, X + dx, Y + dy);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _dragging = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Ignore click if this gesture already swapped via drag.
            if (_swapFired) return;
            Owner?.NotifyCellClicked(X, Y);
        }
    }
}

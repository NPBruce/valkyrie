using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// A custom version of InputField class witch redefines
    /// the OnSelect event to disable the board Panning when editing
    /// </summary>
    public class PanCancelInputField : InputField
    {
        // Constructor removed as it is invalid for MonoBehaviours
        
        public UnityEvent onSelectEvent = new UnityEvent();
        public UnityEvent onDeselectEvent = new UnityEvent();

        public override void OnSelect(BaseEventData eventData)
        {
            CameraController.panDisable = true;
            base.OnSelect(eventData);
            this.caretPosition = lastCaretPosition;
            this.selectionAnchorPosition = lastCaretPosition;
            this.selectionFocusPosition = lastCaretPosition;
            onSelectEvent.Invoke();
        }

        private int lastCaretPosition = 0;

        public int getLastCaretPosition()
        {
            return lastCaretPosition;
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            lastCaretPosition = this.caretPosition;
            CameraController.panDisable = false;
            base.OnDeselect(eventData);
            onDeselectEvent.Invoke();
        }
    }

}

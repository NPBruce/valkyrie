using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// A custom version of InputField class witch redefines
    /// the OnSelect event to disable the board Panning when editing
    /// </summary>
    public class PanCancelInputField : InputField
    {
        public PanCancelInputField() : base() {}

        /// <summary>
        /// When selecting the component, the pan is disabled
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnSelect(BaseEventData eventData)
        {
            CameraController.panDisable = true;
            base.OnSelect(eventData);
            this.caretPosition = lastCaretPosition;
            this.selectionAnchorPosition = lastCaretPosition;
            this.selectionFocusPosition = lastCaretPosition;
        }

        private int lastCaretPosition = 0;

        /// <summary>
        /// The caret reset after deselect. We must store one
        /// in order to know where to insert special characters.
        /// </summary>
        /// <returns></returns>
        public int getLastCaretPosition()
        {
            return lastCaretPosition;
        }

        /// <summary>
        /// When deselecting the component, the pan is enabled
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnDeselect(BaseEventData eventData)
        {
            lastCaretPosition = this.caretPosition;
            CameraController.panDisable = false;
            base.OnDeselect(eventData);
        }
    }

}

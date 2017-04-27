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
        }

        /// <summary>
        /// When deselecting the component, the pan is enabled
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnDeselect(BaseEventData eventData)
        {
            CameraController.panDisable = false;
            base.OnDeselect(eventData);
        }
    }

}

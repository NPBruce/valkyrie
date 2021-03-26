using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class UITtsSpeakButton : UIElement
    {
        private static readonly string PLAY_LABEL = "\u25B6";
        private static readonly string STOP_LABEL = "\u25FC";

        private bool playing;
        
        public UITtsSpeakButton(UIElement textElement) : base(textElement.GetTag(), textElement.GetTransform())
        {
            var transform = GetRectTransform();
            var size = UIScaler.GetPixelsPerUnit() * 1.3f;
            
            transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 2f, size);
            transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 2f, size);
            UpdateButtonLabel();
            SetBGColor(Color.clear);
            SetFontSize(UIScaler.GetSmallFont());
            Game.game.audioControl.ttsAudioSourceStartStopHandler = audioSource =>
            {
                if (ObjectDestroyed())
                {
                    Game.game.audioControl.ttsAudioSourceStartStopHandler = null;
                }
                else
                {
                    playing = audioSource.isPlaying;
                    UpdateButtonLabel();
                }
            };
            SetButton(() =>
            {
                if (playing)
                {
                    Game.game.audioControl.StopTts();
                }
                else
                {
                    GoogleTTSClient.SpeakText(textElement.GetText());
                }
            });
        }

        private void UpdateButtonLabel()
        {
            SetText(playing ? STOP_LABEL : PLAY_LABEL);
        }
    }
}
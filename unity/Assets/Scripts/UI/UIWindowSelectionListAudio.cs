using System;
using UnityEngine;
using Assets.Scripts.Content;
using System.IO;

namespace Assets.Scripts.UI
{
    public class UIWindowSelectionListAudio : UIWindowSelectionListTraits
    {
        private static readonly Color AVAILABLE_COLOR = new Color(0.6f, 0.6f, 1f);
        private static readonly Color UNAVAILABLE_COLOR = new Color(0.3f, 0.3f, 0.3f);

        public UIWindowSelectionListAudio(UnityEngine.Events.UnityAction<string> call, string title = "") : base(call, title)
        {
        }

        public UIWindowSelectionListAudio(UnityEngine.Events.UnityAction<string> call, StringKey title) : base(call, title)
        {
        }

        protected override float DrawItem(SelectionItemTraits item, Transform transform, float offset)
        {
            string key = item.GetKey();
            UIElement ui = new UIElement(transform);
            ui.SetLocation(0, offset, 18.9f, 1);
            var audioFilePath = FindAudioPathIfExists(key);
            if (!string.IsNullOrEmpty(audioFilePath))
            {
                ui.SetButton(delegate { SelectItem(key); });
            }
            ui.SetBGColor(item.GetColor());
            ui.SetText(item.GetDisplay(), Color.black);

            ui = new UIElement(transform);
            ui.SetLocation(19, offset, 1, 1);
            ui.SetButton(delegate { Play(key); });
            var rightButtonColor = !string.IsNullOrEmpty(audioFilePath) ? AVAILABLE_COLOR : UNAVAILABLE_COLOR;
            ui.SetBGColor(rightButtonColor);
            var buttonText = !string.IsNullOrEmpty(audioFilePath) ? "►" : "■";
            ui.SetText(buttonText, Color.black);
            // this character is strange
            ui.SetFontSize(Mathf.RoundToInt((float)UIScaler.GetSmallFont() * 0.5f));

            return offset + 1.05f;
        }

        protected void Play(string key)
        {
            Game game = Game.Get();
            game.audioControl.StopAudioEffect();
            var audioPath = FindAudioPathIfExists(key);
            
            if (String.IsNullOrEmpty(audioPath))
            {
                return;
            }
            game.audioControl.Play(audioPath);
        }

        private string FindAudioPathIfExists(string key)
        {
            if (key == null)
            {
                return null;
            }

            var audioPath = ConvertKeyToPath(key);
            if (!File.Exists(audioPath))
            {
                return null;
            }

            return audioPath;
        }

        private string ConvertKeyToPath(string key)
        {
            var game = Game.Get();
            
            // FFG/Valkyrie audio
            if (game.cd.TryGet(key, out AudioData audioData))
            {
                return audioData.file;
            }
            
            // Custom Quest audio
            return Path.GetDirectoryName(Game.Get().quest.qd.questPath) + Path.DirectorySeparatorChar + key;
        }
    }
}

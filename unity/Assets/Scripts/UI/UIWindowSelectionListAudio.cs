using UnityEngine;
using Assets.Scripts.Content;
using System.IO;

namespace Assets.Scripts.UI
{
    public class UIWindowSelectionListAudio : UIWindowSelectionListTraits
    {
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
            if (key != null)
            {
                ui.SetButton(delegate { SelectItem(key); });
            }
            ui.SetBGColor(item.GetColor());
            ui.SetText(item.GetDisplay(), Color.black);

            ui = new UIElement(transform);
            ui.SetLocation(19, offset, 1, 1);
            if (key != null)
            {
                ui.SetButton(delegate { Play(key); });
            }
            ui.SetBGColor(new Color(0.6f, 0.6f, 1));
            ui.SetText("►", Color.black);
            // this character is strange
            ui.SetFontSize(Mathf.RoundToInt((float)UIScaler.GetSmallFont() * 0.5f));

            return offset + 1.05f;
        }

        protected void Play(string key)
        {
            Game game = Game.Get();
            if (game.cd.audio.ContainsKey(key))
            {
                game.audioControl.Play(game.cd.audio[key].file);
            }
            else
            {
                string path = Path.GetDirectoryName(Game.Get().quest.qd.questPath) + "/" + key;
                game.audioControl.Play(path);
            }
        }
    }
}

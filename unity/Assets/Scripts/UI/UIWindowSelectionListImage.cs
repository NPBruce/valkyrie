using UnityEngine;
using Assets.Scripts.Content;
using System.IO;

namespace Assets.Scripts.UI
{
    class UIWindowSelectionListImage : UIWindowSelectionListTraits
    {
        public UIWindowSelectionListImage(UnityEngine.Events.UnityAction<string> call, string title = "") : base(call, title)
        {
        }

        public UIWindowSelectionListImage(UnityEngine.Events.UnityAction<string> call, StringKey title) : base(call, title)
        {
        }

        protected override void DrawItemList()
        {
            UIElementScrollVertical itemScrollArea = new UIElementScrollVertical();
            itemScrollArea.SetLocation(UIScaler.GetHCenter(-3.5f), 2, 21, 25);
            new UIElementBorder(itemScrollArea);

            float offset = 0;
            float xOffset = 0;
            foreach (SelectionItemTraits item in traitItems)
            {
                bool display = true;
                foreach (TraitGroup tg in traitData)
                {
                    display &= tg.ActiveItem(item);
                }

                if (!display) continue;

                xOffset = DrawItem(item, itemScrollArea.GetScrollTransform(), offset, xOffset);
                if (xOffset > 18)
                {
                    offset += 4;
                    xOffset = 0;
                }
            }
            if (xOffset != 0)
            {
                offset += 4;
            }
            itemScrollArea.SetScrollSize(offset);
        }


        protected float DrawItem(SelectionItemTraits item, Transform transform, float offset, float xOffset)
        {
            Game game = Game.Get();
            string key = item.GetKey();

            UIElement ui = new UIElement(transform);
            ui.SetLocation(xOffset, offset, 3.95f, 3.95f);
            ui.SetButton(delegate { SelectItem(key); });
            ui.SetBGColor(item.GetColor());

            if (game.cd.tokens.ContainsKey(key))
            {
                Vector2 texPos = new Vector2(game.cd.tokens[key].x, game.cd.tokens[key].y);
                Vector2 texSize = new Vector2(game.cd.tokens[key].width, game.cd.tokens[key].height);
                ui.SetImage(ContentData.FileToTexture(game.cd.tokens[key].image, texPos, texSize));
            }
            else
            {
                ui.SetText(item.GetDisplay(), Color.black);
            }
            return xOffset + 4;
        }
    }
}

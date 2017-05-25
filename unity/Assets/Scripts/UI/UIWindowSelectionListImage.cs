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
            Game game = Game.Get();
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

                if (game.cd.tokens.ContainsKey(item.GetKey()))
                {
                    xOffset = DrawItem(item, itemScrollArea.GetScrollTransform(), game.cd.tokens[item.GetKey()], offset, xOffset);
                }
                else if (game.cd.images.ContainsKey(item.GetKey()))
                {
                    xOffset = DrawItem(item, itemScrollArea.GetScrollTransform(), game.cd.images[item.GetKey()], offset, xOffset);
                }
                else if (game.cd.tileSides.ContainsKey(item.GetKey()))
                {
                    xOffset = DrawItem(item, itemScrollArea.GetScrollTransform(), game.cd.tileSides[item.GetKey()], offset, xOffset);
                }
                else if (File.Exists(Path.GetDirectoryName(game.quest.qd.questPath) + "/" + item.GetKey()))
                {
                    xOffset = DrawItem(item.GetKey(), item.GetColor(), itemScrollArea.GetScrollTransform(), ContentData.FileToTexture(Path.GetDirectoryName(game.quest.qd.questPath) + "/" + item.GetKey()), offset, xOffset);
                }
                else
                {
                    if (xOffset > 0) offset += 4;
                    xOffset = 0;
                    offset = DrawItem(item, itemScrollArea.GetScrollTransform(), offset);
                }

                if (xOffset > 16)
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

        protected float DrawItem(SelectionItemTraits item, Transform transform, TokenData token, float offset, float xOffset)
        {
            Vector2 texPos = new Vector2(token.x, token.y);
            Vector2 texSize = new Vector2(token.width, token.height);
            Texture2D tex = ContentData.FileToTexture(token.image, texPos, texSize);
            return DrawItem(item.GetKey(), item.GetColor(), transform, tex, offset, xOffset);
        }

        protected float DrawItem(SelectionItemTraits item, Transform transform, TileSideData tile, float offset, float xOffset)
        {
            Texture2D tex = ContentData.FileToTexture(tile.image);
            return DrawItem(item.GetKey(), item.GetColor(), transform, tex, offset, xOffset);
        }

        protected float DrawItem(string key, Color color, Transform transform, Texture2D tex, float offset, float xOffset)
        {
            UIElement ui = new UIElement(transform);
            ui.SetButton(delegate { SelectItem(key); });
            ui.SetBGColor(color);

            float width = 3.95f;
            if (tex.height <= tex.width)
            {
                ui.SetImage(Sprite.Create(tex, new Rect(0, 0, tex.height, tex.height), Vector2.zero, 1));
                ui.SetLocation(xOffset, offset, width, width);
            }
            else
            {
                width = width * tex.width / tex.height;
                ui.SetImage(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1));
                ui.SetLocation(xOffset, offset, width, 3.95f);
            }
            return xOffset + width + 0.05f;
        }

    }
}

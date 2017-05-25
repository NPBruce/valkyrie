using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;
using System.IO;

namespace Assets.Scripts.UI
{
    class UIWindowSelectionListImage : UIWindowSelectionListTraits
    {
        protected Dictionary<string, ItemDraw> spriteCache = new Dictionary<string, ItemDraw>();

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

                if (spriteCache.ContainsKey(item.GetKey()))
                {
                    xOffset = DrawItem(item.GetKey(), itemScrollArea.GetScrollTransform(), offset, xOffset);
                }
                else if (game.cd.tokens.ContainsKey(item.GetKey()))
                {
                    xOffset = DrawItem(item, itemScrollArea.GetScrollTransform(), game.cd.tokens[item.GetKey()], offset, xOffset);
                }
                else if (game.cd.puzzles.ContainsKey(item.GetKey()))
                {
                    xOffset = DrawItem(item, itemScrollArea.GetScrollTransform(), game.cd.puzzles[item.GetKey()], offset, xOffset);
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

        protected float DrawItem(SelectionItemTraits item, Transform transform, PuzzleData puzzle, float offset, float xOffset)
        {
            return DrawItem(item.GetKey(), item.GetColor(), transform, ContentData.FileToTexture(puzzle.image), offset, xOffset);
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

            spriteCache.Add(key, new ItemDraw());

            spriteCache[key].width = 3.95f;
            if (tex.height <= tex.width)
            {
                spriteCache[key].sprite = Sprite.Create(tex, new Rect(0, 0, tex.height, tex.height), Vector2.zero, 1);
            }
            else
            {
                spriteCache[key].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero, 1);
                spriteCache[key].width = spriteCache[key].width * tex.width / tex.height;
            }
            return DrawItem(key, transform, offset, xOffset);
        }

        protected float DrawItem(string key, Transform transform, float offset, float xOffset)
        {
            UIElement ui = new UIElement(transform);
            ui.SetButton(delegate { SelectItem(key); });
            ui.SetBGColor(spriteCache[key].color);
            ui.SetImage(spriteCache[key].sprite);
            ui.SetLocation(xOffset, offset, spriteCache[key].width, 3.95f);
            return xOffset + spriteCache[key].width + 0.05f;
        }

        protected class ItemDraw
        {
            public Sprite sprite;
            public Color color;
            public float width;
        }
    }
}

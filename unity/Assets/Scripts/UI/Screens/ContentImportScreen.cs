// unity/Assets/Scripts/UI/Screens/ContentImportScreen.cs
using System.IO;
using Assets.Scripts.Content;
using Assets.Scripts.UI;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.UI.Screens
{
    public class ContentImportScreen
    {
        private readonly string gameType;
        private readonly bool needImport;

        private static readonly StringKey IMPORT_SCREEN_TITLE    = new StringKey("val", "IMPORT_SCREEN_TITLE");
        private static readonly StringKey IMPORT_SCREEN_SUBTITLE = new StringKey("val", "IMPORT_SCREEN_SUBTITLE");
        private static readonly StringKey IMPORT_FROM_APP        = new StringKey("val", "IMPORT_FROM_APP");
        private static readonly StringKey IMPORT_FROM_APP_DESC   = new StringKey("val", "IMPORT_FROM_APP_DESC");
        private static readonly StringKey IMPORT_LINUX_UNAVAILABLE = new StringKey("val", "IMPORT_LINUX_UNAVAILABLE");
        private static readonly StringKey IMPORT_FROM_ZIP        = new StringKey("val", "IMPORT_FROM_ZIP");
        private static readonly StringKey IMPORT_FROM_ZIP_DESC   = new StringKey("val", "IMPORT_FROM_ZIP_DESC");
        private static readonly StringKey D2E_NAME               = new StringKey("val", "D2E_NAME");
        private static readonly StringKey MOM_NAME               = new StringKey("val", "MOM_NAME");

        public ContentImportScreen(string gameType, bool needImport)
        {
            this.gameType = gameType;
            this.needImport = needImport;
            Game.Get().contentImport = this;
            Draw();
        }

        private void Draw()
        {
            Destroyer.Destroy();

            float centerX = (UIScaler.GetWidthUnits() - 30) / 2;
            bool appFound = ImportManager.ImportAvailable(gameType);
            bool isAndroid = Application.platform == RuntimePlatform.Android;
            bool isLinux   = Application.platform == RuntimePlatform.LinuxPlayer;
            string gameName = GetGameName();

            // Back button
            UIElement back = new UIElement();
            back.SetLocation(1, UIScaler.GetBottom(-3), 8, 2);
            back.SetText(CommonStringKeys.BACK, Color.red);
            back.SetFontSize(UIScaler.GetMediumFont());
            back.SetButton(OnBack);
            back.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(back, Color.red);

            // Title
            UIElement title = new UIElement();
            title.SetLocation(centerX, 1, 30, 2);
            title.SetText(IMPORT_SCREEN_TITLE);
            title.SetFontSize(UIScaler.GetMediumFont());
            title.SetBGColor(Color.clear);

            // Subtitle
            UIElement subtitle = new UIElement();
            subtitle.SetLocation(centerX, 3.5f, 30, 2.5f);
            subtitle.SetText(string.Format(IMPORT_SCREEN_SUBTITLE.Translate(), gameName));
            subtitle.SetFontSize(UIScaler.GetSmallFont());
            subtitle.SetBGColor(Color.clear);

            if (!isAndroid)
                DrawAppCard(centerX, 7f, appFound, isLinux);
            DrawZipCard(centerX, 13f);
        }

        private void DrawAppCard(float x, float y, bool appFound, bool isLinux)
        {
            bool active = !isLinux;
            Color cardColor = active ? Color.white : Color.grey;

            string desc = isLinux
                ? IMPORT_LINUX_UNAVAILABLE.Translate()
                : IMPORT_FROM_APP_DESC.Translate();

            string cardText = IMPORT_FROM_APP.Translate()
                + System.Environment.NewLine + desc;

            UIElement card = new UIElement();
            card.SetLocation(x, y, 30, 3f);
            card.SetText(cardText, cardColor);
            card.SetFontSize(UIScaler.GetSmallFont());
            card.SetBGColor(new Color(0, 0.03f, 0f));
            if (active)
                card.SetButton(OnImportFromApp);
            new UIElementBorder(card, cardColor);
        }

        private void DrawZipCard(float x, float y)
        {
            string cardText = IMPORT_FROM_ZIP.Translate()
                + System.Environment.NewLine + IMPORT_FROM_ZIP_DESC.Translate();

            UIElement card = new UIElement();
            card.SetLocation(x, y, 30, 3f);
            card.SetText(cardText);
            card.SetFontSize(UIScaler.GetSmallFont());
            card.SetButton(OnImportFromZip);
            card.SetBGColor(new Color(0, 0.03f, 0f));
            new UIElementBorder(card);
        }

        private void OnBack()
        {
            Game.Get().contentImport = null;
            Game.Get().gameSelect.Draw();
        }

        private void OnImportFromApp()
        {
            bool appFound = ImportManager.ImportAvailable(gameType);

            if (appFound)
            {
                // Auto-import: AppFinder already located the install directory
                ImportManager.Import(gameType, null, OnImportComplete);
                return;
            }

            // App not found — open file picker pre-populated at Steam/Applications hint
            string appFilename = gameType.Equals(ValkyrieConstants.typeDescent)
                ? "Road to Legend"
                : "Mansions of Madness";

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            string extension = "app";
#else
            string extension = "exe";
#endif

            string hintPath = ImportManager.GetInstallHintPath();
            string pickedPath = NativeFileDialog.OpenFilePanel("Locate " + appFilename, hintPath, extension);
            if (string.IsNullOrEmpty(pickedPath)) return;

            string dataPath;
            if (Application.platform == RuntimePlatform.OSXPlayer)
                dataPath = Path.Combine(pickedPath, "Contents/Resources/Data");
            else
                dataPath = Path.Combine(Path.GetDirectoryName(pickedPath), appFilename + "_Data");

            ValkyrieDebug.Log("Using path: " + dataPath);
            if (!Directory.Exists(dataPath)) return;

            ImportManager.Import(gameType, dataPath, OnImportComplete);
        }

        private void OnImportFromZip()
        {
            ImportManager.ImportZip(gameType, OnImportComplete);
        }

        private void OnImportComplete()
        {
            Game.Get().contentImport = null;
            Game.Get().gameSelect.Draw();
        }

        private string GetGameName()
        {
            if (gameType.Equals(ValkyrieConstants.typeDescent))
                return D2E_NAME.Translate();
            if (gameType.Equals(ValkyrieConstants.typeMom))
                return MOM_NAME.Translate();
            return gameType;
        }

        public void Update()
        {
            ImportManager.Update();
        }
    }
}

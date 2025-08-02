﻿using Assets.Scripts.Content;
using System;

namespace Assets.Scripts
{
    public sealed class ValkyrieConstants
    {
        private static readonly Lazy<ValkyrieConstants> lazy =
            new Lazy<ValkyrieConstants>(() => new ValkyrieConstants());

        public static ValkyrieConstants Instance { get { return lazy.Value; } }

        private ValkyrieConstants()
        {
        }

        public const string typeMom = "MoM";
        public const string typeDescent = "D2E";
        public const string DefaultLanguage = "English";
        public const string customCategoryLabel = "Custom";
        public const string customCategoryName = "Custom";
        public const string ScenarioDownloadContainerExtension = ".valkyrie";
        public const string ContentPackDownloadContainerExtensionAllFileReference = "*" + ScenarioDownloadContainerExtension;
        public const string ContentPackDownloadContainerExtension = ".valkyrieContentPack";
        public const string ScenarioManifestPath = "/manifest.ini";
        public const string ContentPackManifestPath = "/manifest.ini";
        public const string QuestIniFilePath = "/quest.ini";
        public const string RemoteContentPackIniType = "RemoteContentPack";
        public const string ContentPackIniFile = "content_pack.ini";
    }
}

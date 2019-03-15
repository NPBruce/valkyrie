using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System;
using Fabric.Internal.Editor.ThirdParty.xcodeapi.PBX;

namespace Fabric.Internal.Editor.ThirdParty.xcodeapi
{
	using Utils = Fabric.Internal.Editor.ThirdParty.xcodeapi.PBX.Utils;

	using PBXBuildFileSection           = KnownSectionBase<PBXBuildFileData>;
    using PBXFileReferenceSection       = KnownSectionBase<PBXFileReferenceData>;
    using PBXGroupSection               = KnownSectionBase<PBXGroupData>;
    using PBXContainerItemProxySection  = KnownSectionBase<PBXContainerItemProxyData>;
    using PBXReferenceProxySection      = KnownSectionBase<PBXReferenceProxyData>;
    using PBXSourcesBuildPhaseSection   = KnownSectionBase<PBXSourcesBuildPhaseData>;
    using PBXFrameworksBuildPhaseSection= KnownSectionBase<PBXFrameworksBuildPhaseData>;
    using PBXResourcesBuildPhaseSection = KnownSectionBase<PBXResourcesBuildPhaseData>;
    using PBXCopyFilesBuildPhaseSection = KnownSectionBase<PBXCopyFilesBuildPhaseData>;
    using PBXShellScriptBuildPhaseSection = KnownSectionBase<PBXShellScriptBuildPhaseData>;
    using PBXVariantGroupSection        = KnownSectionBase<PBXVariantGroupData>;
    using PBXNativeTargetSection        = KnownSectionBase<PBXNativeTargetData>;
    using PBXTargetDependencySection    = KnownSectionBase<PBXTargetDependencyData>;
    using XCBuildConfigurationSection   = KnownSectionBase<XCBuildConfigurationData>;
    using XCConfigurationListSection    = KnownSectionBase<XCConfigurationListData>;
    using UnknownSection                = KnownSectionBase<PBXObjectData>;

    // Determines the tree the given path is relative to
    public enum PBXSourceTree
    {
        Absolute,   // The path is absolute
        Source,     // The path is relative to the source folder
        Group,      // The path is relative to the folder it's in. This enum is used only internally,
        // do not use it as function parameter
        Build,      // The path is relative to the build products folder
        Developer,  // The path is relative to the developer folder
        Sdk         // The path is relative to the sdk folder
    };

    public class PBXProject
    {
        PBXProjectData m_Data = new PBXProjectData();

        // convenience accessors for public members of data. This is temporary; will be fixed by an interface change
        // of PBXProjectData
        PBXContainerItemProxySection containerItems { get { return m_Data.containerItems; } }
        PBXReferenceProxySection references         { get { return m_Data.references; } }
        PBXSourcesBuildPhaseSection sources         { get { return m_Data.sources; } }
        PBXFrameworksBuildPhaseSection frameworks   { get { return m_Data.frameworks; } }
        PBXResourcesBuildPhaseSection resources     { get { return m_Data.resources; } }
        PBXCopyFilesBuildPhaseSection copyFiles     { get { return m_Data.copyFiles; } }
        PBXShellScriptBuildPhaseSection shellScripts { get { return m_Data.shellScripts; } }
        PBXNativeTargetSection nativeTargets        { get { return m_Data.nativeTargets; } }
        PBXTargetDependencySection targetDependencies { get { return m_Data.targetDependencies; } }
        PBXVariantGroupSection variantGroups        { get { return m_Data.variantGroups; } }
        XCBuildConfigurationSection buildConfigs    { get { return m_Data.buildConfigs; } }
        XCConfigurationListSection configs          { get { return m_Data.configs; } }
        PBXProjectSection project                   { get { return m_Data.project; } }

        PBXBuildFileData BuildFilesGet(string guid) { return m_Data.BuildFilesGet(guid); }
        void BuildFilesAdd(string targetGuid, PBXBuildFileData buildFile) { m_Data.BuildFilesAdd(targetGuid, buildFile); }
        void BuildFilesRemove(string targetGuid, string fileGuid) { m_Data.BuildFilesRemove(targetGuid, fileGuid); }
        PBXBuildFileData BuildFilesGetForSourceFile(string targetGuid, string fileGuid) { return m_Data.BuildFilesGetForSourceFile(targetGuid, fileGuid); }
        IEnumerable<PBXBuildFileData> BuildFilesGetAll() { return m_Data.BuildFilesGetAll(); }
        void FileRefsAdd(string realPath, string projectPath, PBXGroupData parent, PBXFileReferenceData fileRef) { m_Data.FileRefsAdd(realPath, projectPath, parent, fileRef); }
        PBXFileReferenceData FileRefsGet(string guid) { return m_Data.FileRefsGet(guid); }
        PBXFileReferenceData FileRefsGetByRealPath(string path, PBXSourceTree sourceTree) { return m_Data.FileRefsGetByRealPath(path, sourceTree); }
        PBXFileReferenceData FileRefsGetByProjectPath(string path) { return m_Data.FileRefsGetByProjectPath(path); }
        void FileRefsRemove(string guid) { m_Data.FileRefsRemove(guid); }
        PBXGroupData GroupsGet(string guid) { return m_Data.GroupsGet(guid); }
        PBXGroupData GroupsGetByChild(string childGuid) { return m_Data.GroupsGetByChild(childGuid); }
        PBXGroupData GroupsGetMainGroup() { return m_Data.GroupsGetMainGroup(); }
        PBXGroupData GroupsGetByProjectPath(string sourceGroup) { return m_Data.GroupsGetByProjectPath(sourceGroup); }
        void GroupsAdd(string projectPath, PBXGroupData parent, PBXGroupData gr) { m_Data.GroupsAdd(projectPath, parent, gr); }
        void GroupsAddDuplicate(PBXGroupData gr) { m_Data.GroupsAddDuplicate(gr); }
        void GroupsRemove(string guid) { m_Data.GroupsRemove(guid); }
        FileGUIDListBase BuildSectionAny(PBXNativeTargetData target, string path, bool isFolderRef) { return m_Data.BuildSectionAny(target, path, isFolderRef); }


        public static string GetPBXProjectPath(string buildPath)
        {
            return Utils.CombinePaths(buildPath, "Unity-iPhone.xcodeproj/project.pbxproj");
        }

        public static string GetUnityTargetName()
        {
            return "Unity-iPhone";
        }

        public static string GetUnityTestTargetName()
        {
            return "Unity-iPhone Tests";
        }

        internal string ProjectGuid()
        {
            return project.project.guid;
        }

        /// Returns a guid identifying native target with name @a name
        public string TargetGuidByName(string name)
        {
            foreach (var entry in nativeTargets.GetEntries())
                if (entry.Value.name == name)
                    return entry.Key;
            return null;
        }

        public static bool IsKnownExtension(string ext)
        {
            return FileTypeUtils.IsKnownExtension(ext);
        }

        public static bool IsBuildable(string ext)
        {
            return FileTypeUtils.IsBuildableFile(ext);
        }

        // The same file can be referred to by more than one project path.
        private string AddFileImpl(string path, string projectPath, PBXSourceTree tree, bool isFolderReference)
        {
            path = Utils.FixSlashesInPath(path);
            projectPath = Utils.FixSlashesInPath(projectPath);

            if (!isFolderReference && Path.GetExtension(path) != Path.GetExtension(projectPath))
                throw new Exception("Project and real path extensions do not match");

            string guid = FindFileGuidByProjectPath(projectPath);
            if (guid == null)
                guid = FindFileGuidByRealPath(path);
            if (guid == null)
            {
                PBXFileReferenceData fileRef;
                if (isFolderReference)
                    fileRef = PBXFileReferenceData.CreateFromFolderReference(path, Utils.GetFilenameFromPath(projectPath), tree);
                else
                    fileRef = PBXFileReferenceData.CreateFromFile(path, Utils.GetFilenameFromPath(projectPath), tree);
                PBXGroupData parent = CreateSourceGroup(Utils.GetDirectoryFromPath(projectPath));
                parent.children.AddGUID(fileRef.guid);
                FileRefsAdd(path, projectPath, parent, fileRef);
                guid = fileRef.guid;
            }
            return guid;
        }

        // The extension of the files identified by path and projectPath must be the same.
        public string AddFile(string path, string projectPath)
        {
            return AddFileImpl(path, projectPath, PBXSourceTree.Source, false);
        }

        // sourceTree must not be PBXSourceTree.Group
        public string AddFile(string path, string projectPath, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            return AddFileImpl(path, projectPath, sourceTree, false);
        }
        
        public string AddFolderReference(string path, string projectPath)
        {
            return AddFileImpl(path, projectPath, PBXSourceTree.Source, true);
        }
        
        // sourceTree must not be PBXSourceTree.Group
        public string AddFolderReference(string path, string projectPath, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            return AddFileImpl(path, projectPath, sourceTree, true);
        }

        private void AddBuildFileImpl(string targetGuid, string fileGuid, bool weak, string compileFlags)
        {
            PBXNativeTargetData target = nativeTargets[targetGuid];
            PBXFileReferenceData fileRef = FileRefsGet(fileGuid);
            
            string ext = Path.GetExtension(fileRef.path);
 
            if (FileTypeUtils.IsBuildable(ext, fileRef.isFolderReference) &&
                BuildFilesGetForSourceFile(targetGuid, fileGuid) == null)
            {
                PBXBuildFileData buildFile = PBXBuildFileData.CreateFromFile(fileGuid, weak, compileFlags);
                BuildFilesAdd(targetGuid, buildFile);
                BuildSectionAny(target, ext, fileRef.isFolderReference).files.AddGUID(buildFile.guid);
            }
        }

        public void AddFileToBuild(string targetGuid, string fileGuid)
        {
            AddBuildFileImpl(targetGuid, fileGuid, false, null);
        }

        public void AddFileToBuildWithFlags(string targetGuid, string fileGuid, string compileFlags)
        {
            AddBuildFileImpl(targetGuid, fileGuid, false, compileFlags);
        }

        // returns null on error
        // FIXME: at the moment returns all flags as the first element of the array
        public List<string> GetCompileFlagsForFile(string targetGuid, string fileGuid)
        {
            var buildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
            if (buildFile == null)
                return null;
            if (buildFile.compileFlags == null)
                return new List<string>();
            return new List<string>{buildFile.compileFlags};
        }

        public void SetCompileFlagsForFile(string targetGuid, string fileGuid, List<string> compileFlags)
        {
            var buildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
            if (buildFile == null)
                return;
            if (compileFlags == null)
                buildFile.compileFlags = null;
            else
                buildFile.compileFlags = string.Join(" ", compileFlags.ToArray());
        }

        public void AddAssetTagForFile(string targetGuid, string fileGuid, string tag)
        {
            var buildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
            if (buildFile == null)
                return;
            if (!buildFile.assetTags.Contains(tag))
                buildFile.assetTags.Add(tag);
            if (!project.project.knownAssetTags.Contains(tag))
                project.project.knownAssetTags.Add(tag);
        }

        public void RemoveAssetTagForFile(string targetGuid, string fileGuid, string tag)
        {
            var buildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
            if (buildFile == null)
                return;
            buildFile.assetTags.Remove(tag);
            // remove from known tags if this was the last one
            foreach (var buildFile2 in BuildFilesGetAll())
            {
                if (buildFile2.assetTags.Contains(tag))
                    return;
            }
            project.project.knownAssetTags.Remove(tag);
        }

        public void AddAssetTagToDefaultInstall(string targetGuid, string tag)
        {
            if (!project.project.knownAssetTags.Contains(tag))
                return;
            AddBuildProperty(targetGuid, "ON_DEMAND_RESOURCES_INITIAL_INSTALL_TAGS", tag);
        }

        public void RemoveAssetTagFromDefaultInstall(string targetGuid, string tag)
        {
            UpdateBuildProperty(targetGuid, "ON_DEMAND_RESOURCES_INITIAL_INSTALL_TAGS", null, new string[]{tag});   
        }

        public void RemoveAssetTag(string tag)
        {
            foreach (var buildFile in BuildFilesGetAll())
                buildFile.assetTags.Remove(tag);
            foreach (var targetGuid in nativeTargets.GetGuids())
                RemoveAssetTagFromDefaultInstall(targetGuid, tag);
            project.project.knownAssetTags.Remove(tag);
        }

        public bool ContainsFileByRealPath(string path)
        {
            return FindFileGuidByRealPath(path) != null;
        }

        // sourceTree must not be PBXSourceTree.Group
        public bool ContainsFileByRealPath(string path, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            return FindFileGuidByRealPath(path, sourceTree) != null;
        }

        public bool ContainsFileByProjectPath(string path)
        {
            return FindFileGuidByProjectPath(path) != null;
        }

        public bool HasFramework(string framework)
        {
            return ContainsFileByRealPath("System/Library/Frameworks/"+framework);
        }

        /// The framework must be specified with the '.framework' extension
        public void AddFrameworkToProject(string targetGuid, string framework, bool weak)
        {
            string fileGuid = AddFile("System/Library/Frameworks/"+framework, "Frameworks/"+framework, PBXSourceTree.Sdk);
            AddBuildFileImpl(targetGuid, fileGuid, weak, null);
        }

        /// The framework must be specified with the '.framework' extension
        // FIXME: targetGuid is ignored at the moment
        public void RemoveFrameworkFromProject(string targetGuid, string framework)
        {
            string fileGuid = FindFileGuidByRealPath("System/Library/Frameworks/"+framework);
            if (fileGuid != null)
                RemoveFile(fileGuid);
        }

        // sourceTree must not be PBXSourceTree.Group
        public string FindFileGuidByRealPath(string path, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            path = Utils.FixSlashesInPath(path);
            var fileRef = FileRefsGetByRealPath(path, sourceTree);
            if (fileRef != null)
                return fileRef.guid;
            return null;
        }

        public string FindFileGuidByRealPath(string path)
        {
            path = Utils.FixSlashesInPath(path);

            foreach (var tree in FileTypeUtils.AllAbsoluteSourceTrees())
            {
                string res = FindFileGuidByRealPath(path, tree);
                if (res != null)
                    return res;
            }
            return null;
        }

        public string FindFileGuidByProjectPath(string path)
        {
            path = Utils.FixSlashesInPath(path);
            var fileRef = FileRefsGetByProjectPath(path);
            if (fileRef != null)
                return fileRef.guid;
            return null;
        }

        public void RemoveFileFromBuild(string targetGuid, string fileGuid)
        {
            var buildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
            if (buildFile == null)
                return;
            BuildFilesRemove(targetGuid, fileGuid);

            string buildGuid = buildFile.guid;
            if (buildGuid != null)
            {
                foreach (var section in sources.GetEntries())
                    section.Value.files.RemoveGUID(buildGuid);
                foreach (var section in resources.GetEntries())
                    section.Value.files.RemoveGUID(buildGuid);
                foreach (var section in copyFiles.GetEntries())
                    section.Value.files.RemoveGUID(buildGuid);
                foreach (var section in frameworks.GetEntries())
                    section.Value.files.RemoveGUID(buildGuid);
            }
        }

        public void RemoveFile(string fileGuid)
        {
            if (fileGuid == null)
                return;

            // remove from parent
            PBXGroupData parent = GroupsGetByChild(fileGuid);
            if (parent != null)
                parent.children.RemoveGUID(fileGuid);
            RemoveGroupIfEmpty(parent);

            // remove actual file
            foreach (var target in nativeTargets.GetEntries())
                RemoveFileFromBuild(target.Value.guid, fileGuid);
            FileRefsRemove(fileGuid);
        }

        void RemoveGroupIfEmpty(PBXGroupData gr)
        {
            if (gr.children.Count == 0 && gr != GroupsGetMainGroup())
            {
                // remove from parent
                PBXGroupData parent = GroupsGetByChild(gr.guid);
                parent.children.RemoveGUID(gr.guid);
                RemoveGroupIfEmpty(parent);

                // remove actual group
                GroupsRemove(gr.guid);
            }
        }

        private void RemoveGroupChildrenRecursive(PBXGroupData parent)
        {
            List<string> children = new List<string>(parent.children);
            parent.children.Clear();
            foreach (string guid in children)
            {
                PBXFileReferenceData file = FileRefsGet(guid);
                if (file != null)
                {
                    foreach (var target in nativeTargets.GetEntries())
                        RemoveFileFromBuild(target.Value.guid, guid);
                    FileRefsRemove(guid);
                    continue;
                }

                PBXGroupData gr = GroupsGet(guid);
                if (gr != null)
                {
                    RemoveGroupChildrenRecursive(gr);
                    GroupsRemove(gr.guid);
                    continue;
                }
            }
        }

        internal void RemoveFilesByProjectPathRecursive(string projectPath)
        {
            projectPath = Utils.FixSlashesInPath(projectPath);
            PBXGroupData gr = GroupsGetByProjectPath(projectPath);
            if (gr == null)
                return;
            RemoveGroupChildrenRecursive(gr);
            RemoveGroupIfEmpty(gr);
        }

        // Returns null on error
        internal List<string> GetGroupChildrenFiles(string projectPath)
        {
            projectPath = Utils.FixSlashesInPath(projectPath);
            PBXGroupData gr = GroupsGetByProjectPath(projectPath);
            if (gr == null)
                return null;
            var res = new List<string>();
            foreach (var guid in gr.children)
            {
                var fileRef = FileRefsGet(guid);
                if (fileRef != null)
                    res.Add(fileRef.name);
            }
            return res;
        }

        private PBXGroupData GetPBXGroupChildByName(PBXGroupData group, string name)
        {
            foreach (string guid in group.children)
            {
                var gr = GroupsGet(guid);
                if (gr != null && gr.name == name)
                    return gr;
            }
            return null;
        }

        /// Creates source group identified by sourceGroup, if needed, and returns it.
        /// If sourceGroup is empty or null, root group is returned
        private PBXGroupData CreateSourceGroup(string sourceGroup)
        {
            sourceGroup = Utils.FixSlashesInPath(sourceGroup);

            if (sourceGroup == null || sourceGroup == "")
                return GroupsGetMainGroup();

            PBXGroupData gr = GroupsGetByProjectPath(sourceGroup);
            if (gr != null)
                return gr;

            // the group does not exist -- create new
            gr = GroupsGetMainGroup();

            var elements = PBX.Utils.SplitPath(sourceGroup);
            string projectPath = null;
            foreach (string pathEl in elements)
            {
                if (projectPath == null)
                    projectPath = pathEl;
                else
                    projectPath += "/" + pathEl;

                PBXGroupData child = GetPBXGroupChildByName(gr, pathEl);
                if (child != null)
                    gr = child;
                else
                {
                    PBXGroupData newGroup = PBXGroupData.Create(pathEl, pathEl, PBXSourceTree.Group);
                    gr.children.AddGUID(newGroup.guid);
                    GroupsAdd(projectPath, gr, newGroup);
                    gr = newGroup;
                }
            }
            return gr;
        }

        // sourceTree must not be PBXSourceTree.Group
        public void AddExternalProjectDependency(string path, string projectPath, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            path = Utils.FixSlashesInPath(path);
            projectPath = Utils.FixSlashesInPath(projectPath);

            // note: we are duplicating products group for the project reference. Otherwise Xcode crashes.
            PBXGroupData productGroup = PBXGroupData.CreateRelative("Products");
            GroupsAddDuplicate(productGroup); // don't use GroupsAdd here

            PBXFileReferenceData fileRef = PBXFileReferenceData.CreateFromFile(path, Path.GetFileName(projectPath),
                                                                               sourceTree);
            FileRefsAdd(path, projectPath, null, fileRef);
            CreateSourceGroup(Utils.GetDirectoryFromPath(projectPath)).children.AddGUID(fileRef.guid);

            project.project.AddReference(productGroup.guid, fileRef.guid);
        }

        /** This function must be called only after the project the library is in has
            been added as a dependency via AddExternalProjectDependency. projectPath must be
            the same as the 'path' parameter passed to the AddExternalProjectDependency.
            remoteFileGuid must be the guid of the referenced file as specified in
            PBXFileReference section of the external project

            TODO: what. is remoteInfo entry in PBXContainerItemProxy? Is in referenced project name or
            referenced library name without extension?
        */
        public void AddExternalLibraryDependency(string targetGuid, string filename, string remoteFileGuid, string projectPath,
                                                 string remoteInfo)
        {
            PBXNativeTargetData target = nativeTargets[targetGuid];
            filename = Utils.FixSlashesInPath(filename);
            projectPath = Utils.FixSlashesInPath(projectPath);

            // find the products group to put the new library in
            string projectGuid = FindFileGuidByRealPath(projectPath);
            if (projectGuid == null)
                throw new Exception("No such project");

            string productsGroupGuid = null;
            foreach (var proj in project.project.projectReferences)
            {
                if (proj.projectRef == projectGuid)
                {
                    productsGroupGuid = proj.group;
                    break;
                }
            }

            if (productsGroupGuid == null)
                throw new Exception("Malformed project: no project in project references");

            PBXGroupData productGroup = GroupsGet(productsGroupGuid);

            // verify file extension
            string ext = Path.GetExtension(filename);
            if (!FileTypeUtils.IsBuildableFile(ext))
                throw new Exception("Wrong file extension");

            // create ContainerItemProxy object
            var container = PBXContainerItemProxyData.Create(projectGuid, "2", remoteFileGuid, remoteInfo);
            containerItems.AddEntry(container);

            // create a reference and build file for the library
            string typeName = FileTypeUtils.GetTypeName(ext);

            var libRef = PBXReferenceProxyData.Create(filename, typeName, container.guid, "BUILT_PRODUCTS_DIR");
            references.AddEntry(libRef);
            PBXBuildFileData libBuildFile = PBXBuildFileData.CreateFromFile(libRef.guid, false, null);
            BuildFilesAdd(targetGuid, libBuildFile);
            BuildSectionAny(target, ext, false).files.AddGUID(libBuildFile.guid);

            // add to products folder
            productGroup.children.AddGUID(libRef.guid);
        }

        private void SetDefaultAppExtensionReleaseBuildFlags(XCBuildConfigurationData config, string infoPlistPath)
        {
            config.AddProperty("ALWAYS_SEARCH_USER_PATHS", "NO");
            config.AddProperty("CLANG_CXX_LANGUAGE_STANDARD", "gnu++0x");
            config.AddProperty("CLANG_CXX_LIBRARY", "libc++");
            config.AddProperty("CLANG_ENABLE_MODULES", "YES");
            config.AddProperty("CLANG_ENABLE_OBJC_ARC", "YES");
            config.AddProperty("CLANG_WARN_BOOL_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_CONSTANT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_DIRECT_OBJC_ISA_USAGE", "YES_ERROR");
            config.AddProperty("CLANG_WARN_EMPTY_BODY", "YES");
            config.AddProperty("CLANG_WARN_ENUM_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_INT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_OBJC_ROOT_CLASS", "YES_ERROR");
            config.AddProperty("CLANG_WARN_UNREACHABLE_CODE", "YES");
            config.AddProperty("CLANG_WARN__DUPLICATE_METHOD_MATCH", "YES");
            config.AddProperty("COPY_PHASE_STRIP", "YES");
            config.AddProperty("ENABLE_NS_ASSERTIONS", "NO");
            config.AddProperty("ENABLE_STRICT_OBJC_MSGSEND", "YES");
            config.AddProperty("GCC_C_LANGUAGE_STANDARD", "gnu99");
            config.AddProperty("GCC_WARN_64_TO_32_BIT_CONVERSION", "YES");
            config.AddProperty("GCC_WARN_ABOUT_RETURN_TYPE", "YES_ERROR");
            config.AddProperty("GCC_WARN_UNDECLARED_SELECTOR", "YES");
            config.AddProperty("GCC_WARN_UNINITIALIZED_AUTOS", "YES_AGGRESSIVE");
            config.AddProperty("GCC_WARN_UNUSED_FUNCTION", "YES");
            config.AddProperty("INFOPLIST_FILE", infoPlistPath);
            config.AddProperty("IPHONEOS_DEPLOYMENT_TARGET", "8.0");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "$(inherited)");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/../../Frameworks");
            config.AddProperty("MTL_ENABLE_DEBUG_INFO", "NO");
            config.AddProperty("PRODUCT_NAME", "$(TARGET_NAME)");
            config.AddProperty("SKIP_INSTALL", "YES");
            config.AddProperty("VALIDATE_PRODUCT", "YES");
        }

        private void SetDefaultAppExtensionDebugBuildFlags(XCBuildConfigurationData config, string infoPlistPath)
        {
            config.AddProperty("ALWAYS_SEARCH_USER_PATHS", "NO");
            config.AddProperty("CLANG_CXX_LANGUAGE_STANDARD", "gnu++0x");
            config.AddProperty("CLANG_CXX_LIBRARY", "libc++");
            config.AddProperty("CLANG_ENABLE_MODULES", "YES");
            config.AddProperty("CLANG_ENABLE_OBJC_ARC", "YES");
            config.AddProperty("CLANG_WARN_BOOL_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_CONSTANT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_DIRECT_OBJC_ISA_USAGE", "YES_ERROR");
            config.AddProperty("CLANG_WARN_EMPTY_BODY", "YES");
            config.AddProperty("CLANG_WARN_ENUM_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_INT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_OBJC_ROOT_CLASS", "YES_ERROR");
            config.AddProperty("CLANG_WARN_UNREACHABLE_CODE", "YES");
            config.AddProperty("CLANG_WARN__DUPLICATE_METHOD_MATCH", "YES");
            config.AddProperty("COPY_PHASE_STRIP", "NO");
            config.AddProperty("ENABLE_STRICT_OBJC_MSGSEND", "YES");
            config.AddProperty("GCC_C_LANGUAGE_STANDARD", "gnu99");
            config.AddProperty("GCC_DYNAMIC_NO_PIC", "NO");
            config.AddProperty("GCC_OPTIMIZATION_LEVEL", "0");
            config.AddProperty("GCC_PREPROCESSOR_DEFINITIONS", "DEBUG=1");
            config.AddProperty("GCC_PREPROCESSOR_DEFINITIONS", "$(inherited)");
            config.AddProperty("GCC_SYMBOLS_PRIVATE_EXTERN", "NO");
            config.AddProperty("GCC_WARN_64_TO_32_BIT_CONVERSION", "YES");
            config.AddProperty("GCC_WARN_ABOUT_RETURN_TYPE", "YES_ERROR");
            config.AddProperty("GCC_WARN_UNDECLARED_SELECTOR", "YES");
            config.AddProperty("GCC_WARN_UNINITIALIZED_AUTOS", "YES_AGGRESSIVE");
            config.AddProperty("GCC_WARN_UNUSED_FUNCTION", "YES");
            config.AddProperty("INFOPLIST_FILE", infoPlistPath);
            config.AddProperty("IPHONEOS_DEPLOYMENT_TARGET", "8.0");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "$(inherited)");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/../../Frameworks");
            config.AddProperty("MTL_ENABLE_DEBUG_INFO", "YES");
            config.AddProperty("ONLY_ACTIVE_ARCH", "YES");
            config.AddProperty("PRODUCT_NAME", "$(TARGET_NAME)");
            config.AddProperty("SKIP_INSTALL", "YES");
        }

        internal PBXNativeTargetData CreateNewTarget(string name, string ext, string type)
        {
            // create build configurations
            var releaseBuildConfig = XCBuildConfigurationData.Create("Release");
            buildConfigs.AddEntry(releaseBuildConfig);
            
            var debugBuildConfig = XCBuildConfigurationData.Create("Debug");
            buildConfigs.AddEntry(debugBuildConfig);
            
            var buildConfigList = XCConfigurationListData.Create();
            configs.AddEntry(buildConfigList);
            buildConfigList.buildConfigs.AddGUID(releaseBuildConfig.guid);
            buildConfigList.buildConfigs.AddGUID(debugBuildConfig.guid);
            
            // create build file reference
            string fullName = name + ext;
            var productFileRef = AddFile(fullName, "Products/" + fullName, PBXSourceTree.Build);
            var newTarget = PBXNativeTargetData.Create(name, productFileRef, type, buildConfigList.guid);
            nativeTargets.AddEntry(newTarget);
            project.project.targets.Add(newTarget.guid);
            
            return newTarget;
        }

        // Returns the guid of the new target
        internal string AddAppExtension(string mainTarget, string name, string infoPlistPath)
        {
            string ext = ".appex";
            var newTarget = CreateNewTarget(name, ext, "com.apple.product-type.app-extension");
            
            SetDefaultAppExtensionReleaseBuildFlags(buildConfigs[BuildConfigByName(newTarget.guid, "Release")], infoPlistPath);
            SetDefaultAppExtensionDebugBuildFlags(buildConfigs[BuildConfigByName(newTarget.guid, "Debug")], infoPlistPath);

            var sourcesBuildPhase = PBXSourcesBuildPhaseData.Create();
            sources.AddEntry(sourcesBuildPhase);
            newTarget.phases.AddGUID(sourcesBuildPhase.guid);

            var resourcesBuildPhase = PBXResourcesBuildPhaseData.Create();
            resources.AddEntry(resourcesBuildPhase);
            newTarget.phases.AddGUID(resourcesBuildPhase.guid);

            var frameworksBuildPhase = PBXFrameworksBuildPhaseData.Create();
            frameworks.AddEntry(frameworksBuildPhase);
            newTarget.phases.AddGUID(frameworksBuildPhase.guid);

            var copyFilesBuildPhase = PBXCopyFilesBuildPhaseData.Create("Embed App Extensions", "13");
            copyFiles.AddEntry(copyFilesBuildPhase);
            nativeTargets[mainTarget].phases.AddGUID(copyFilesBuildPhase.guid);

            var containerProxy = PBXContainerItemProxyData.Create(project.project.guid, "1", newTarget.guid, name);
            containerItems.AddEntry(containerProxy);

            var targetDependency = PBXTargetDependencyData.Create(newTarget.guid, containerProxy.guid);
            targetDependencies.AddEntry(targetDependency);

            nativeTargets[mainTarget].dependencies.AddGUID(targetDependency.guid);

            var buildAppCopy = PBXBuildFileData.CreateFromFile(FindFileGuidByProjectPath("Products/" + name + ext), false, "");
            BuildFilesAdd(mainTarget, buildAppCopy);
            copyFilesBuildPhase.files.AddGUID(buildAppCopy.guid);

            AddFile(infoPlistPath, name + "/Supporting Files/Info.plist", PBXSourceTree.Source);

            return newTarget.guid;
        }

        public string BuildConfigByName(string targetGuid, string name)
        {
            PBXNativeTargetData target = nativeTargets[targetGuid];
            foreach (string guid in configs[target.buildConfigList].buildConfigs)
            {
                var buildConfig = buildConfigs[guid];
                if (buildConfig != null && buildConfig.name == name)
                    return buildConfig.guid;
            }
            return null;
        }

        string GetConfigListForTarget(string targetGuid)
        {
            if (targetGuid == project.project.guid)
                return project.project.buildConfigList;
            else
                return nativeTargets[targetGuid].buildConfigList;
        }

        // Adds an item to a build property that contains a value list. Duplicate build properties
        // are ignored. Values for name "LIBRARY_SEARCH_PATHS" are quoted if they contain spaces.
        // targetGuid may refer to PBXProject object
        public void AddBuildProperty(string targetGuid, string name, string value)
        {
            foreach (string guid in configs[GetConfigListForTarget(targetGuid)].buildConfigs)
                AddBuildPropertyForConfig(guid, name, value);
        }

        public void AddBuildProperty(IEnumerable<string> targetGuids, string name, string value)
        {
            foreach (string t in targetGuids)
                AddBuildProperty(t, name, value);
        }
        public void AddBuildPropertyForConfig(string configGuid, string name, string value)
        {
            buildConfigs[configGuid].AddProperty(name, value);
        }

        public void AddBuildPropertyForConfig(IEnumerable<string> configGuids, string name, string value)
        {
            foreach (string guid in configGuids)
                AddBuildPropertyForConfig(guid, name, value);
        }
        // targetGuid may refer to PBXProject object
        public void SetBuildProperty(string targetGuid, string name, string value)
        {
            foreach (string guid in configs[GetConfigListForTarget(targetGuid)].buildConfigs)
                SetBuildPropertyForConfig(guid, name, value);
        }
        public void SetBuildProperty(IEnumerable<string> targetGuids, string name, string value)
        {
            foreach (string t in targetGuids)
                SetBuildProperty(t, name, value);
        }
        public void SetBuildPropertyForConfig(string configGuid, string name, string value)
        {
            buildConfigs[configGuid].SetProperty(name, value);
        }
        public void SetBuildPropertyForConfig(IEnumerable<string> configGuids, string name, string value)
        {
            foreach (string guid in configGuids)
                SetBuildPropertyForConfig(guid, name, value);
        }

        internal void RemoveBuildProperty(string targetGuid, string name)
        {
            foreach (string guid in configs[GetConfigListForTarget(targetGuid)].buildConfigs)
                RemoveBuildPropertyForConfig(guid, name);
        }
        internal void RemoveBuildProperty(IEnumerable<string> targetGuids, string name)
        {
            foreach (string t in targetGuids)
                RemoveBuildProperty(t, name);
        }
        internal void RemoveBuildPropertyForConfig(string configGuid, string name)
        {
            buildConfigs[configGuid].RemoveProperty(name);
        }
        internal void RemoveBuildPropertyForConfig(IEnumerable<string> configGuids, string name)
        {
            foreach (string guid in configGuids)
                RemoveBuildPropertyForConfig(guid, name);
        }

        /// Interprets the value of the given property as a set of space-delimited strings, then
        /// removes strings equal to items to removeValues and adds strings in addValues.
        public void UpdateBuildProperty(string targetGuid, string name, 
                                        IEnumerable<string> addValues, IEnumerable<string> removeValues)
        {
            foreach (string guid in configs[GetConfigListForTarget(targetGuid)].buildConfigs)
                UpdateBuildPropertyForConfig(guid, name, addValues, removeValues);
        }
        public void UpdateBuildProperty(IEnumerable<string> targetGuids, string name, 
                                        IEnumerable<string> addValues, IEnumerable<string> removeValues)
        {
            foreach (string t in targetGuids)
                UpdateBuildProperty(t, name, addValues, removeValues);
        }
        public void UpdateBuildPropertyForConfig(string configGuid, string name, 
                                                 IEnumerable<string> addValues, IEnumerable<string> removeValues)
        {
            var config = buildConfigs[configGuid];
            if (config != null)
            {
                if (removeValues != null)
                    foreach (var v in removeValues)
                        config.RemovePropertyValue(name, v);
                if (addValues != null)
                    foreach (var v in addValues)
                        config.AddProperty(name, v);
            }
        }
        public void UpdateBuildPropertyForConfig(IEnumerable<string> configGuids, string name, 
                                                 IEnumerable<string> addValues, IEnumerable<string> removeValues)
        {
            foreach (string guid in configGuids)
                UpdateBuildProperty(guid, name, addValues, removeValues);
        }

        public void ReadFromFile(string path)
        {
            ReadFromString(File.ReadAllText(path));
        }

        public void ReadFromString(string src)
        {
            TextReader sr = new StringReader(src);
            ReadFromStream(sr);
        }

        public void ReadFromStream(TextReader sr)
        {
            m_Data.ReadFromStream(sr);
        }

        public void WriteToFile(string path)
        {
            File.WriteAllText(path, WriteToString());
        }

        public void WriteToStream(TextWriter sw)
        {
            sw.Write(WriteToString());
        }

        public string WriteToString()
        {
            return m_Data.WriteToString();
        }
    }

} // namespace UnityEditor.iOS.Xcode

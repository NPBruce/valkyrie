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

    internal class PBXProjectData
    {
        private Dictionary<string, SectionBase> m_Section = null;
        private PBXElementDict m_RootElements = null;
        private PBXElementDict m_UnknownObjects = null;
        private string m_ObjectVersion = null;
        private List<string> m_SectionOrder = null;

        private Dictionary<string, UnknownSection> m_UnknownSections;
        private PBXBuildFileSection buildFiles = null; // use BuildFiles* methods instead of manipulating directly
        private PBXFileReferenceSection fileRefs = null; // use FileRefs* methods instead of manipulating directly
        private PBXGroupSection groups = null; // use Groups* methods instead of manipulating directly
        public PBXContainerItemProxySection containerItems = null;
        public PBXReferenceProxySection references = null;
        public PBXSourcesBuildPhaseSection sources = null;
        public PBXFrameworksBuildPhaseSection frameworks = null;
        public PBXResourcesBuildPhaseSection resources = null;
        public PBXCopyFilesBuildPhaseSection copyFiles = null;
        public PBXShellScriptBuildPhaseSection shellScripts = null;
        public PBXNativeTargetSection nativeTargets = null;
        public PBXTargetDependencySection targetDependencies = null;
        public PBXVariantGroupSection variantGroups = null;
        public XCBuildConfigurationSection buildConfigs = null;
        public XCConfigurationListSection configs = null;
        public PBXProjectSection project = null;

        // FIXME: create a separate PBXObject tree to represent these relationships

        // A build file can be represented only once in all *BuildPhaseSection sections, thus
        // we can simplify the cache by not caring about the file extension
        private Dictionary<string, Dictionary<string, PBXBuildFileData>> m_FileGuidToBuildFileMap = null;
        private Dictionary<string, PBXFileReferenceData> m_ProjectPathToFileRefMap = null;
        private Dictionary<string, string> m_FileRefGuidToProjectPathMap = null;
        private Dictionary<PBXSourceTree, Dictionary<string, PBXFileReferenceData>> m_RealPathToFileRefMap = null;
        private Dictionary<string, PBXGroupData> m_ProjectPathToGroupMap = null;
        private Dictionary<string, string> m_GroupGuidToProjectPathMap = null;
        private Dictionary<string, PBXGroupData> m_GuidToParentGroupMap = null;

        public PBXBuildFileData BuildFilesGet(string guid)
        {
            return buildFiles[guid];
        }

        // targetGuid is the guid of the target that contains the section that contains the buildFile
        public void BuildFilesAdd(string targetGuid, PBXBuildFileData buildFile)
        {
            if (!m_FileGuidToBuildFileMap.ContainsKey(targetGuid))
                m_FileGuidToBuildFileMap[targetGuid] = new Dictionary<string, PBXBuildFileData>();
            m_FileGuidToBuildFileMap[targetGuid][buildFile.fileRef] = buildFile;
            buildFiles.AddEntry(buildFile);
        }

        public void BuildFilesRemove(string targetGuid, string fileGuid)
        {
            var buildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
            if (buildFile != null)
            {
                m_FileGuidToBuildFileMap[targetGuid].Remove(buildFile.fileRef);
                buildFiles.RemoveEntry(buildFile.guid);
            }
        }

        public PBXBuildFileData BuildFilesGetForSourceFile(string targetGuid, string fileGuid)
        {
            if (!m_FileGuidToBuildFileMap.ContainsKey(targetGuid))
                return null;
            if (!m_FileGuidToBuildFileMap[targetGuid].ContainsKey(fileGuid))
                return null;
            return m_FileGuidToBuildFileMap[targetGuid][fileGuid];
        }
        
        public IEnumerable<PBXBuildFileData> BuildFilesGetAll() 
        { 
            return buildFiles.GetObjects();
        }

        public void FileRefsAdd(string realPath, string projectPath, PBXGroupData parent, PBXFileReferenceData fileRef)
        {
            fileRefs.AddEntry(fileRef);
            m_ProjectPathToFileRefMap.Add(projectPath, fileRef);
            m_FileRefGuidToProjectPathMap.Add(fileRef.guid, projectPath);
            m_RealPathToFileRefMap[fileRef.tree].Add(realPath, fileRef); // FIXME
            m_GuidToParentGroupMap.Add(fileRef.guid, parent);
        }

        public PBXFileReferenceData FileRefsGet(string guid)
        {
            return fileRefs[guid];
        }

        public PBXFileReferenceData FileRefsGetByRealPath(string path, PBXSourceTree sourceTree)
        {
            if (m_RealPathToFileRefMap[sourceTree].ContainsKey(path))
                return m_RealPathToFileRefMap[sourceTree][path];
            return null;
        }

        public PBXFileReferenceData FileRefsGetByProjectPath(string path)
        {
            if (m_ProjectPathToFileRefMap.ContainsKey(path))
                return m_ProjectPathToFileRefMap[path];
            return null;
        }

        public void FileRefsRemove(string guid)
        {
            PBXFileReferenceData fileRef = fileRefs[guid];
            fileRefs.RemoveEntry(guid);
            m_ProjectPathToFileRefMap.Remove(m_FileRefGuidToProjectPathMap[guid]);
            m_FileRefGuidToProjectPathMap.Remove(guid);
            foreach (var tree in FileTypeUtils.AllAbsoluteSourceTrees())
                m_RealPathToFileRefMap[tree].Remove(fileRef.path);
            m_GuidToParentGroupMap.Remove(guid);
        }

        public PBXGroupData GroupsGet(string guid)
        {
            return groups[guid];
        }

        public PBXGroupData GroupsGetByChild(string childGuid)
        {
            return m_GuidToParentGroupMap[childGuid];
        }

        public PBXGroupData GroupsGetMainGroup()
        {
            return groups[project.project.mainGroup];
        }

        /// Returns the source group identified by sourceGroup. If sourceGroup is empty or null,
        /// root group is returned. If no group is found, null is returned.
        public PBXGroupData GroupsGetByProjectPath(string sourceGroup)
        {
            if (m_ProjectPathToGroupMap.ContainsKey(sourceGroup))
                return m_ProjectPathToGroupMap[sourceGroup];
            return null;
        }

        public void GroupsAdd(string projectPath, PBXGroupData parent, PBXGroupData gr)
        {
            m_ProjectPathToGroupMap.Add(projectPath, gr);
            m_GroupGuidToProjectPathMap.Add(gr.guid, projectPath);
            m_GuidToParentGroupMap.Add(gr.guid, parent);
            groups.AddEntry(gr);
        }

        public void GroupsAddDuplicate(PBXGroupData gr)
        {
            groups.AddEntry(gr);
        }

        public void GroupsRemove(string guid)
        {
            m_ProjectPathToGroupMap.Remove(m_GroupGuidToProjectPathMap[guid]);
            m_GroupGuidToProjectPathMap.Remove(guid);
            m_GuidToParentGroupMap.Remove(guid);
            groups.RemoveEntry(guid);
        }

        public FileGUIDListBase BuildSectionAny(PBXNativeTargetData target, string path, bool isFolderRef)
        {
            string ext = Path.GetExtension(path);
            var phase = FileTypeUtils.GetFileType(ext, isFolderRef);
            switch (phase) {
                case PBXFileType.Framework:
                    foreach (var guid in target.phases)
                        if (frameworks.HasEntry(guid))
                            return frameworks[guid];
                    break;
                case PBXFileType.Resource:
                    foreach (var guid in target.phases)
                        if (resources.HasEntry(guid))
                            return resources[guid];
                    break;
                case PBXFileType.Source:
                    foreach (var guid in target.phases)
                        if (sources.HasEntry(guid))
                            return sources[guid];
                    break;
                case PBXFileType.CopyFile:
                    foreach (var guid in target.phases)
                        if (copyFiles.HasEntry(guid))
                            return copyFiles[guid];
                    break;
            }
            return null;
        }

        void RefreshBuildFilesMapForBuildFileGuidList(Dictionary<string, PBXBuildFileData> mapForTarget,
                                                      FileGUIDListBase list)
        {
            foreach (string guid in list.files)
            {
                var buildFile = buildFiles[guid];
                mapForTarget[buildFile.fileRef] = buildFile;
            }
        }

        void RefreshMapsForGroupChildren(string projectPath, string realPath, PBXSourceTree realPathTree, PBXGroupData parent)
        {
            var children = new List<string>(parent.children);
            foreach (string guid in children)
            {
                PBXFileReferenceData fileRef = fileRefs[guid];
                string pPath;
                string rPath;
                PBXSourceTree rTree;

                if (fileRef != null)
                {
                    pPath = Utils.CombinePaths(projectPath, fileRef.name);
                    Utils.CombinePaths(realPath, realPathTree, fileRef.path, fileRef.tree, out rPath, out rTree);

                    if (!m_ProjectPathToFileRefMap.ContainsKey(pPath))
                    {
                        m_ProjectPathToFileRefMap.Add(pPath, fileRef);
                    }
                    if (!m_FileRefGuidToProjectPathMap.ContainsKey(fileRef.guid))
                    {
                        m_FileRefGuidToProjectPathMap.Add(fileRef.guid, pPath);
                    }
                    if (!m_RealPathToFileRefMap[rTree].ContainsKey(rPath))
                    {
                        m_RealPathToFileRefMap[rTree].Add(rPath, fileRef);
                    }
                    if (!m_GuidToParentGroupMap.ContainsKey(guid))
                    {
                        m_GuidToParentGroupMap.Add(guid, parent);
                    }

                    continue;
                }

                PBXGroupData gr = groups[guid];
                if (gr != null)
                {
                    pPath = Utils.CombinePaths(projectPath, gr.name);
                    Utils.CombinePaths(realPath, realPathTree, gr.path, gr.tree, out rPath, out rTree);

                    if (!m_ProjectPathToGroupMap.ContainsKey(pPath))
                    {
                        m_ProjectPathToGroupMap.Add(pPath, gr);
                    }
                    if (!m_GroupGuidToProjectPathMap.ContainsKey(gr.guid))
                    {
                        m_GroupGuidToProjectPathMap.Add(gr.guid, pPath);
                    }
                    if (!m_GuidToParentGroupMap.ContainsKey(guid))
                    {
                        m_GuidToParentGroupMap.Add(guid, parent);
                    }

                    RefreshMapsForGroupChildren(pPath, rPath, rTree, gr);
                }
            }
        }

        void RefreshAuxMaps()
        {
            foreach (var targetEntry in nativeTargets.GetEntries())
            {
                var map = new Dictionary<string, PBXBuildFileData>();
                foreach (string phaseGuid in targetEntry.Value.phases)
                {
                    if (frameworks.HasEntry(phaseGuid))
                        RefreshBuildFilesMapForBuildFileGuidList(map, frameworks[phaseGuid]);
                    if (resources.HasEntry(phaseGuid))
                        RefreshBuildFilesMapForBuildFileGuidList(map, resources[phaseGuid]);
                    if (sources.HasEntry(phaseGuid))
                        RefreshBuildFilesMapForBuildFileGuidList(map, sources[phaseGuid]);
                    if (copyFiles.HasEntry(phaseGuid))
                        RefreshBuildFilesMapForBuildFileGuidList(map, copyFiles[phaseGuid]);
                }
                m_FileGuidToBuildFileMap[targetEntry.Key] = map;
            }
            RefreshMapsForGroupChildren("", "", PBXSourceTree.Source, GroupsGetMainGroup());
        }

        public void Clear()
        {
            buildFiles = new PBXBuildFileSection("PBXBuildFile");
            fileRefs = new PBXFileReferenceSection("PBXFileReference");
            groups = new PBXGroupSection("PBXGroup");
            containerItems = new PBXContainerItemProxySection("PBXContainerItemProxy");
            references = new PBXReferenceProxySection("PBXReferenceProxy");
            sources = new PBXSourcesBuildPhaseSection("PBXSourcesBuildPhase");
            frameworks = new PBXFrameworksBuildPhaseSection("PBXFrameworksBuildPhase");
            resources = new PBXResourcesBuildPhaseSection("PBXResourcesBuildPhase");
            copyFiles = new PBXCopyFilesBuildPhaseSection("PBXCopyFilesBuildPhase");
            shellScripts = new PBXShellScriptBuildPhaseSection("PBXShellScriptBuildPhase");
            nativeTargets = new PBXNativeTargetSection("PBXNativeTarget");
            targetDependencies = new PBXTargetDependencySection("PBXTargetDependency");
            variantGroups = new PBXVariantGroupSection("PBXVariantGroup");
            buildConfigs = new XCBuildConfigurationSection("XCBuildConfiguration");
            configs = new XCConfigurationListSection("XCConfigurationList");
            project = new PBXProjectSection();
            m_UnknownSections = new Dictionary<string, UnknownSection>();

            m_Section = new Dictionary<string, SectionBase>
            {
                { "PBXBuildFile", buildFiles },
                { "PBXFileReference", fileRefs },
                { "PBXGroup", groups },
                { "PBXContainerItemProxy", containerItems },
                { "PBXReferenceProxy", references },
                { "PBXSourcesBuildPhase", sources },
                { "PBXFrameworksBuildPhase", frameworks },
                { "PBXResourcesBuildPhase", resources },
                { "PBXCopyFilesBuildPhase", copyFiles },
                { "PBXShellScriptBuildPhase", shellScripts },
                { "PBXNativeTarget", nativeTargets },
                { "PBXTargetDependency", targetDependencies },
                { "PBXVariantGroup", variantGroups },
                { "XCBuildConfiguration", buildConfigs },
                { "XCConfigurationList", configs },

                { "PBXProject", project },
            };
            m_RootElements = new PBXElementDict();
            m_UnknownObjects = new PBXElementDict();
            m_ObjectVersion = null;
            m_SectionOrder = new List<string>{
                "PBXBuildFile", "PBXContainerItemProxy", "PBXCopyFilesBuildPhase", "PBXFileReference",
                "PBXFrameworksBuildPhase", "PBXGroup", "PBXNativeTarget", "PBXProject", "PBXReferenceProxy",
                "PBXResourcesBuildPhase", "PBXShellScriptBuildPhase", "PBXSourcesBuildPhase", "PBXTargetDependency",
                "PBXVariantGroup", "XCBuildConfiguration", "XCConfigurationList"
            };
            m_FileGuidToBuildFileMap = new Dictionary<string, Dictionary<string, PBXBuildFileData>>();
            m_ProjectPathToFileRefMap = new Dictionary<string, PBXFileReferenceData>();
            m_FileRefGuidToProjectPathMap = new Dictionary<string, string>();
            m_RealPathToFileRefMap = new Dictionary<PBXSourceTree, Dictionary<string, PBXFileReferenceData>>();
            foreach (var tree in FileTypeUtils.AllAbsoluteSourceTrees())
                m_RealPathToFileRefMap.Add(tree, new Dictionary<string, PBXFileReferenceData>());
            m_ProjectPathToGroupMap = new Dictionary<string, PBXGroupData>();
            m_GroupGuidToProjectPathMap = new Dictionary<string, string>();
            m_GuidToParentGroupMap = new Dictionary<string, PBXGroupData>();
        }

        private void BuildCommentMapForBuildFiles(GUIDToCommentMap comments, List<string> guids, string sectName)
        {
            foreach (var guid in guids)
            {
                var buildFile = BuildFilesGet(guid);
                if (buildFile != null)
                {
                    var fileRef = FileRefsGet(buildFile.fileRef);
                    if (fileRef != null)
                        comments.Add(guid, String.Format("{0} in {1}", fileRef.name, sectName));
                    else
                    {
                        var reference = references[buildFile.fileRef];
                        if (reference != null)
                            comments.Add(guid, String.Format("{0} in {1}", reference.path, sectName));
                    }
                }
            }
        }

        private GUIDToCommentMap BuildCommentMap()
        {
            GUIDToCommentMap comments = new GUIDToCommentMap();

            // buildFiles are handled below
            // filerefs are handled below
            foreach (var e in groups.GetObjects())
                comments.Add(e.guid, e.name);
            foreach (var e in containerItems.GetObjects())
                comments.Add(e.guid, "PBXContainerItemProxy");
            foreach (var e in references.GetObjects())
                comments.Add(e.guid, e.path);
            foreach (var e in sources.GetObjects())
            {
                comments.Add(e.guid, "Sources");
                BuildCommentMapForBuildFiles(comments, e.files, "Sources");
            }
            foreach (var e in resources.GetObjects())
            {
                comments.Add(e.guid, "Resources");
                BuildCommentMapForBuildFiles(comments, e.files, "Resources");
            }
            foreach (var e in frameworks.GetObjects())
            {
                comments.Add(e.guid, "Frameworks");
                BuildCommentMapForBuildFiles(comments, e.files, "Frameworks");
            }
            foreach (var e in copyFiles.GetObjects())
            {
                string sectName = e.name;
                if (sectName == null)
                    sectName = "CopyFiles";
                comments.Add(e.guid, sectName);
                BuildCommentMapForBuildFiles(comments, e.files, sectName);
            }
            foreach (var e in shellScripts.GetObjects())
                comments.Add(e.guid, "ShellScript");
            foreach (var e in targetDependencies.GetObjects())
                comments.Add(e.guid, "PBXTargetDependency");
            foreach (var e in nativeTargets.GetObjects())
            {
                comments.Add(e.guid, e.name);
                comments.Add(e.buildConfigList, String.Format("Build configuration list for PBXNativeTarget \"{0}\"", e.name));
            }
            foreach (var e in variantGroups.GetObjects())
                comments.Add(e.guid, e.name);
            foreach (var e in buildConfigs.GetObjects())
                comments.Add(e.guid, e.name);
            foreach (var e in project.GetObjects())
            {
                comments.Add(e.guid, "Project object");
                comments.Add(e.buildConfigList, "Build configuration list for PBXProject \"Unity-iPhone\""); // FIXME: project name is hardcoded
            }
            foreach (var e in fileRefs.GetObjects())
                comments.Add(e.guid, e.name);
            if (m_RootElements.Contains("rootObject") && m_RootElements["rootObject"] is PBXElementString)
                comments.Add(m_RootElements["rootObject"].AsString(), "Project object");

            return comments;
        }

        private static PBXElementDict ParseContent(string content)
        {
            TokenList tokens = Lexer.Tokenize(content);
            var parser = new Parser(tokens);
            TreeAST ast = parser.ParseTree();
            return Serializer.ParseTreeAST(ast, tokens, content);
        }

        public void ReadFromStream(TextReader sr)
        {
            Clear();
            m_RootElements = ParseContent(sr.ReadToEnd());

            if (!m_RootElements.Contains("objects"))
                throw new Exception("Invalid PBX project file: no objects element");

            var objects = m_RootElements["objects"].AsDict();
            m_RootElements.Remove("objects");
            m_RootElements.SetString("objects", "OBJMARKER");

            if (m_RootElements.Contains("objectVersion"))
            {
                m_ObjectVersion = m_RootElements["objectVersion"].AsString();
                m_RootElements.Remove("objectVersion");
            }

            var allGuids = new List<string>();
            string prevSectionName = null;
            foreach (var kv in objects.values)
            {
                allGuids.Add(kv.Key);
                var el = kv.Value;

                if (!(el is PBXElementDict) || !el.AsDict().Contains("isa"))
                {
                    m_UnknownObjects.values.Add(kv.Key, el);
                    continue;
                }
                var dict = el.AsDict();
                var sectionName = dict["isa"].AsString();

                if (m_Section.ContainsKey(sectionName))
                {
                    var section = m_Section[sectionName];
                    section.AddObject(kv.Key, dict);
                }
                else
                {
                    UnknownSection section;
                    if (m_UnknownSections.ContainsKey(sectionName))
                        section = m_UnknownSections[sectionName];
                    else
                    {
                        section = new UnknownSection(sectionName);
                        m_UnknownSections.Add(sectionName, section);
                    }
                    section.AddObject(kv.Key, dict);

                    // update section order
                    if (!m_SectionOrder.Contains(sectionName))
                    {
                        int pos = 0;
                        if (prevSectionName != null)
                        {
                            // this never fails, because we already added any previous unknown sections
                            // to m_SectionOrder
                            pos = m_SectionOrder.FindIndex(x => x == prevSectionName);
                            pos += 1;
                        }
                        m_SectionOrder.Insert(pos, sectionName);
                    }
                }
                prevSectionName = sectionName;
            }
            RepairStructure(allGuids);
            RefreshAuxMaps();
        }


        public string WriteToString()
        {
            var commentMap = BuildCommentMap();
            var emptyChecker = new PropertyCommentChecker();
            var emptyCommentMap = new GUIDToCommentMap();

            // since we need to add custom comments, the serialization is much more complex
            StringBuilder objectsSb = new StringBuilder();
            if (m_ObjectVersion != null) // objectVersion comes right before objects
                objectsSb.AppendFormat("objectVersion = {0};\n\t", m_ObjectVersion);
            objectsSb.Append("objects = {");
            foreach (string sectionName in m_SectionOrder)
            {
                if (m_Section.ContainsKey(sectionName))
                    m_Section[sectionName].WriteSection(objectsSb, commentMap);
                else if (m_UnknownSections.ContainsKey(sectionName))
                    m_UnknownSections[sectionName].WriteSection(objectsSb, commentMap);
            }
            foreach (var kv in m_UnknownObjects.values)
                Serializer.WriteDictKeyValue(objectsSb, kv.Key, kv.Value, 2, false, emptyChecker, emptyCommentMap);
            objectsSb.Append("\n\t};");

            StringBuilder contentSb = new StringBuilder();
            contentSb.Append("// !$*UTF8*$!\n");
            Serializer.WriteDict(contentSb, m_RootElements, 0, false,
                                 new PropertyCommentChecker(new string[]{"rootObject/*"}), commentMap);
            contentSb.Append("\n");
            string content = contentSb.ToString();

            content = content.Replace("objects = OBJMARKER;", objectsSb.ToString());
            return content;
        }

        // This method walks the project structure and removes invalid entries.
        void RepairStructure(List<string> allGuids)
        {
            var guidSet = new Dictionary<string, bool>(); // emulate HashSet on .Net 2.0
            foreach (var guid in allGuids)
                guidSet.Add(guid, false);

            while (RepairStructureImpl(guidSet) == true)
                ;
        }

        /* Iterates the given guid list and removes all guids that are not in allGuids dictionary.
        */
        static void RemoveMissingGuidsFromGuidList(PBX.GUIDList guidList, Dictionary<string, bool> allGuids)
        {
            List<string> guidsToRemove = null;
            foreach (var guid in guidList)
            {
                if (!allGuids.ContainsKey(guid))
                {
                    if (guidsToRemove == null)
                        guidsToRemove = new List<string>();
                    guidsToRemove.Add(guid);
                }
            }
            if (guidsToRemove != null)
            {
                foreach (var guid in guidsToRemove)
                    guidList.RemoveGUID(guid);
            }
        }

        /*  Removes objects from the given @a section for which @a checker returns true.
            Also removes the guids of the removed elements from allGuids dictionary.
            Returns true if any objects were removed.
        */
        static bool RemoveObjectsFromSection<T>(KnownSectionBase<T> section,
                                                Dictionary<string, bool> allGuids,
                                                Func<T, bool> checker) where T : PBXObjectData, new()
        {
            List<string> guidsToRemove = null;
            foreach (var kv in section.GetEntries())
            {
                if (checker(kv.Value))
                {
                    if (guidsToRemove == null)
                        guidsToRemove = new List<string>();
                    guidsToRemove.Add(kv.Key);
                }
            }
            if (guidsToRemove != null)
            {
                foreach (var guid in guidsToRemove)
                {
                    section.RemoveEntry(guid);
                    allGuids.Remove(guid);
                }
                return true;
            }
            return false;
        }

        // Returns true if changes were done and one should call RepairStructureImpl again
        bool RepairStructureImpl(Dictionary<string, bool> allGuids)
        {
            bool changed = false;

            // PBXBuildFile
            changed |= RemoveObjectsFromSection(buildFiles, allGuids,
                                                o => (o.fileRef == null || !allGuids.ContainsKey(o.fileRef)));
            // PBXFileReference / fileRefs not cleaned

            // PBXGroup
            changed |= RemoveObjectsFromSection(groups, allGuids, o => o.children == null);
            foreach (var o in groups.GetObjects())
                RemoveMissingGuidsFromGuidList(o.children, allGuids);

            // PBXContainerItem / containerItems not cleaned
            // PBXReferenceProxy / references not cleaned

            // PBXSourcesBuildPhase
            changed |= RemoveObjectsFromSection(sources, allGuids, o => o.files == null);
            foreach (var o in sources.GetObjects())
                RemoveMissingGuidsFromGuidList(o.files, allGuids);
            // PBXFrameworksBuildPhase
            changed |= RemoveObjectsFromSection(frameworks, allGuids, o => o.files == null);
            foreach (var o in frameworks.GetObjects())
                RemoveMissingGuidsFromGuidList(o.files, allGuids);
            // PBXResourcesBuildPhase
            changed |= RemoveObjectsFromSection(resources, allGuids, o => o.files == null);
            foreach (var o in resources.GetObjects())
                RemoveMissingGuidsFromGuidList(o.files, allGuids);
            // PBXCopyFilesBuildPhase
            changed |= RemoveObjectsFromSection(copyFiles, allGuids, o => o.files == null);
            foreach (var o in copyFiles.GetObjects())
                RemoveMissingGuidsFromGuidList(o.files, allGuids);
            // PBXShellScriptsBuildPhase
            changed |= RemoveObjectsFromSection(shellScripts, allGuids, o => o.files == null);
            foreach (var o in shellScripts.GetObjects())
                RemoveMissingGuidsFromGuidList(o.files, allGuids);

            // PBXNativeTarget
            changed |= RemoveObjectsFromSection(nativeTargets, allGuids, o => o.phases == null);
            foreach (var o in nativeTargets.GetObjects())
                RemoveMissingGuidsFromGuidList(o.phases, allGuids);

            // PBXTargetDependency / targetDependencies not cleaned

            // PBXVariantGroup
            changed |= RemoveObjectsFromSection(variantGroups, allGuids, o => o.children == null);
            foreach (var o in variantGroups.GetObjects())
                RemoveMissingGuidsFromGuidList(o.children, allGuids);

            // XCBuildConfiguration / buildConfigs not cleaned

            // XCConfigurationList
            changed |= RemoveObjectsFromSection(configs, allGuids, o => o.buildConfigs == null);
            foreach (var o in configs.GetObjects())
                RemoveMissingGuidsFromGuidList(o.buildConfigs, allGuids);

            // PBXProject project not cleaned
            return changed;
        }
    }

} // namespace UnityEditor.iOS.Xcode


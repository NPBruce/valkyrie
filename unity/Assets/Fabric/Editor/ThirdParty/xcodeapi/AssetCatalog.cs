using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fabric.Internal.Editor.ThirdParty.xcodeapi
{
	internal class DeviceTypeRequirement
    {
        public static readonly string Key = "idiom";
        public static readonly string Any = "universal";
        public static readonly string iPhone = "iphone";
        public static readonly string iPad = "ipad";
        public static readonly string Mac = "mac";
        public static readonly string iWatch = "watch";
    }

	internal class MemoryRequirement
    {
        public static readonly string Key = "memory";
        public static readonly string Any = "";
        public static readonly string Mem1GB = "1GB";
        public static readonly string Mem2GB = "2GB";
    }

	internal class GraphicsRequirement
    {
        public static readonly string Key = "graphics-feature-set";
        public static readonly string Any = "";
        public static readonly string Metal1v2 = "metal1v2";
        public static readonly string Metal2v2 = "metal2v2";
    }

    // only used for image sets
    internal class SizeClassRequirement
    {
        public static readonly string HeightKey = "height-class";
        public static readonly string WidthKey = "width-class";
        public static readonly string Any = "";
        public static readonly string Compact = "compact";
        public static readonly string Regular = "regular";
    }

    // only used for image sets
    internal class ScaleRequirement
    {
        public static readonly string Key = "scale";
        public static readonly string Any = ""; // vector image
        public static readonly string X1 = "1x";
        public static readonly string X2 = "2x";
        public static readonly string X3 = "3x";
    }

    internal class DeviceRequirement
    {
        internal Dictionary<string, string> values = new Dictionary<string, string>();

        public DeviceRequirement AddDevice(string device)
        {
			AddCustom(DeviceTypeRequirement.Key, device);
            return this;
        }

        public DeviceRequirement AddMemory(string memory)
        {
			AddCustom(MemoryRequirement.Key, memory);
            return this;
        }

        public DeviceRequirement AddGraphics(string graphics)
        {
			AddCustom(GraphicsRequirement.Key, graphics);
            return this;
        }

        public DeviceRequirement AddWidthClass(string sizeClass)
        {
            AddCustom(SizeClassRequirement.WidthKey, sizeClass);
            return this;
        }

        public DeviceRequirement AddHeightClass(string sizeClass)
        {
            AddCustom(SizeClassRequirement.HeightKey, sizeClass);
            return this;
        }

        public DeviceRequirement AddScale(string scale)
        {
            AddCustom(ScaleRequirement.Key, scale);
            return this;
        }

        public DeviceRequirement AddCustom(string key, string value)
        {
            if (values.ContainsKey(key))
                values.Remove(key);
            values.Add(key, value);
            return this;
        }

        public DeviceRequirement()
        {
			values.Add("idiom", DeviceTypeRequirement.Any);
        }
    }

    internal class AssetCatalog
    {
        AssetFolder m_Root;

        public string path { get { return m_Root.path; } }
        public AssetFolder root { get { return m_Root; } }

        public AssetCatalog(string path, string authorId)
        {
            if (Path.GetExtension(path) != ".xcassets")
                throw new Exception("Asset catalogs must have xcassets extension");
            m_Root = new AssetFolder(path, null, authorId);
        }

        AssetFolder OpenFolderForResource(string relativePath)
        {
            var pathItems = PBX.Utils.SplitPath(relativePath).ToList();

            // remove path filename
            pathItems.RemoveAt(pathItems.Count - 1);

            AssetFolder folder = root;
            foreach (var pathItem in pathItems)
                folder = folder.OpenFolder(pathItem);
            return folder;
        }

        // Checks if a dataset at the given path exists and returns it if it does.
        // Otherwise, creates a new dataset. Parent folders are created if needed.
        // Note: the path is filesystem path, not logical asset name formed
        // only from names of the folders that have "provides namespace" attribute.
        // If you want to put certain resources in folders with namespace, first
        // manually create the folders and then set the providesNamespace attribute.
        // OpenNamespacedFolder may help to do this.
        public AssetDataSet OpenDataSet(string relativePath)
        {
            var folder = OpenFolderForResource(relativePath);
            return folder.OpenDataSet(Path.GetFileName(relativePath));
        }

        public AssetImageSet OpenImageSet(string relativePath)
        {
            var folder = OpenFolderForResource(relativePath);
            return folder.OpenImageSet(Path.GetFileName(relativePath));
        }
        
        public AssetImageStack OpenImageStack(string relativePath)
        {
            var folder = OpenFolderForResource(relativePath);
            return folder.OpenImageStack(Path.GetFileName(relativePath));
        }

        // Checks if a folder with given path exists and returns it if it does.
        // Otherwise, creates a new folder. Parent folders are created if needed.
        public AssetFolder OpenFolder(string relativePath)
        {
            if (relativePath == null)
                return root;
            var pathItems = PBX.Utils.SplitPath(relativePath);
            if (pathItems.Length == 0)
                return root;
            AssetFolder folder = root;
            foreach (var pathItem in pathItems)
                folder = folder.OpenFolder(pathItem);
            return folder;
        }

        // Creates a directory structure with "provides namespace" attribute.
        // First, retrieves or creates the directory at relativeBasePath, creating parent
        // directories if needed. Effectively calls OpenFolder(relativeBasePath).
        // Then, relative to this directory, creates namespacePath directories with "provides
        // namespace" attribute set. Fails if the attribute can't be set.
        public AssetFolder OpenNamespacedFolder(string relativeBasePath, string namespacePath)
        {
            var folder = OpenFolder(relativeBasePath);
            var pathItems = PBX.Utils.SplitPath(namespacePath);
            foreach (var pathItem in pathItems)
            {
                folder = folder.OpenFolder(pathItem);
                folder.providesNamespace = true;
            }
            return folder;
        }

        public void Write()
        {
            m_Root.Write();
        }
    }

    internal abstract class AssetCatalogItem
    {
        public readonly string name;
        public readonly string authorId;
        public string path { get { return m_Path; } }

        protected Dictionary<string, string> m_Properties = new Dictionary<string, string>();

        protected string m_Path;

        public AssetCatalogItem(string name, string authorId)
        {
            if (name != null && name.Contains("/"))
                throw new Exception("Asset catalog item must not have slashes in name");
            this.name = name;
            this.authorId = authorId;
        }

        protected JsonElementDict WriteInfoToJson(JsonDocument doc)
        {
            var info = doc.root.CreateDict("info");
            info.SetInteger("version", 1);
            info.SetString("author", authorId);
            return info;
        }

        public abstract void Write();
    }

    internal class AssetFolder : AssetCatalogItem
    {
        List<AssetCatalogItem> m_Items = new List<AssetCatalogItem>();
        bool m_ProvidesNamespace = false;

        public bool providesNamespace
        {
            get { return m_ProvidesNamespace; }
            set {
                if (m_Items.Count > 0 && value != m_ProvidesNamespace)
                    throw new Exception("Asset folder namespace providing status can't be "+
                                        "changed after items have been added");
                m_ProvidesNamespace = value;
            }
        }

        internal AssetFolder(string parentPath, string name, string authorId) : base(name, authorId)
        {
            if (name != null)
                m_Path = Path.Combine(parentPath, name);
            else
                m_Path = parentPath;
        }

        // Checks if a folder with given name exists and returns it if it does.
        // Otherwise, creates a new folder.
        public AssetFolder OpenFolder(string name)
        {
            var item = GetChild(name);
            if (item != null)
            {
                if (item is AssetFolder)
                    return item as AssetFolder;
                throw new Exception("The given path is already occupied with an asset");
            }

            var folder = new AssetFolder(m_Path, name, authorId);
            m_Items.Add(folder);
            return folder;
        }

        T GetExistingItemWithType<T>(string name) where T : class
        {
            var item = GetChild(name);
            if (item != null)
            {
                if (item is T)
                    return item as T;
                throw new Exception("The given path is already occupied with an asset");
            }
            return null;
        }

        // Checks if a dataset with given name exists and returns it if it does.
        // Otherwise, creates a new data set.
        public AssetDataSet OpenDataSet(string name)
        {
            var item = GetExistingItemWithType<AssetDataSet>(name);
            if (item != null)
                return item;

            var dataset = new AssetDataSet(m_Path, name, authorId);
            m_Items.Add(dataset);
            return dataset;
        }

        // Checks if an imageset with given name exists and returns it if it does.
        // Otherwise, creates a new image set.
        public AssetImageSet OpenImageSet(string name)
        {
            var item = GetExistingItemWithType<AssetImageSet>(name);
            if (item != null)
                return item;

            var imageset = new AssetImageSet(m_Path, name, authorId);
            m_Items.Add(imageset);
            return imageset;
        }
        
        // Checks if a image stack with given name exists and returns it if it does.
        // Otherwise, creates a new image stack.
        public AssetImageStack OpenImageStack(string name)
        {
            var item = GetExistingItemWithType<AssetImageStack>(name);
            if (item != null)
                return item;
            
            var imageStack = new AssetImageStack(m_Path, name, authorId);
            m_Items.Add(imageStack);
            return imageStack;
        }

        // Returns the requested item or null if not found
        public AssetCatalogItem GetChild(string name)
        {
            foreach (var item in m_Items)
            {
                if (item.name == name)
                    return item;
            }
            return null;
        }

        void WriteJson()
        {
            if (!providesNamespace)
                return; // json is optional when namespace is not provided

            var doc = new JsonDocument();

            WriteInfoToJson(doc);

            var props = doc.root.CreateDict("properties");
            props.SetBoolean("provides-namespace", providesNamespace);
            doc.WriteToFile(Path.Combine(m_Path, "Contents.json"));
        }

        public override void Write()
        {
            if (Directory.Exists(m_Path))
                Directory.Delete(m_Path, true); // ensure we start from clean state
            Directory.CreateDirectory(m_Path);
            WriteJson();

            foreach (var item in m_Items)
                item.Write();
        }
    }

    abstract class AssetCatalogItemWithVariants : AssetCatalogItem
    {
		protected List<VariantData> m_Variants = new List<VariantData>();
        protected List<string> m_ODRTags = new List<string>();

        protected AssetCatalogItemWithVariants(string name, string authorId) :
            base(name, authorId)
        {
        }

        protected class VariantData
        {
			public DeviceRequirement requirement;
            public string path;

            public VariantData(DeviceRequirement requirement, string path)
            {
				this.requirement = requirement;
                this.path = path;
            }
        }

        public bool HasVariant(DeviceRequirement requirement)
        {
            foreach (var item in m_Variants)
            {
                if (item.requirement.values == requirement.values)
                    return true;
            }
            return false;
        }

        public void AddOnDemandResourceTag(string tag)
        {
            if (!m_ODRTags.Contains(tag))
                m_ODRTags.Add(tag);
        }

        protected void AddVariant(VariantData newItem)
        {
            foreach (var item in m_Variants)
            {
                if (item.requirement.values == newItem.requirement.values)
                    throw new Exception("The given requirement has been already added");
                if (Path.GetFileName(item.path) == Path.GetFileName(path))
                    throw new Exception("Two items within the same set must not have the same file name");
            }
            if (Path.GetFileName(newItem.path) == "Contents.json")
                throw new Exception("The file name must not be equal to Contents.json");
            m_Variants.Add(newItem);
        }

        protected void WriteODRTagsToJson(JsonElementDict info)
        {
            if (m_ODRTags.Count > 0)
            {
                var tags = info.CreateArray("on-demand-resource-tags");
                foreach (var tag in m_ODRTags)
                    tags.AddString(tag);
            }
        }

        protected void WriteRequirementsToJson(JsonElementDict item, DeviceRequirement req)
        {
            foreach (var kv in req.values)
            {
                if (kv.Value != null && kv.Value != "")
                    item.SetString(kv.Key, kv.Value);
            }
        }
    }

    internal class AssetDataSet : AssetCatalogItemWithVariants
    {
        class DataSetVariant : VariantData
        {
            public string id;

            public DataSetVariant(DeviceRequirement requirement, string path, string id) : base(requirement, path)
            {
                this.id = id;
            }
        }

        internal AssetDataSet(string parentPath, string name, string authorId) : base(name, authorId)
        {
            m_Path = Path.Combine(parentPath, name + ".dataset");
        }

		// an exception is thrown is two equivalent requirements are added.
        // The same asset dataset must not have paths with equivalent filenames.
        // The identifier allows to identify which data variant is actually loaded (use
        // the typeIdentifer property of the NSDataAsset that was created from the data set)
        public void AddVariant(DeviceRequirement requirement, string path, string typeIdentifier)
        {
            foreach (DataSetVariant item in m_Variants)
            {
                if (item.id != null && typeIdentifier != null && item.id == typeIdentifier)
                    throw new Exception("Two items within the same dataset must not have the same id");
            }
            AddVariant(new DataSetVariant(requirement, path, typeIdentifier));
        }

        public override void Write()
        {
            Directory.CreateDirectory(m_Path);

            var doc = new JsonDocument();

            var info = WriteInfoToJson(doc);
            WriteODRTagsToJson(info);

            var data = doc.root.CreateArray("data");

            foreach (DataSetVariant item in m_Variants)
            {
                var filename = Path.GetFileName(item.path);
                File.Copy(item.path, Path.Combine(m_Path, filename));

                var docItem = data.AddDict();
                docItem.SetString("filename", filename);
                WriteRequirementsToJson(docItem, item.requirement);
                if (item.id != null)
                    docItem.SetString("universal-type-identifier", item.id);
            }
            doc.WriteToFile(Path.Combine(m_Path, "Contents.json"));
        }
    }

    internal class ImageAlignment
    {
        public int left = 0, right = 0, top = 0, bottom = 0;
    }

    internal class ImageResizing
    {
        public enum SlicingType
        {
            Horizontal,
            Vertical,
            HorizontalAndVertical
        }

        public enum ResizeMode
        {
            Stretch,
            Tile
        }

        public SlicingType type = SlicingType.HorizontalAndVertical;
        public int left = 0;                // only valid for horizontal slicing
        public int right = 0;               // only valid for horizontal slicing
        public int top = 0;                 // only valid for vertical slicing
        public int bottom = 0;              // only valid for vertical slicing
        public ResizeMode centerResizeMode = ResizeMode.Stretch;
        public int centerWidth = 0;         // only valid for vertical slicing
        public int centerHeight = 0;        // only valid for horizontal slicing
    }

    // TODO: rendering intent property
    internal class AssetImageSet : AssetCatalogItemWithVariants
    {
        internal AssetImageSet(string assetCatalogPath, string name, string authorId) : base(name, authorId)
        {
            m_Path = Path.Combine(assetCatalogPath, name + ".imageset");
        }

        class ImageSetVariant : VariantData
        {
            public ImageAlignment alignment = null;
            public ImageResizing resizing = null;

            public ImageSetVariant(DeviceRequirement requirement, string path) : base(requirement, path)
            {
            }
        }

        public void AddVariant(DeviceRequirement requirement, string path)
        {
            AddVariant(new ImageSetVariant(requirement, path));
        }

        public void AddVariant(DeviceRequirement requirement, string path, ImageAlignment alignment, ImageResizing resizing)
        {
            var imageset = new ImageSetVariant(requirement, path);
            imageset.alignment = alignment;
            imageset.resizing = resizing;
            AddVariant(imageset);
        }

        void WriteAlignmentToJson(JsonElementDict item, ImageAlignment alignment)
        {
            var docAlignment = item.CreateDict("alignment-insets");
            docAlignment.SetInteger("top", alignment.top);
            docAlignment.SetInteger("bottom", alignment.bottom);
            docAlignment.SetInteger("left", alignment.left);
            docAlignment.SetInteger("right", alignment.right);
        }

        static string GetSlicingMode(ImageResizing.SlicingType mode)
        {
            switch (mode)
            {
                case ImageResizing.SlicingType.Horizontal: return "3-part-horizontal";
                case ImageResizing.SlicingType.Vertical: return "3-part-vertical";
                case ImageResizing.SlicingType.HorizontalAndVertical: return "9-part";
            }
            return "";
        }

        static string GetCenterResizeMode(ImageResizing.ResizeMode mode)
        {
            switch (mode)
            {
                case ImageResizing.ResizeMode.Stretch: return "stretch";
                case ImageResizing.ResizeMode.Tile: return "tile";
            }
            return "";
        }

        void WriteResizingToJson(JsonElementDict item, ImageResizing resizing)
        {
            var docResizing = item.CreateDict("resizing");
            docResizing.SetString("mode", GetSlicingMode(resizing.type));

            var docCenter = docResizing.CreateDict("center");
            docCenter.SetString("mode", GetCenterResizeMode(resizing.centerResizeMode));
            docCenter.SetInteger("width", resizing.centerWidth);
            docCenter.SetInteger("height", resizing.centerHeight);

            var docInsets = docResizing.CreateDict("cap-insets");
            docInsets.SetInteger("top", resizing.top);
            docInsets.SetInteger("bottom", resizing.bottom);
            docInsets.SetInteger("left", resizing.left);
            docInsets.SetInteger("right", resizing.right);
        }

        public override void Write()
        {
            Directory.CreateDirectory(m_Path);
            var doc = new JsonDocument();
            var info = WriteInfoToJson(doc);
            WriteODRTagsToJson(info);

            var images = doc.root.CreateArray("images");

            foreach (ImageSetVariant item in m_Variants)
            {
                var filename = Path.GetFileName(item.path);
                File.Copy(item.path, Path.Combine(m_Path, filename));

                var docItem = images.AddDict();
                docItem.SetString("filename", filename);
                WriteRequirementsToJson(docItem, item.requirement);
                if (item.alignment != null)
                    WriteAlignmentToJson(docItem, item.alignment);
                if (item.resizing != null)
                    WriteResizingToJson(docItem, item.resizing);
            }
            doc.WriteToFile(Path.Combine(m_Path, "Contents.json"));
        }
    }

    /*  A stack layer may either contain an image set or reference another imageset
    */
    class AssetImageStackLayer : AssetCatalogItem
    {
        internal AssetImageStackLayer(string assetCatalogPath, string name, string authorId) : base(name, authorId)
        {
            m_Path = Path.Combine(assetCatalogPath, name + ".imagestacklayer");
            m_Imageset = new AssetImageSet(m_Path, "Content", authorId);
        }

        AssetImageSet m_Imageset = null;
        string m_ReferencedName = null;

        public void SetReference(string name)
        {
            m_Imageset = null;
            m_ReferencedName = name;
        }

        public string ReferencedName()
        {
            return m_ReferencedName;
        }

        public AssetImageSet GetImageSet()
        {
            return m_Imageset;
        }

        public override void Write()
        {
            Directory.CreateDirectory(m_Path);
            var doc = new JsonDocument();
            WriteInfoToJson(doc);

            if (m_ReferencedName != null)
            {
                var props = doc.root.CreateDict("properties");
                var reference = props.CreateDict("content-reference");
                reference.SetString("type", "image-set");
                reference.SetString("name", m_ReferencedName);
                reference.SetString("matching-style", "fully-qualified-name");
            }
            if (m_Imageset != null)
                m_Imageset.Write();

            doc.WriteToFile(Path.Combine(m_Path, "Contents.json"));
        }
    }

    class AssetImageStack : AssetCatalogItem
    {
        List<AssetImageStackLayer> m_Layers = new List<AssetImageStackLayer>();

        internal AssetImageStack(string assetCatalogPath, string name, string authorId) : base(name, authorId)
        {
            m_Path = Path.Combine(assetCatalogPath, name + ".imagestack");
        }

        public AssetImageStackLayer AddLayer(string name)
        {
            foreach (var layer in m_Layers)
            {
                if (layer.name == name)
                    throw new Exception("A layer with given name already exists");
            }
            var newLayer = new AssetImageStackLayer(m_Path, name, authorId);
            m_Layers.Add(newLayer);
            return newLayer;
        }

        public override void Write()
        {
            Directory.CreateDirectory(m_Path);
            var doc = new JsonDocument();
            WriteInfoToJson(doc);

            var docLayers = doc.root.CreateArray("layers");
            foreach (var layer in m_Layers)
            {
                layer.Write();
 
                var docLayer = docLayers.AddDict();
                docLayer.SetString("filename", Path.GetFileName(layer.path));
            }
            doc.WriteToFile(Path.Combine(m_Path, "Contents.json"));
        }
    }

} // namespace UnityEditor.iOS.Xcode

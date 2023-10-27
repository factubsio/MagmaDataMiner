using BubbleAssets;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using ShinyShoe;
using ShinyShoe.Ares;
using ShinyShoe.EcsEventSystem;
using ShinyShoe.SharedDataLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WikiGen;
using WikiGen.Assets;

namespace MagmaDataMiner
{

    public class Miner
    {
        // Token: 0x0400283D RID: 10301
        private readonly BinaryReader reader;

        public Miner(Stream stream)
        {
            stream.Position = 0L;
            reader = new BinaryReader(stream, new UTF8Encoding());
            var version = reader.ReadInt32();
            var yamlCrc = reader.ReadUInt32();
        }

        public SoBinarySerializerBase.Token ReadToken()
        {
            return (SoBinarySerializerBase.Token)this.reader.ReadByte();
        }

        private int level = 0;

        public MinedField Mine(Type hint)
        {
            var token = ReadToken();

            if (token == SoBinarySerializerBase.Token.ExpectedTypeHint)
            {
                hint = this.DeserializeExpectedTypeHint();
				token = this.ReadToken();
            }

            object value;

            switch (token)
            {
                case SoBinarySerializerBase.Token.Primitive:
                    value = this.DeserializePrimitive();
                    break;

                case SoBinarySerializerBase.Token.Enum:
                    value = this.DeserializeEnum(hint);
                    break;

                case SoBinarySerializerBase.Token.Object:
                    value = this.DeserializeObject(hint);
                    break;

                case SoBinarySerializerBase.Token.Array:
                    value = this.DeserializeList(hint.GetElementType()!);
                    break;

                case SoBinarySerializerBase.Token.List:
                    value = this.DeserializeList(hint.GetGenericArguments()[0]);
                    break;

                case SoBinarySerializerBase.Token.Null:
                    value = MinedField.Null;
                    break;

                case SoBinarySerializerBase.Token.Guid:
                    throw new NotImplementedException("guid");
                    //value = this.DeserializeGuid(expectedType);

                default:
                    throw new Exception();
            }

            return new(value);
        }

        private Type DeserializeExpectedTypeHint()
        {
            string guid = this.reader.ReadString();
			long fileID = this.reader.ReadInt64();
			return MineDb.GetClassTypeByGuid(guid, fileID);
        }


        private object DeserializeEnum(Type hint)
        {
            SoBinarySerializerBase.Token token = this.ReadToken();
            object value = this.DeserializePrimitive();
            return Enum.ToObject(hint, value);
        }


        private object DeserializeList(Type elemHint)
        {
            int num = reader.ReadInt32();

            MinedField[] arr = new MinedField[num];
            for (int i = 0; i < num; i++)
            {
                arr[i] = Mine(elemHint);
            }
            End();

            return arr;
        }

        public static ClassFieldCache _fieldCache = new();

        private object DeserializeObject(Type hint)
        {
            int num = reader.ReadInt32();
            var obj = new MinedAsset();

            level++;

            for (int i = 0; i < num; i++)
            {
                string fieldName = reader.ReadString();
                long pos = reader.BaseStream.Position;
                ushort len = this.reader.ReadUInt16();

                FieldInfo fieldInfo = _fieldCache.GetFieldInfo(hint, fieldName)!;


                if (fieldInfo == null)
                {
                    reader.BaseStream.Position = pos + len;
                    obj.fields.Add(fieldName, new("<unknown>"));
                    if (len == 0)
                    {
                        throw new Exception("length mismatch");
                    }
                }
                else
                {
                    var value = Mine(fieldInfo.FieldType);
                    obj.fields.Add(fieldName, value);
                }


                //this.reader.BaseStream.Position = pos + (long)((ulong)len);
            }

            End();

            level--;

            return obj;
        }

        private void End()
        {
            if (ReadToken() != SoBinarySerializerBase.Token.End)
            {
                throw new NotSupportedException();
            }
        }

        private object DeserializePrimitive()
        {
            TypeCode typeCode = (TypeCode)this.reader.ReadByte();
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return this.reader.ReadBoolean();

                case TypeCode.Char:
                    return this.reader.ReadChar();

                case TypeCode.SByte:
                    return this.reader.ReadSByte();

                case TypeCode.Byte:
                    return this.reader.ReadByte();

                case TypeCode.Int16:
                    return this.reader.ReadInt16();

                case TypeCode.UInt16:
                    return this.reader.ReadUInt16();

                case TypeCode.Int32:
                    return this.reader.ReadInt32();

                case TypeCode.UInt32:
                    return this.reader.ReadUInt32();

                case TypeCode.Int64:
                    return this.reader.ReadInt64();

                case TypeCode.UInt64:
                    return this.reader.ReadUInt64();

                case TypeCode.Single:
                    return this.reader.ReadSingle();

                case TypeCode.Double:
                    return this.reader.ReadDouble();

                case TypeCode.Decimal:
                    return this.reader.ReadDecimal();

                case TypeCode.String:
                    return this.reader.ReadString();
            }
            throw new Exception();
        }

        //// Token: 0x0400283E RID: 10302
        //private UnityAsset assetDeserialized;

        //// Token: 0x0400283F RID: 10303
        //private ClassFieldCache fieldCache;

        //// Token: 0x04002840 RID: 10304
        //private string parentAssetGuid;

        //// Token: 0x04002841 RID: 10305
        //private string parentAssetDebugName;

        //// Token: 0x04002842 RID: 10306
        //private Queue<UnityAsset> assetsToLoad;

        //// Token: 0x04002843 RID: 10307
        //private AssetLibrary assetLibrary;

        //// Token: 0x04002844 RID: 10308
        //private int numObjectsSoFar;
    }
    public record struct MinedField(object Value)
    {
        public readonly static object Null = new();

        public bool IsAsset => Value is MinedAsset;
        public bool IsArray => Value.GetType().IsArray;

        public bool IsFlat => !IsAsset && !IsArray;
        public bool IsNull => ReferenceEqualityComparer.ReferenceEquals(Value, Null);

        public bool IsEmptyLocString => String == "EmptyString-00000000-00000000000000000000000000000000";

        public MinedAsset Asset => (Value as MinedAsset)!;

        public string String => (Value as string)!;

        public string Localized()
        {
            if (IsNull)
            {
                return "";
            }

            var raw = (MineDb.Translate(String).Clone() as string)!;


            Loccer.ApplyLocalizationParams(ref raw, param =>
            {
                if (Loccer.TryGetLocalizationParameterActionParamAsInt(param, out var value))
                    return value.ToString();
                else if (Loccer.TryGetLocalizationParameterStatusEffectProcChance(param, out var value2))
                    return value2.ToString();
                else if (param == "oa")
                    return "<span class=\"text-added\">";
                else if (param == "ca")
                    return "</span>";
                else if (Loccer.TryGetLocalizationParameterActionDamageParam(param, out var damage))
                    return damage.ToString();
                else
                    return param;

            });
            return raw;
        }

        public int Length => Array.Length;
        public MinedField At(int index) => Array[index];

        public class VisitedElement
        {
            public string key { get; set; } = "";
            public string value { get; set; } = "";
            public int Count { get; set; }
            public int levelDelta { get; set; }
            public bool isObj { get; set; }
            public string link { get; set; } = "";
            //public string linkTarget;
            public bool Empty { get; set; }
            public bool Last { get; set; }
        }

        public static IEnumerable<VisitedElement> Visit(MinedField node, string name)
        {
            if (node.IsNull)
            {
                yield return new() { key = name, value = "null" };
            }
            if (node.IsFlat)
            {
                yield return new() { key = name, value = node.Value.ToString()! };
            }
            else if (node.IsArray)
            {
                yield return new VisitedElement { key = name, levelDelta = 1, Count = node.Length };
                for (int index = 0; index < node.Length; index++)
                {
                    foreach (var n in Visit(node.At(index), index.ToString()))
                        yield return n;
                }
                yield return new VisitedElement { levelDelta = -1, Count = node.Length };
            }
            else
            {
                yield return new VisitedElement { key = name, levelDelta = 1, isObj = true, Count = node.Asset.fields.Count };
                if (node.Asset.IsRoot)
                {
                    yield return new() { key = "DataId", value = node.Asset.DataId };
                }
                foreach (var elem in node.Asset.fields)
                {
                    foreach (var n in Visit(elem.Value, elem.Key))
                        yield return n;
                }
                yield return new VisitedElement { levelDelta = -1, isObj = true, Count = node.Asset.fields.Count };
            }
        }

        public IEnumerable<MinedField> Enumerate()
        {
            return Array;
        }

        private readonly MinedField[] Array => (Value as MinedField[])!;

        public int Int => (int)Value;
        public float Float => (float)Value;

        public bool Bool => (bool)Value;

        public IEnumerable<MinedAsset> EnumerateAssetLinks()
        {
            return Enumerate().Select(s => s.Deref());
        }

        public MinedAsset Deref() => MineDb.Get(new((long)this["fileID"].Value, this["guid"].String));
        public MinedAsset Deref(string v) => Asset.Deref(v);

        public bool Has(string v) => Asset.fields.ContainsKey(v);

        public string Translated()
        {
            return MineDb.Translate(String).Sanitized();
        }

        public MinedField this[string index]
        {
            get
            {
                if (!IsAsset)
                    throw new NotSupportedException();
                return Asset.fields[index];
            }
        }
        public MinedField this[int index]
        {
            get
            {
                if (!IsArray)
                    throw new NotSupportedException();
                return At(index);
            }
        }
    }

    public class MinedAsset
    {
        public Dictionary<string, MinedField> fields = new();

        public IEnumerable<MinedField.VisitedElement> Iterate
        {
            get
            {
                return MinedField.Visit(new(this), "root");
            }
        }
        private static void Indent(int level, TextWriter output)
        {
            for (int i = 0; i < level; i++)
            {
                output.Write("  ");
            }
        }

        public string RawJson
        {
            get
            {
                StringWriter raw = new();

                int level = 0;

                Stack<bool> isObject = new();

                foreach (var x in Iterate)
                {
                    if (x.levelDelta > 0)
                    {
                        Indent(level, raw);
                        char ch = x.isObj ? '{' : '[';
                        string end = "";
                        if (x.Count == 0)
                            end = x.isObj ? "}," : "],";

                        if (level > 0)
                        {
                            if (isObject.Peek())
                                raw.WriteLine($"\"{x.key}\": {ch}{end}");
                            else
                                raw.WriteLine($"{ch}{end}");
                        }
                        else
                        {
                            raw.WriteLine("{");
                        }

                        isObject.Push(x.isObj);
                        level++;
                    }
                    else if (x.levelDelta < 0)
                    {
                        level--;
                        isObject.Pop();
                        if (x.Count > 0)
                        {
                            Indent(level, raw);
                            if (level == 0)
                                raw.WriteLine("}");
                            else
                                raw.WriteLine($"{(x.isObj ? '}' : ']')},");
                        }
                    }
                    else
                    {
                        Indent(level, raw);
                        if (isObject.Peek())
                            raw.WriteLine($"\"{x.key}\": \"{Escape(x.value)}\",");
                        else
                            raw.WriteLine($"\"{Escape(x.value)}\",");
                    }
                }

                return raw.ToString();
            }
        }

        private string Escape(string value)
        {
            return value.Replace("\n", "\\n");
        }

        public MinedField this[string index] => fields[index];

        public MinedAsset Deref(string v)
        {
            return this[v].Deref();
        }

        public IEnumerable<MinedAsset> EnumerateAssetLinks(string v)
        {
            return this[v].EnumerateAssetLinks();
        }

        public bool IsRoot = false;
        public string AssetName = "";
        public string DataId = "";
        public string AssetGuid = "";
        public long AssetFile;
        public string TypeName = "";
    }

    public class LocKeyTable
    {
        public string TableName = "";

        public void Add(string k, long v) => keyToId.Add(k, v);
        public bool TryGet(string k, out long id) => keyToId.TryGetValue(k, out id);

        private readonly Dictionary<string, long> keyToId = new();
    }
    public class LocValTable
    {
        public string TableName = "";

        public void Add(long k, string v) => idToStr.Add(k, v);
        public string? this[long k] {
            get
            {
                if (!idToStr.TryGetValue(k, out var val))
                    return null;
                return val;
            }
        }

        private readonly Dictionary<long, string> idToStr = new();
    }

    public class LocValSet
    {
        public readonly Dictionary<string, LocValTable> Tables = new();

        public string? Lookup(string table, long id) => Tables[table][id];
    }

    public class LocKeySet
    {
        public readonly List<LocKeyTable> Tables = new();

        public (string, long) Lookup(string key)
        {
            foreach (var table in Tables)
            {
                if (table.TryGet(key, out var id))
                {
                    return (table.TableName, id);
                }
            }

            throw new KeyNotFoundException(key);
        }
    }

    public class MineDb
    {

        public const string draftableAssetName = "ClassesAndLoot/AbilityDrafts/DraftLists/NonClassAbilities_AbilityList";
        public static IEnumerable<MinedAsset> CharacterClasses => AssetsByType("CharacterClassData");
        public static IEnumerable<MinedAsset> DraftableBindings => Lookup(draftableAssetName).EnumerateAssetLinks("abilities");

        public static readonly Dictionary<string, Sprite> IconsByName = new();

        public static string? Base64Icon(string? iconName)
        {
            if (iconName == null)
                return null;

            string? base64Icon = null;

            if (IconsByName.TryGetValue(iconName, out var iconSprite))
            {
                if (BlueprintAssetsContext.TryRenderSprite(iconSprite, out var icon))
                {
                    base64Icon = BlueprintAssetsContext.ImageToBase64(icon);
                }
            }

            return base64Icon;
        }

        private static readonly Dictionary<AssetID, MinedAsset> db = new();
        private static readonly Dictionary<string, MinedAsset> byData = new();
        private static readonly Dictionary<string, MinedAsset> byName = new();
        private static FlatBufferAssetLoader? loader;
        public static Assembly? asm;
        private static StreamWriter? bobwr;
        private static readonly Dictionary<string, AssetLibraryManifest.Entry> nameToEntry = new();
        private static readonly Dictionary<AssetID, AssetLibraryManifest.Entry> guidAndFileIDToEntry = new();
        private static readonly Dictionary<string, AssetLibraryManifest.Entry> dataIdToEntry = new();
        private static readonly Dictionary<string, List<AssetLibraryManifest.Entry>> baseDataTypeToEntries = new();

        private const long DefaultObjectFileID = 11400000L;

        public static readonly LocKeySet LocKeys = new();

        public static readonly Dictionary<string, LocValSet> Translations = new();

        public static Dictionary<string, List<AssetLibraryManifest.Entry>> ByType => baseDataTypeToEntries;

        public static IEnumerable<MinedAsset> All => db.Values;

        public static string Translate(string lang, string key)
        {
            var (table, id) = LocKeys.Lookup(key);
            return Translations[lang].Lookup(table, id) ?? key;
        }
        public static string Translate(string key)
        {
            if (GlobalLanguageSet.Language == null)
            {
                throw new Exception();
            }

            return Translate(GlobalLanguageSet.Language, key);
        }

        private static readonly AssetContext assets = new();

        public static void Init(string path)
        {
            loader = new FlatBufferAssetLoader(path);
            if (loader == null)
                throw new NullReferenceException();

            TextureDecoder.ForceLoaded();

            bobwr = File.CreateText(@"D:\bobber.txt");

            var streamingAssetsDir = Path.GetDirectoryName(Path.GetDirectoryName(path))!;
            var locDir = Path.Combine(streamingAssetsDir, "aa", "StandaloneWindows64");

            assets.AddBundle(Path.Combine(locDir, "localization-assets-shared_assets_all.bundle"));
            //assets.AddBundle(Path.Combine(locDir, ""));
            List<string> translationBundles = new();

            string criticalAssetsBundle = "";
            string allAssetsBundle = "";

            foreach (var maybe in Directory.GetFiles(locDir)) {
                if (maybe.Contains("localization-string-tables-"))
                {
                    translationBundles.Add(Path.GetFileName(maybe));
                    assets.AddBundle(maybe);
                }

                if (maybe.Contains("artemisuigroup_assets_all_"))
                {
                    allAssetsBundle = Path.GetFileName(maybe);
                    assets.AddBundle(maybe);
                }

                if (maybe.Contains("mainmenuloadingcriticalgroup_assets_all_"))
                {
                    criticalAssetsBundle = Path.GetFileName(maybe);
                    assets.AddBundle(maybe);
                }
            }

            foreach (var obj in assets.assetsByBundle[criticalAssetsBundle][0].ObjectIndex.Where(x => x.ClassType == ClassIDType.Sprite))
            {
                UnityAssetReference assetRef = new(0, obj.m_PathID);
                var ptrToSprite = new PPtr<Sprite>(assetRef, obj.Owner);
                var sprite = ptrToSprite.Object;
                //bobwr.Write($"adding icon: {sprite.Name}\n");
                IconsByName.Add(sprite.Name, sprite);
            }

            foreach (var obj in assets.assetsByBundle[allAssetsBundle][0].ObjectIndex.Where(x => x.ClassType == ClassIDType.Sprite))
            {
                UnityAssetReference assetRef = new(0, obj.m_PathID);
                var ptrToSprite = new PPtr<Sprite>(assetRef, obj.Owner);
                var sprite = ptrToSprite.Object;
                //bobwr.Write($"adding icon: {sprite.Name}\n");
                if (!IconsByName.ContainsKey(sprite.Name))
                {
                    IconsByName.Add(sprite.Name, sprite);
                }
            }

            var keyToIdAssets = assets.assetsByBundle["localization-assets-shared_assets_all.bundle"][0];

            foreach (var obj in  keyToIdAssets.ObjectIndex.Where(x => x.ClassType == ClassIDType.MonoBehaviour))
            {
                var sharedTable = AssetMining.MineMonoBehaviour(obj.serializedType.TypeTree, new AssetFileReader(obj))["Base"].Asset;
                LocKeyTable table = new()
                {
                    TableName = sharedTable["m_TableCollectionName"].String
                };

                foreach (var entry in sharedTable["m_Entries"].Enumerate().Select(x => x.Asset))
                {
                    table.Add(entry["m_Key"].String, (long)entry["m_Id"].Value);
                }

                LocKeys.Tables.Add(table);
            }

            foreach (var lang in translationBundles)
            {

                var translationBundle = assets.assetsByBundle[lang][0];
                foreach (var obj in translationBundle.ObjectIndex.Where(x => x.ClassType == ClassIDType.MonoBehaviour))
                {
                    //var str = TreeDumper.ReadTypeString(obj.serializedType.TypeTree, new AssetFileReader(obj));
                    //bobwr.Write(str);
                    //bobwr.Write("\n");

                    var stringTable = AssetMining.MineMonoBehaviour(obj.serializedType.TypeTree, new AssetFileReader(obj))["Base"].Asset;
                    var suffix = stringTable["m_LocaleId"]["m_Code"].String;
                    if (!Translations.TryGetValue(suffix, out var locVals))
                    {
                        locVals = new();
                        Translations.Add(suffix, locVals);
                    }

                    LocValTable table = new()
                    {
                        TableName = stringTable["m_Name"].String.Replace($"_{suffix}", "")
                    };

                    locVals.Tables.Add(table.TableName, table);

                    foreach (var entry in stringTable["m_TableData"].Enumerate().Select(x => x.Asset))
                    {
                        table.Add((long)entry["m_Id"].Value, entry["m_Localized"].String);
                    }
                }
            }



            var manifestAsset = loader.LoadAsset<FbTextAsset>(FbAssetType.FbTextAsset, AssetLibraryServerDesktop.ManifestFileName);

            if (manifestAsset == null)
            {
                throw new NotSupportedException("could not load manifest");
            }

            var manifest = JsonSerializerShared.Deserialize<AssetLibraryManifest>(manifestAsset.Value.Text, null);

            //AssetLibraryServerDesktop.SetManifestClassTypes(manifest, "Assembly-CSharp");

            asm = Assembly.LoadFile(@"C:\Program Files (x86)\Steam\steamapps\common\Inkbound\Inkbound_Data\Managed\Assembly-CSharp.dll");

            foreach (AssetLibraryManifest.Entry entry in manifest.entries)
            {
                if (entry.assetID._fileID == DefaultObjectFileID)
                {
                    nameToEntry[entry.name] = entry;
                }

                guidAndFileIDToEntry.Add(entry.assetID, entry);

                if (!string.IsNullOrEmpty(entry.dataId))
                {
                    dataIdToEntry.Add(entry.dataId, entry);
                }
                if (entry.className != null)
                {
                    List<AssetLibraryManifest.Entry> list;
                    var typeName = Path.GetExtension(entry.className)[1..];
                    if (!baseDataTypeToEntries.TryGetValue(typeName, out list!))
                    {
                        list = new List<AssetLibraryManifest.Entry>();
                        baseDataTypeToEntries.Add(typeName, list);
                    }
                    list.Add(entry);
                }
            }

        }

        private static void Add(MinedAsset asset, AssetLibraryManifest.Entry entry)
        {
            db[entry.assetID] = asset;
            byData[entry.dataId] = asset;
            byName[entry.name] = asset;
        }

        public static MinedAsset Get(AssetID id) => db[id];

        public static MinedAsset Action(string dataId) => byData[dataId];

        public static bool TryGetByData(string dataId, out MinedAsset asset)
        {
            return byData.TryGetValue(dataId, out asset!);
        }


        public static MinedAsset GetOrLoadAsset(AssetLibraryManifest.Entry entry)
        {
            if (!MineDb.db.TryGetValue(entry.assetID, out var asset))
            {
                var type = asm?.GetType(entry.className);

                if (type is null)
                {
                    throw new TypeLoadException();
                }

                var blob = AssetLibraryServerDesktop.LoadYamlTextOrBinaryCommon(loader!, entry);
                var stream = blob.binaryData!;

                Miner miner = new(stream);
                asset = miner.Mine(type).Asset;
                asset.IsRoot = true;
                asset.AssetName = entry.name;
                asset.DataId = entry.dataId;
                asset.AssetGuid = entry.assetID._guid;
                asset.AssetFile = entry.assetID._fileID;
                asset.TypeName = entry.className;

                //output.WriteLine("loaded: " + entry.dataId);

                MineDb.Add(asset, entry);
            }
            return asset;
        }


        public static MinedAsset LoadAsset(string name)
        {
            var entry = nameToEntry[name];
            return GetOrLoadAsset(entry);
        }

        public static void LoadAll(string type)
        {
            foreach (var entry in baseDataTypeToEntries[type])
            {
                GetOrLoadAsset(entry);
            }

        }

        public static MinedAsset Lookup(string v) => byName[v];

        public static IEnumerable<MinedAsset> AssetsByType(string v) => ByType[v].Select(x => Get(x.assetID));

        public static void LoadAll()
        {
            foreach (var type in baseDataTypeToEntries.Keys)
            {
                bobwr?.Write($"Loading type: {type}\n");
                LoadAll(type);
            }
            bobwr?.Flush();
            bobwr?.Close();
        }

        public static IDisposable ActivateLanguage(string language)
        {
            return new GlobalLanguageSet(language);
        }

        public static Type GetClassTypeByGuid(string guid, long fileID)
        {
            return asm?.GetType(guidAndFileIDToEntry[new(fileID, guid)].className) ?? throw new Exception();
        }
    }

    public class GlobalLanguageSet : IDisposable
    {
        public static string? Language = null;

        public GlobalLanguageSet(string language)
        {
            Language = language;
        }

        public void Dispose()
        {
            Language = null;
        }
    }
}

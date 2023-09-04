using ShinyShoe;
using ShinyShoe.Ares;
using ShinyShoe.SharedDataLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
                    throw new NotImplementedException("array");
                    //value = this.DeserializeArray(expectedType);
                    break;

                case SoBinarySerializerBase.Token.List:
                    value = this.DeserializeList(hint);
                    break;

                case SoBinarySerializerBase.Token.Null:
                    value = MinedField.Null;
                    break;

                case SoBinarySerializerBase.Token.Guid:
                    throw new NotImplementedException("guid");
                    //value = this.DeserializeGuid(expectedType);
                    break;

                default:
                    throw new Exception();
            }

            return new(value);
        }

        private object DeserializeEnum(Type hint)
        {
            SoBinarySerializerBase.Token token = this.ReadToken();
            object value = this.DeserializePrimitive();
            return Enum.ToObject(hint, value);
        }

        private object DeserializeList(Type hint)
        {
            int num = reader.ReadInt32();

            Type elemHint = hint.GetGenericArguments()[0];

            MinedField[] arr = new MinedField[num];
            level++;
            for (int i = 0; i < num; i++)
            {
                arr[i] = Mine(elemHint);
            }
            level--;
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
                }
                else
                {
                    var value = Mine(fieldInfo.FieldType);
                    obj.fields.Add(fieldName, value);
                }

                if (reader.BaseStream.Position != pos + len)
                {
                    throw new Exception("length mismatch");
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

        public MinedAsset Asset => (Value as MinedAsset)!;

        public string String => (Value as string)!;

        public string Localized()
        {
            var raw = (String.Clone() as string)!;
            Loccer.ApplyLocalizationParams(ref raw, param =>
            {
                if (Loccer.TryGetLocalizationParameterActionParamAsInt(param, out var value))
                    return value.ToString();
                else if (Loccer.TryGetLocalizationParameterStatusEffectProcChance(param, out var value2))
                    return value2.ToString();
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

        public MinedAsset Deref() => MineDb.Get(this["guid"].String);
        public MinedAsset Deref(string v) => Asset.Deref(v);

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

        public MinedField this[string index] => fields[index];

        public MinedAsset Deref(string v)
        {
            return this[v].Deref();
        }

        internal IEnumerable<MinedAsset> EnumerateAssetLinks(string v)
        {
            return this[v].EnumerateAssetLinks();
        }

        public string AssetName = "";
    }

    public class MineDb
    {
        private static readonly Dictionary<string, MinedAsset> db = new();
        private static readonly Dictionary<string, MinedAsset> byData = new();
        private static readonly Dictionary<string, MinedAsset> byName = new();
        private static FlatBufferAssetLoader? loader;
        private static readonly Dictionary<string, AssetLibraryManifest.Entry> nameToEntry = new();
        private static readonly Dictionary<string, AssetLibraryManifest.Entry> dataIdToEntry = new();
        private static readonly Dictionary<string, List<AssetLibraryManifest.Entry>> baseDataTypeToEntries = new();

        private const long DefaultObjectFileID = 11400000L;

        public static Dictionary<string, List<AssetLibraryManifest.Entry>> ByType => baseDataTypeToEntries;

        public static void Init(string path)
        {
            loader = new FlatBufferAssetLoader(path);
            if (loader == null)
                throw new NullReferenceException();

            var manifestAsset = loader.LoadAsset<FbTextAsset>(FbAssetType.FbTextAsset, AssetLibraryServerDesktop.ManifestFileName);

            if (manifestAsset == null)
            {
                throw new NotSupportedException("could not load manifest");
            }

            var manifest = JsonSerializerShared.Deserialize<AssetLibraryManifest>(manifestAsset.Value.Text, null);

            AssetLibraryServerDesktop.SetManifestClassTypes(manifest, "Assembly-CSharp");

            foreach (AssetLibraryManifest.Entry entry in manifest.entries)
            {
                if (entry.assetID._fileID == DefaultObjectFileID)
                {
                    nameToEntry[entry.name] = entry;
                }
                if (!string.IsNullOrEmpty(entry.dataId))
                {
                    dataIdToEntry.Add(entry.dataId, entry);
                }
                {
                    List<AssetLibraryManifest.Entry> list;
                    if (!baseDataTypeToEntries.TryGetValue(entry.classType.Name, out list!))
                    {
                        list = new List<AssetLibraryManifest.Entry>();
                        baseDataTypeToEntries.Add(entry.classType.Name, list);
                    }
                    list.Add(entry);
                }
            }



        }

        public static void Add(MinedAsset asset, AssetLibraryManifest.Entry entry)
        {
            db[entry.assetID._guid] = asset;
            byData[entry.dataId] = asset;
            byName[entry.name] = asset;
        }

        public static MinedAsset Get(string guid) => db[guid];

        public static MinedAsset Action(string dataId) => byData[dataId];

        public static bool TryGetByData(string dataId, out MinedAsset asset)
        {
            return byData.TryGetValue(dataId, out asset);
        }

        public static MinedAsset Get(AssetLibraryManifest.Entry entry) => Get(entry.assetID._guid);

        public static MinedAsset GetOrLoadAsset(AssetLibraryManifest.Entry entry)
        {
            if (!MineDb.TryGetByData(entry.dataId, out var asset))
            {
                var type = entry.classType;

                var blob = AssetLibraryServerDesktop.LoadYamlTextOrBinaryCommon(loader!, entry);
                var stream = blob.binaryData!;

                Miner miner = new(stream);
                asset = miner.Mine(type).Asset;
                asset.AssetName = entry.name;

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
            foreach (var ability in baseDataTypeToEntries[type])
            {
                GetOrLoadAsset(ability);
            }
        }

        internal static MinedAsset Lookup(string v) => byName[v];

        internal static IEnumerable<MinedAsset> AssetsByType(string v) => ByType[v].Select(x => Get(x));
    }
}

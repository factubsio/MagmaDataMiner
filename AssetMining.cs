﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiGen.Assets;

namespace MagmaDataMiner
{
    public static class AssetMining
    {
        public static MinedAsset MineMonoBehaviour(List<TypeTreeNode> nodes, AssetFileReader reader)
        {
            reader.Reset();
            MinedAsset mb = new();

            for (int i = 0; i < nodes.Count; i++)
            {
                var (fieldName, fieldValue) = ReadField(nodes, reader, ref i);
                mb.fields.Add(fieldName, fieldValue);
            }
            //var readed = reader.Position - reader.byteStart;
            //if (readed != reader.byteSize)
            //{
            //    Logger.Info($"Error while read type, read {readed} bytes but expected {reader.byteSize} bytes");
            //}
            return mb;
        }

        private static (string, MinedField) ReadField(List<TypeTreeNode> m_Nodes, AssetFileReader reader, ref int i)
        {
            var m_Node = m_Nodes[i];
            var level = m_Node.Level;
            var varTypeStr = m_Node.Type;
            var varNameStr = m_Node.Name;
            object value = "";
            var align = (m_Node.MetaFlag & 0x4000) != 0;
            switch (varTypeStr)
            {
                case "SInt8":
                    value = reader.ReadSByte();
                    break;
                case "UInt8":
                    value = reader.ReadByte();
                    break;
                case "char":
                    value = BitConverter.ToChar(reader.ReadBytes(2), 0);
                    break;
                case "short":
                case "SInt16":
                    value = reader.ReadInt16();
                    break;
                case "UInt16":
                case "unsigned short":
                    value = reader.ReadUInt16();
                    break;
                case "int":
                case "SInt32":
                    value = reader.ReadInt32();
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    value = reader.ReadUInt32();
                    break;
                case "long long":
                case "SInt64":
                    value = reader.ReadInt64();
                    break;
                case "UInt64":
                case "unsigned long long":
                case "FileSize":
                    value = reader.ReadUInt64();
                    break;
                case "float":
                    value = reader.ReadSingle();
                    break;
                case "double":
                    value = reader.ReadDouble();
                    break;
                case "bool":
                    value = reader.ReadBoolean();
                    break;
                case "string":
                    value = reader.ReadAlignedString();
                    var toSkip = GetNodes(m_Nodes, i);
                    i += toSkip.Count - 1;
                    break;
                case "map":
                    {
                        if ((m_Nodes[i + 1].MetaFlag & 0x4000) != 0)
                            align = true;

                        var size = reader.ReadInt32();


                        var map = GetNodes(m_Nodes, i);
                        i += map.Count - 1;
                        var first = GetNodes(map, 4);
                        var next = 4 + first.Count;
                        var second = GetNodes(map, next);
                        for (int j = 0; j < size; j++)
                        {
                            //sb.AppendFormat("{0}[{1}]\r\n", new string('\t', level + 2), j);
                            //sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 2), "pair", "data");
                            int tmp1 = 0;
                            int tmp2 = 0;
                            var key = ReadField(first, reader, ref tmp1);
                            var val = ReadField(second, reader, ref tmp2);
                        }
                        break;
                    }
                case "TypelessData":
                    {
                        //append = false;
                        var size = reader.ReadInt32();
                        reader.ReadBytes(size);
                        i += 2;
                        //sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
                        //sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level), "int", "size", size);
                        break;
                    }
                default:
                    {
                        if (i < m_Nodes.Count - 1 && m_Nodes[i + 1].Type == "Array") //Array
                        {
                            if ((m_Nodes[i + 1].MetaFlag & 0x4000) != 0)
                                align = true;
                            var size = reader.ReadInt32();
                            var vector = GetNodes(m_Nodes, i);
                            MinedField[] arr = new MinedField[size];
                            i += vector.Count - 1;
                            for (int j = 0; j < size; j++)
                            {
                                int tmp = 3;
                                var (_, elem) = ReadField(vector, reader, ref tmp);
                                arr[j] = elem;
                            }
                            value = arr;
                            break;
                        }
                        else //Class
                        {
                            var obj = new MinedAsset();
                            var @class = GetNodes(m_Nodes, i);
                            i += @class.Count - 1;
                            for (int j = 1; j < @class.Count; j++)
                            {
                                var (fName, fValue) = ReadField(@class, reader, ref j);
                                obj.fields.Add(fName, fValue);
                            }
                            value = obj;
                            break;
                        }
                    }
            }

            if (align)
                reader.AlignStream();

            return (varNameStr, new(value));
        }

        private static List<TypeTreeNode> GetNodes(List<TypeTreeNode> m_Nodes, int index)
        {
            var nodes = new List<TypeTreeNode>();
            nodes.Add(m_Nodes[index]);
            var level = m_Nodes[index].Level;
            for (int i = index + 1; i < m_Nodes.Count; i++)
            {
                var member = m_Nodes[i];
                var level2 = member.Level;
                if (level2 <= level)
                {
                    return nodes;
                }
                nodes.Add(member);
            }
            return nodes;
        }
    }
}

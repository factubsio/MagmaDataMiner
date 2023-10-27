using FixedPointy;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ShinyShoe.Ares.ConditionalNodes;
using ShinyShoe.SharedDataLoader.XNode;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MagmaDataMiner
{
    public class MinedNode
    {
        public Point Position;
        public string ShinyType = "";
        public readonly Dictionary<string, MinedPort> Ports = new();
        public Color Color = Color.DarkGray;
        public bool IsDark = false;

        public readonly Dictionary<string, object> Parameters = new();

        public object? UserValue;
    }

    public class MinedPort
    {
        public record struct Link(MinedNode Node, string Target);

        public readonly List<Link> Links = new();
        public readonly string Direction;
        public readonly string ConnectionType;
        public readonly string Constraint;
        public readonly string TypeName;
        public readonly string Name;

        public bool IsStrict => Constraint == "Strict";

        public MinedPort(string name, string direction, string connectionType, string constraint, string typeName)
        {
            Name = name;
            this.Direction = direction;
            this.ConnectionType = connectionType;
            this.Constraint = constraint;
            this.TypeName = typeName;
        }
    }

    public class MinedGraph
    {
        public string Name;
        public readonly List<MinedNode> Nodes = new();

        public MinedGraph(string assetName)
        {
            Name = assetName;
        }
    }

    public class AresSimulator
    {
        private static Dictionary<long, MinedAsset> nodes = new();
        private static Dictionary<long, MinedNode> minedNodes = new();

        private const string prefix = "ShinyShoe.Ares.SharedSOs.";
        private static readonly int prefixLength = prefix.Length;

        private static readonly HashSet<string> CommonFields = new()
        {
"m_ObjectHideFlags",
"m_CorrespondingSourceObject",
"m_PrefabInstance",
"m_PrefabAsset",
"m_GameObject",
"m_Enabled",
"m_EditorHideFlags",
"m_Script",
"m_Name",
"m_EditorClassIdentifier",
"graph",
"position",
"ports",
"comment",
        };

        public static MinedGraph ParseGraph(MinedAsset graph)
        {
            MinedGraph result = new(graph.AssetName);

            nodes.Clear();
            minedNodes.Clear();

            foreach (var nodeFileID in graph["nodes"].Enumerate().Select(x => (long)x["fileID"].Value))
            {
                MinedNode minedNode = new();

                var node = MineDb.Get(new(nodeFileID, graph.AssetGuid));

                nodes.Add(nodeFileID, node);
                minedNodes.Add(nodeFileID, minedNode);
                result.Nodes.Add(minedNode);
            }

            foreach (var (id, minedNode) in minedNodes)
            {
                var data = nodes[id];
                minedNode.Position.X = (int)data["position"]["x"].Float;
                minedNode.Position.Y = (int)data["position"]["y"].Float;
                var lastDot = data.TypeName.LastIndexOf('.');
                var (color, dark) = LookupTint(data.TypeName);
                minedNode.Color = color;
                minedNode.IsDark = dark;
                minedNode.ShinyType = data.TypeName.Remove(0, lastDot + 1);

                foreach (var (name, field) in data.fields.Where(x => !CommonFields.Contains(x.Key)))
                {
                    if (field.IsFlat)
                    {
                        minedNode.Parameters.Add(name, field.Value);
                    }
                    else if (field.IsArray)
                    {
                        minedNode.Parameters.Add(name, "[...]");
                    }
                    else if (field.IsAsset)
                    {
                        if (field.Asset.fields.ContainsKey("guid") && field.Asset.fields.ContainsKey("fileID"))
                        {
                            var linkTo = field.Deref();
                            minedNode.Parameters.Add(name, $"&{Path.GetFileName(linkTo.AssetName)}");
                        }
                        else
                        {
                            minedNode.Parameters.Add(name, field.Asset.RawJson);
                        }
                    }
                    else
                    {
                        minedNode.Parameters.Add(name, "<unknown>");
                    }

                }

                foreach (var port in data["ports"]["valuesList"].Enumerate())
                {
                    var name = port["_fieldName"].String;
                    var direction = (NodePort.IO)port["_direction"].Value;
                    var connectionType = (Node.ConnectionType)port["_connectionType"].Value;
                    var typeConstraint = (Node.TypeConstraint)port["_typeConstraint"].Value;

                    var portTypeQualifiedName = port["_typeQualifiedName"].String;
                    var portType = Type.GetType(portTypeQualifiedName);

                    if (portType == null)
                    {
                        throw new NotSupportedException();
                    }

                    MinedPort minedPort = new(name, direction.ToString(), connectionType.ToString(), typeConstraint.ToString(), portType.FullName!);
                    minedNode.Ports.Add(name, minedPort);

                    foreach (var conn in port["connections"].Enumerate())
                    {
                        var targetNode = minedNodes[(long)conn["node"]["fileID"].Value];
                        var targetPort = conn["fieldName"].String;
                        minedPort.Links.Add(new(targetNode, targetPort));

                        if (conn["reroutePoints"].Length > 0)
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }

            return result;
        }

        private static readonly Dictionary<string, (Color Color, bool IsDark)> TintCache = new();
        private static (Color, bool) LookupTint(string typeName)
        {
            if (!TintCache.TryGetValue(typeName, out var colorAndDark)) {
                colorAndDark.Color = Color.DarkGray;

                var type = MineDb.asm?.GetType(typeName);
                if (type == null)
                {
                    colorAndDark.Color = Color.Black;
                }
                else
                {
                    foreach (var attrib in type.GetCustomAttributesData())
                    {
                        if (attrib.AttributeType.FullName == "ShinyShoe.SharedDataLoader.XNode.Node+NodeTintAttribute")
                        {
                            var args = attrib.ConstructorArguments;
                            if (args.Count == 1)
                            {
                                var hexString = args[0].Value as string;
                                colorAndDark.Color = ColorTranslator.FromHtml(hexString!);
                            }
                            break;
                        }
                    }
                }

                double luminance = (0.299 * colorAndDark.Color.R + 0.587 * colorAndDark.Color.G + 0.114 * colorAndDark.Color.B) / 255;

                // If the color is "dark" (luminance is less than 0.5), return white; otherwise, return black
                colorAndDark.IsDark = luminance < 0.5;

                TintCache[typeName] = colorAndDark;
            }
            return colorAndDark;
        }

        public static List<(string ActionName, MinedGraph Graph)> ParseAbilityGraph(MinedAsset abilityData)
        {
            List<(string ActionName, MinedGraph Graph)> result = new();

            foreach (var actionWrapper in abilityData["actionDataWrappers"]["entries"].Enumerate())
            {
                var actionData = actionWrapper.Deref("actionData");

                var graph = actionData.Deref("actionGraph");

                result.Add((actionData.AssetName, ParseGraph(graph)));

            }

            return result;
        }

        public static int ProcessAbilityGraph(MinedAsset abilityData)
        {
            int damage = 0;

            foreach (var actionWrapper in abilityData["actionDataWrappers"]["entries"].Enumerate())
            {
                var actionData = actionWrapper.Deref("actionData");

                var graph = actionData.Deref("actionGraph");

                nodes.Clear();

                ResolveContext ctx = new(actionData);

                foreach (var nodeFileID in graph["nodes"].Enumerate().Select(x => (long)x["fileID"].Value))
                {
                    var node = MineDb.Get(new(nodeFileID, graph.AssetGuid));
                    nodes.Add(nodeFileID, node);
                }

                MinedAsset? current = null;

                foreach (var node in nodes.Values)
                {
                    if (node.TypeName == "ShinyShoe.Ares.SharedSOs.SetupNodes.ActionNodeEntry")
                    {
                        current = node;
                    }

                    if (node.TypeName == "ShinyShoe.Ares.SharedSOs.ActionNodes.DamageFlatAmountActionNode")
                    {
                        Connection amount = GetPort(node, "amount");

                        damage += ResolveIntNode(ctx, amount);
                    }
                }
            }

            return damage;
        }
        private static BubbleFix ResolveFixNode(ResolveContext ctx, Connection conn)
        {
            if (conn.Node == null)
            {
                return new(0);
            }

            switch (conn.Node.TypeName)
            {
                case "ShinyShoe.Ares.SharedSOs.UnitNodes.GetDistanceBetweenTwoEntitiesNode":
                    return new(0);
                default:
                    throw new NotSupportedException();
            }
        }

        private static int ResolveIntNode(ResolveContext ctx, Connection conn)
        {
            if (conn.Node == null)
                return 0;

            switch (conn.Node.TypeName)
            {
                case "ShinyShoe.Ares.SharedSOs.ConstantNodes.IntegerConstantNode":
                    return conn.Node["value"].Int;
                case "ShinyShoe.Ares.SharedSOs.MathNodes.FixFloorNode":
                    return (int)BubbleFixMath.Floor(ResolveFixNode(ctx, GetPort(conn.Node, "input")));
                case "ShinyShoe.Ares.SharedSOs.MathNodes.DivisionNode":
                    {
                        var in1 = GetPort(conn.Node, "in1");
                        var in2 = GetPort(conn.Node, "in2");
                        var div = ResolveIntNode(ctx, in2) ;
                        return ResolveIntNode(ctx, in1) / (div == 0 ? 1 : div);
                    }
                case "ShinyShoe.Ares.SharedSOs.MathNodes.MultiplicationNode":
                    {
                        var in1 = GetPort(conn.Node, "in1");
                        var in2 = GetPort(conn.Node, "in2");
                        return ResolveIntNode(ctx, in1) * ResolveIntNode(ctx, in2);
                    }
                case "ShinyShoe.Ares.SharedSOs.MathNodes.AdditionNode":
                    {
                        var in1 = GetPort(conn.Node, "in1");
                        var in2 = GetPort(conn.Node, "in2");
                        return ResolveIntNode(ctx, in1) + ResolveIntNode(ctx, in2);
                    }
                case "ShinyShoe.Ares.SharedSOs.SetupNodes.ActionNodeActionParams":
                    return conn.PortName switch
                    {
                        "paramInt" => ctx.ActionData["paramInt"].Int,
                        "paramInt2" => ctx.ActionData["paramInt2"].Int,
                        "paramPercent1" => ctx.ActionData["paramPercent1"].Int,
                        "paramPercent2" => ctx.ActionData["paramPercent2"].Int,
                        _ => throw new NotSupportedException(),
                    };
                case "ShinyShoe.Ares.SharedSOs.UnitStatNodes.GetUnitStatusCountNode":
                case "ShinyShoe.Ares.SharedSOs.UnitStatNodes.GetNumConditionalUnitsNode":
                    return 0;
                case "ShinyShoe.Ares.SharedSOs.ConditionalNodes.IntSelectorNode":
                    {
                        var choose = ResolveBoolNode(ctx, GetPort(conn.Node, "input"));
                        return 0;
                    }
                case "ShinyShoe.Ares.SharedSOs.UnitStatNodes.GetUnitStatNode":
                    var statData = ResolveStatNode(ctx, GetPort(conn.Node, "statData"));
                    return statData switch
                    {
                        "_blank_" => 0,
                        _ => throw new NotSupportedException(),
                    };
                default:
                    throw new NotSupportedException();
            }
        }

        private static bool ResolveBoolNode(ResolveContext ctx, Connection conn)
        {
            if (conn.Node == null)
                return false;

            switch (conn.Node.TypeName)
            {
                case "ShinyShoe.Ares.SharedSOs.ConstantNodes.BoolConstantNode":
                    return conn.Node["value"].Bool;
                case "ShinyShoe.Ares.SharedSOs.UnitStatNodes.GetUnitHasStatusNode":
                    return false;
                case "ShinyShoe.Ares.SharedSOs.ConditionalNodes.IntCompareNode":
                    {
                        int in1 = ResolveIntNode(ctx, GetPort(conn.Node, "value1"));
                        int in2 = ResolveIntNode(ctx, GetPort(conn.Node, "value2"));
                        switch ((IntCompareNode.IntComparator)conn.Node["comparator"].Value)
                        {
                            case IntCompareNode.IntComparator.GreaterThan:
                                return in1 > in2;
                            case IntCompareNode.IntComparator.LessThan:
                                return in1 < in2;
                            case IntCompareNode.IntComparator.GreaterEqualTo:
                                return in1 >= in2;
                            case IntCompareNode.IntComparator.LessEqualTo:
                                return in1 <= in2;
                        }
                        return in1 == in2;

                    }

                default:
                    throw new NotSupportedException();
            }
        }

        private static string ResolveStatNode(ResolveContext ctx, Connection conn)
        {
            if (conn.Node == null)
            {
                return "_blank_";
            }

            MinedAsset statData;
            switch (conn.Node.TypeName)
            {
                case "ShinyShoe.Ares.SharedSOs.ConstantNodes.StatDataConstantNode":
                    statData = conn.Node.Deref("value");
                    break;
                case "ShinyShoe.Ares.SharedSOs.SetupNodes.ActionNodeActionParams":
                    statData = conn.PortName switch
                    {
                        "paramStatData" => ctx.ActionData.Deref("paramStatData"),
                        _ => throw new NotSupportedException(),
                    };
                    break;
                default:
                    throw new NotSupportedException();
            }

            return "_blank_";
        }

        private static Connection GetPort(MinedAsset node, string portName, int index = 0)
        {
            var (found, portRaw) = node["ports"]["valuesList"].Enumerate().MaybeFirst(x => x["_fieldName"].String == portName);
            if (!found)
                return new(null, "");

            var connections = portRaw["connections"];

            if (index >= connections.Length)
                return new(null, "");

            var conn = connections.At(index);
            var target = nodes[(long)conn["node"]["fileID"].Value];
            return new(target, conn["fieldName"].String);
        }


        public record class ResolveContext(MinedAsset ActionData);
        public record class Connection(MinedAsset? Node, string PortName);

    }
}

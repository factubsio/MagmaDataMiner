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
        public string ShinyTypeFull = "";
        public readonly Dictionary<string, MinedPort> Ports = new();
        public Color Color = Color.DarkGray;
        public bool IsDark = false;
        public readonly MinedAsset Data;

        public MinedNode(MinedAsset data)
        {
            Data = data;
        }

        public readonly Dictionary<string, object> Parameters = new();

        public object? UserValue;
    }

    public class MinedPort
    {
        public record struct Link(MinedNode Node, string Target);

        public readonly MinedNode Node;
        public readonly List<Link> Links = new();
        public readonly string Direction;
        public readonly string ConnectionType;
        public readonly string Constraint;
        public readonly string TypeName;
        public readonly string Name;

        public bool IsStrict => Constraint == "Strict";

        public MinedPort(MinedNode owner, string name, string direction, string connectionType, string constraint, string typeName)
        {
            Node = owner;
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
    public static class AresParser
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

        private static readonly Dictionary<string, MinedGraph> GraphCache = new();

        public static MinedGraph ParseGraph(MinedAsset graph)
        {
            if (GraphCache.TryGetValue(graph.AssetName, out var cached))
            {
                return cached;
            }

            MinedGraph result = new(graph.AssetName);
            GraphCache[graph.AssetName] = result;

            nodes.Clear();
            minedNodes.Clear();

            foreach (var nodeFileID in graph["nodes"].Enumerate().Select(x => (long)x["fileID"].Value))
            {

                var node = MineDb.Get(new(nodeFileID, graph.AssetGuid));

                MinedNode minedNode = new(node);

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
                minedNode.ShinyTypeFull = data.TypeName;

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

                    MinedPort minedPort = new(minedNode, name, direction.ToString(), connectionType.ToString(), typeConstraint.ToString(), portType.FullName!);
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
            if (!TintCache.TryGetValue(typeName, out var colorAndDark))
            {
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
    }

    public class AresSimulator
    {

        public static List<(string ActionName, MinedGraph Graph)> ParseAbilityGraph(MinedAsset abilityData)
        {
            List<(string ActionName, MinedGraph Graph)> result = new();

            foreach (var actionWrapper in abilityData["actionDataWrappers"]["entries"].Enumerate())
            {
                var actionData = actionWrapper.Deref("actionData");

                var graph = actionData.Deref("actionGraph");

                result.Add((actionData.AssetName, AresParser.ParseGraph(graph)));

            }

            return result;
        }


        public static int ProcessActionDamage(MinedAsset actionData)
        {
            var graphHandle = actionData.Deref("actionGraph");
            var graph = AresParser.ParseGraph(graphHandle);

            ResolveContext ctx = new(actionData);
            int damage = 0;
            foreach (var node in graph.Nodes)
            {
                if (node.ShinyTypeFull == "ShinyShoe.Ares.SharedSOs.ActionNodes.DamageFlatAmountActionNode")
                {
                    damage += ResolveIntNode(ctx, GetSource(node, "amount"));
                }
            }
            return damage;
        }

        public static int ProcessAbilityGraph(MinedAsset abilityData)
        {
            int damage = 0;

            foreach (var actionWrapper in abilityData["actionDataWrappers"]["entries"].Enumerate())
            {
                var actionData = actionWrapper.Deref("actionData");

                var graphData = actionData.Deref("actionGraph");

                var graph = AresParser.ParseGraph(graphData);

                ResolveContext ctx = new(actionData);

                MinedNode? current = null;

                foreach (var node in graph.Nodes)
                {
                    if (node.ShinyTypeFull == "ShinyShoe.Ares.SharedSOs.SetupNodes.ActionNodeEntry")
                    {
                        current = node;
                    }

                    if (node.ShinyTypeFull == "ShinyShoe.Ares.SharedSOs.ActionNodes.DamageFlatAmountActionNode")
                    {
                        var amount = GetSource(node, "amount");

                        damage += ResolveIntNode(ctx, amount);
                    }
                }
            }

            return damage;
        }
        private static BubbleFix ResolveFixNode(ResolveContext ctx, MinedPort? port)
        {
            if (port == null)
                return new(0);

            switch (port.Node.ShinyTypeFull)
            {
                case "ShinyShoe.Ares.SharedSOs.UnitNodes.GetDistanceBetweenTwoEntitiesNode":
                    return new(0);
                default:
                    throw new NotSupportedException();
            }
        }

        private static int ResolveIntNode(ResolveContext ctx, MinedPort? port)
        {
            if (port == null)
                return 0;

            switch (port.Node.ShinyTypeFull)
            {
                case "ShinyShoe.Ares.SharedSOs.ConstantNodes.IntegerConstantNode":
                    return port.Node.Data["value"].Int;
                case "ShinyShoe.Ares.SharedSOs.MathNodes.FixFloorNode":
                    return (int)BubbleFixMath.Floor(ResolveFixNode(ctx, GetSource(port, "input")));
                case "ShinyShoe.Ares.SharedSOs.MathNodes.DivisionNode":
                    {
                        var in1 = GetSource(port, "in1");
                        var in2 = GetSource(port, "in2");
                        var div = ResolveIntNode(ctx, in2) ;
                        return ResolveIntNode(ctx, in1) / (div == 0 ? 1 : div);
                    }
                case "ShinyShoe.Ares.SharedSOs.MathNodes.MultiplicationNode":
                    {
                        var in1 = GetSource(port, "in1");
                        var in2 = GetSource(port, "in2");
                        return ResolveIntNode(ctx, in1) * ResolveIntNode(ctx, in2);
                    }
                case "ShinyShoe.Ares.SharedSOs.MathNodes.AdditionNode":
                    {
                        var in1 = GetSource(port, "in1");
                        var in2 = GetSource(port, "in2");
                        return ResolveIntNode(ctx, in1) + ResolveIntNode(ctx, in2);
                    }
                case "ShinyShoe.Ares.SharedSOs.SetupNodes.ActionNodeActionParams":
                    return port.Name switch
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
                        var choose = ResolveBoolNode(ctx, GetSource(port, "input"));
                        return 0;
                    }
                case "ShinyShoe.Ares.SharedSOs.UnitStatNodes.GetUnitStatNode":
                    var statData = ResolveStatNode(ctx, GetSource(port, "statData"));
                    return statData switch
                    {
                        "_blank_" => 0,
                        _ => throw new NotSupportedException(),
                    };
                default:
                    throw new NotSupportedException();
            }
        }

        private static bool ResolveBoolNode(ResolveContext ctx, MinedPort? port)
        {
            if (port == null)
                return false;

            switch (port.Node.ShinyTypeFull)
            {
                case "ShinyShoe.Ares.SharedSOs.ConstantNodes.BoolConstantNode":
                    return port.Node.Data["value"].Bool;
                case "ShinyShoe.Ares.SharedSOs.UnitStatNodes.GetUnitHasStatusNode":
                    return false;
                case "ShinyShoe.Ares.SharedSOs.ConditionalNodes.IntCompareNode":
                    {
                        int in1 = ResolveIntNode(ctx, GetSource(port, "value1"));
                        int in2 = ResolveIntNode(ctx, GetSource(port, "value2"));
                        switch ((IntCompareNode.IntComparator)port.Node.Data["comparator"].Value)
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
        private static string ResolveStatNode(ResolveContext ctx, MinedPort? port)
        {
            if (port == null)
                return "_blank_";

            MinedAsset statData;
            switch (port.Node.ShinyTypeFull)
            {
                case "ShinyShoe.Ares.SharedSOs.ConstantNodes.StatDataConstantNode":
                    statData = port.Node.Data.Deref("value");
                    break;
                case "ShinyShoe.Ares.SharedSOs.SetupNodes.ActionNodeActionParams":
                    statData = port.Name switch
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
        private static MinedPort? GetSource(MinedNode node, string inputName)
        {
            var input = node.Ports[inputName];
            if (input.Links.Count == 0)
                return null;

            var source = input.Links[0].Node;
            return source.Ports[input.Links[0].Target];
        }

        private static MinedPort? GetSource(MinedPort nodeOwner, string inputName) => GetSource(nodeOwner.Node, inputName);


        public record class ResolveContext(MinedAsset ActionData);
        //public record class Connection(MinedAsset? Node, string PortName);

    }
}

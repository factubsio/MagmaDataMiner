// See https://aka.ms/new-console-template for more information
using ShinyShoe;
using ShinyShoe.Ares;
using ShinyShoe.SharedDataLoader;
using System.Reflection;
using System.Text;

using MagmaDataMiner;

Console.WriteLine("Hello, World!");

using var log = File.Open(@"D:\out.txt", FileMode.Truncate);
using var output = new StreamWriter(log);
output.NewLine = "\n";

const string path = @"C:\Program Files (x86)\Steam\steamapps\common\Inkbound\Inkbound_Data\StreamingAssets\SharedScriptableObjects\SharedScriptableObjects.fb";


Console.WriteLine("manifest loaded");


Console.WriteLine("class type set"); ;

output.WriteLine("TYPES:");
foreach (var key in MineDb.ByType.OrderBy(k => k.Key))
{
    output.WriteLine($"{key.Key}");
    foreach (var e in key.Value.OrderBy(e => e.name))
    {
        output.WriteLine($"    {e.assetID._guid}   {e.dataId}    {key.Key}: {e.name}");
    }
}
output.WriteLine("");
output.WriteLine("");

output.Flush();

MineDb.Init(path);

MineDb.LoadAll("ActionData");
MineDb.LoadAll("EquipmentData");
MineDb.LoadAll("AbilityUpgradeData");
MineDb.LoadAll("CharacterClassData");
MineDb.LoadAll("StatusEffectData");
MineDb.LoadAll("HelperData");
MineDb.LoadAll("AbilityData");
MineDb.LoadAll("AbilityListData");

const string draftableAssetName = "ClassesAndLoot/AbilityDrafts/DraftLists/NonClassAbilities_AbilityList";

var draftableAbilities = MineDb.Lookup(draftableAssetName).EnumerateAssetLinks("abilities").ToList();
var charClasses = MineDb.AssetsByType("CharacterClassData").ToList();

AbilitiesModel model = new();
foreach (var charClass in charClasses)
{
    AbilitySource source = new();
    source.Source = charClass["className"].String;
    foreach (var abilityData in charClass.EnumerateAssetLinks("starterAbilities"))
    {
        source.AddAbility(abilityData);
    }
    model.Sources.Add(source);
}

AbilitySource draftable = new()
{
    Source = "Draftable"
};
foreach (var abilityData in draftableAbilities.OrderBy(d => d["abilityName"].String))
    draftable.AddAbility(abilityData);
model.Sources.Add(draftable);


WebGen.Bob(model);


//foreach (var bob in MineDb.ByType["AbilityListData"])
//{
//    Console.WriteLine(bob.name);
//    foreach (var ability in MineDb.Get(bob)["abilities"].EnumerateAssetLinks())
//    {
//        Console.WriteLine(ability.AssetName);
//    }
//}

//Console.WriteLine(bonk["abilityName"].String);
//Console.WriteLine("==========");
//Console.WriteLine();
//foreach (var augment in bonk["mainUpgrades"].EnumerateAssetLinks())
//{
//    var effect = augment.Deref("statusEffect");
//    var help = effect.Deref("helperData");

//    PrintAsset(effect, output);
//    PrintAsset(help, output);

//    Console.WriteLine($" * {help["titleKey"].String}");
//    Console.WriteLine("   -------");
//    Console.WriteLine($"     > {augment["rarity"].Value}");
//    Console.WriteLine($"     > duplicates: {augment["allowDuplicates"].Value}");
//    var raw = (help["descriptionKey"].String.Clone() as string)!;
//    Loccer.ApplyLocalizationParams(ref raw, param =>
//    {
//        if (Loccer.TryGetLocalizationParameterActionParamAsInt(param, out var value))
//            return value.ToString();
//        else if (Loccer.TryGetLocalizationParameterStatusEffectProcChance(param, out var value2))
//            return value2.ToString();
//        else
//            return param;

//    });
//    foreach (var line in raw.Split("\n", StringSplitOptions.RemoveEmptyEntries))
//        Console.WriteLine($"     | {line}");

//    Console.WriteLine();
//}

void Indent(int level, TextWriter output)
{
    for (int i = 0; i < level; i++)
    {
        output.Write("  ");
    }
}


int PrintAsset(MinedAsset asset, TextWriter output)
{
    int level = 0;

    foreach (var x in asset.Iterate)
    {
        if (x.levelDelta > 0)
        {
            Indent(level, output);
            char ch = x.isObj ? '{' : '[';
            string end = "";
            if (x.Count == 0)
                end = x.isObj ? "}" : "]";
            output.WriteLine($"{x.key}: {ch}{end}");
            level++;
        }
        else if (x.levelDelta < 0)
        {
            level--;
            if (x.Count > 0)
            {
                Indent(level, output);
                output.WriteLine($"{(x.isObj ? '}' : ']')}");
            }
        }
        else
        {
            Indent(level, output);
            output.WriteLine($"{x.key}: {x.value}");
        }
    }

    return level;
}

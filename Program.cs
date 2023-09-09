// See https://aka.ms/new-console-template for more information
using ShinyShoe;
using ShinyShoe.Ares;
using ShinyShoe.SharedDataLoader;
using System.Reflection;
using System.Text;

using MagmaDataMiner;
using Microsoft.AspNetCore.Server.Kestrel.Core;

Console.WriteLine("Hello, World!");

using var log = File.Open(@"D:\out.txt", FileMode.Truncate);
using var output = new StreamWriter(log);
output.NewLine = "\n";

const string path = @"C:\Program Files (x86)\Steam\steamapps\common\Inkbound\Inkbound_Data\StreamingAssets\SharedScriptableObjects\SharedScriptableObjects.fb";

const bool web = true;

Console.WriteLine("manifest loaded");


Console.WriteLine("class type set"); ;

MineDb.Init(path);

MineDb.LoadAll();
//MineDb.LoadAll("ActionData");
//MineDb.LoadAll("EquipmentData");
//MineDb.LoadAll("AbilityUpgradeData");
//MineDb.LoadAll("CharacterClassData");
//MineDb.LoadAll("StatusEffectData");
//MineDb.LoadAll("HelperData");
//MineDb.LoadAll("AbilityData");
//MineDb.LoadAll("StageMutatorData");

//MineDb.LoadAll("AbilityListData");
//MineDb.LoadAll("LootListData");

//MineDb.LoadAll("Chunk");
//MineDb.LoadAll("ChunkSet");
//MineDb.LoadAll("SocketContent");
//MineDb.LoadAll("SocketContentSet");
//MineDb.LoadAll("Stage");
//MineDb.LoadAll("UnitData");
//MineDb.LoadAll("AIBehaviorData");
//MineDb.LoadAll("StageMutatorData");

output.WriteLine("TYPES:");
foreach (var key in MineDb.ByType.OrderBy(k => k.Key))
{
    output.WriteLine($"{key.Key}");
}
output.WriteLine("");
output.WriteLine("");
output.WriteLine("TYPES:");
foreach (var key in MineDb.ByType.OrderBy(k => k.Key))
{
    output.WriteLine($"{key.Key}");
    foreach (var e in key.Value.OrderBy(e => e.name))
    {
        output.WriteLine($"    {e.assetID._guid}   {e.dataId}    {key.Key}: {e.name}");
    }
}

output.Flush();

var chunkContentSet = MineDb.Action("d7B011gC");


    //foreach (var stage in MineDb.AssetsByType("Stage"))
    //{
    //    PrintAsset(stage, output);
    //    output.WriteLine("");
    //    output.WriteLine("");

    //    var spawnSet = stage.Deref("_spawnChunkSet");
    //    PrintAsset(spawnSet, output);

    //    var spawnChunk = spawnSet["_chunks"].At(0).Deref();
    //    PrintAsset(spawnChunk, output);

    //    output.WriteLine("");
    //    output.WriteLine("");
    //    output.WriteLine("");
    //    output.WriteLine("");
    //    output.WriteLine("");
    //}


    const string draftableAssetName = "ClassesAndLoot/AbilityDrafts/DraftLists/NonClassAbilities_AbilityList";

var draftableAbilities = MineDb.Lookup(draftableAssetName).EnumerateAssetLinks("abilities");
var charClasses = MineDb.AssetsByType("CharacterClassData");

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

var vestiges = new EquipmentCategory("Vestiges", new());
var consumables = new EquipmentCategory("Consumables", new());

model.Equipment.Add(vestiges);
model.Equipment.Add(consumables);

foreach (var vestige in MineDb.AssetsByType("EquipmentData").OrderBy(x => x["rarity"].Value).ThenBy(x => x["equipmentName"].String))
{
    if (vestige["description"].IsNull)
    {
        continue;
    }


    var eType = (EquipmentType)vestige["equipmentType"].Value;

    EquipmentCategory? type = eType switch
    {
        EquipmentType.Accessory => vestiges,
        EquipmentType.WorldItem => consumables,
        _ => null,
    };

    if (type == null)
    {
        continue;
    }


    type.All.Add(new(
        vestige["equipmentName"].String,
        vestige["description"].Localized(),
        (RarityType)vestige["rarity"].Value,
        vestige["equipmentType"].Value.ToString()!));
}

HashSet<string> brainsDone = new();

foreach (var unitData in MineDb.AssetsByType("UnitData"))
{
    var ai = unitData.Deref("aiBehaviorData");
    if (!brainsDone.Add(ai.AssetName)) { continue; }

    Dictionary<string, AiState> states = new();
    Enemy unit = new(unitData["unitName"].String, new(), new("on_spawn", new(), new()));
    model.Enemies.Add(unit);

    output.WriteLine(">>>" + unit.Name);
    //PrintAsset(ai, output);


    unit.Phases.Add(new("default", new()));

    AddToPhase(unit.Phases[0], ai["aiStates"]["entries"]);

    var spawnBlocks = ai["aiOnSpawnEvaluationBlocks"]["entries"];
    if (spawnBlocks.Length > 0)
    {
        foreach (var blockData in spawnBlocks.Enumerate())
        {
            var block = ParseBlock(blockData);
            unit.SpawnAction.Blocks.Add(block);
        }
    }

    AiActionBlock ParseBlock(MinedField blockData)
    {
        AiActionBlock block = new(new());
        foreach (var aiAction in blockData["actions"].Enumerate())
        {
            var type = (AIActionType)aiAction["actionType"].Value;
            string name;
            MinedAsset? abilityData = null;
            MinedAsset? moveData = null;
            AiStateTransition? targetState = null;
            string description = "";

            if (type == AIActionType.Ability)
            {
                abilityData = aiAction["aiAbilityData"].Deref("abilityData");
                name = abilityData["abilityName"].String;
                if (string.IsNullOrEmpty(name))
                {
                    name = Path.GetFileName(abilityData.AssetName);
                }
            }
            else if (type == AIActionType.Movement)
            {
                moveData = aiAction["aiMovementData"].Asset;
                name = "Movement";
            }
            else
            {
                name = "_unknown_";
            }

            if (aiAction.Has("stateTransition"))
            {
                var target = aiAction["stateTransition"]["transitionToStateName"].String;
                if (!string.IsNullOrEmpty(target))
                {
                    targetState = new(states[target], "");
                }
            }

            if (abilityData is not null)
            {
                description = abilityData["description"].Localized();
            }

            AiAction action = new(name, description, targetState);
            //output.WriteLine($"adding action ({name})");
            block.Candidates.Add(action);
        }
        return block;
    }

    void AddToPhase(AiPhase phase, MinedField stateList)
    {
        foreach (var aiState in stateList.Enumerate())
        {
            AiState state = new(aiState["name"].String, new(), new());
            states[state.Name] = state;
            phase.States.Add(state);
        }

        foreach (var aiState in stateList.Enumerate())
        {
            AiState state = states[aiState["name"].String];

            foreach (var blockData in aiState["evaluationBlocks"]["entries"].Enumerate())
            {
                state.Blocks.Add(ParseBlock(blockData));
            }
        }

    }

}

var baneList = MineDb.AssetsByType("StageMutatorData");

foreach (var baneData in baneList)
{
    if (baneData["isHiddenFromUI"].Bool) { continue; }

    var helper = baneData.Deref("helperData");

    output.WriteLine(baneData.AssetName);
    output.WriteLine(helper["titleKey"].String);
    output.WriteLine(helper["descriptionKey"].String);
    PrintAsset(baneData, output);

    int gold = baneData["runCurrencyGainedOnSelection"].Int;
    string currencyType = "kwillings";
    if (gold == 0)
    {
        gold = baneData["glyphsGainedOnSelection"].Int;
        currencyType = "glyphs";
    }

    Bane bane = new(
            helper["titleKey"].Localized(),
            helper["descriptionKey"].Localized(),
            gold,
            currencyType,
            baneData.AssetName.StartsWith("DailyChallenge"));
    model.Banes.Add(bane);

}

if (web)
{
    WebGen.Bob(model);
}


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

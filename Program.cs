// See https://aka.ms/new-console-template for more information
using ShinyShoe;
using ShinyShoe.Ares;
using ShinyShoe.SharedDataLoader;
using System.Reflection;
using System.Text;

using MagmaDataMiner;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ShinyShoe.Ares.ConstantNodes;
using BubbleAssets.Assets;

Console.WriteLine("Hello, World!");


Dictionary<long, MinedAsset> nodes = new();

using var log = File.Open(@"D:\out.txt", FileMode.Truncate);
using var output = new StreamWriter(log);
output.NewLine = "\n";

const string path = @"C:\Program Files (x86)\Steam\steamapps\common\Inkbound\Inkbound_Data\StreamingAssets\SharedScriptableObjects\SharedScriptableObjects.fb";

const bool web = true;

Catalog catalog = Catalog.FromJson(@"C:\Program Files (x86)\Steam\steamapps\common\Inkbound\Inkbound_Data\StreamingAssets\aa\catalog.json");
MineDb.ResourceMap = catalog.CreateLocator();


MineDb.Init(path);

MineDb.LoadAll();

Loccer.Init();

var global = MineDb.AssetsByType("GlobalGameData").First();

foreach (var seasonData in MineDb.AssetsByType("SeasonData"))
{
    var name = seasonData["seasonName"].Value;
    Console.WriteLine(name);
}


//output.WriteLine("TYPES:");
//foreach (var key in MineDb.ByType.OrderBy(k => k.Key))
//{
//    output.WriteLine($"{key.Key}");
//}
//output.WriteLine("");
//output.WriteLine("");
//output.WriteLine("TYPES:");
//foreach (var key in MineDb.ByType.OrderBy(k => k.Key))
//{
//    output.WriteLine($"{key.Key}");
//    foreach (var e in key.Value.OrderBy(e => e.name))
//    {
//        output.WriteLine($"    {e.assetID._guid}   {e.dataId}    {key.Key}: {e.name}");
//    }
//}

//output.Flush();



var graphs = MineDb.AssetsByType("ActionGraph");



using (MineDb.ActivateLanguage("en-US"))
{
    AbilitiesModel model = GenerateModel(MineDb.DraftableBindings, MineDb.CharacterClasses);

    if (web)
    {
        WebGen.Bob(model);
    }
}

static void Indent(int level, TextWriter output)
{
    for (int i = 0; i < level; i++)
    {
        output.Write("  ");
    }
}


static int PrintAsset(MinedAsset asset, TextWriter output)
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

AbilitiesModel GenerateModel(IEnumerable<MinedAsset> draftableAbilities, IEnumerable<MinedAsset> charClasses)
{
    AbilitiesModel model = new();
    foreach (var charClass in charClasses)
    {
        PrintAsset(charClass, output);
        if ((CharacterClassType)charClass["classType"].Value == CharacterClassType.C06_StarterClass)
        {
            continue;
        }

        AbilitySource source = new();
        source.Source = charClass["className"].Translated();
        foreach (var abilityData in charClass.EnumerateAssetLinks("starterAbilities"))
        {
            source.AddAbility(abilityData, model.SearchIndex);
        }
        model.Sources.Add(source);
    }

    AbilitySource draftable = new()
    {
        Source = "Draftable"
    };
    foreach (var abilityData in draftableAbilities.OrderBy(d => d["abilityName"].String))
    {
        draftable.AddAbility(abilityData, model.SearchIndex);
    }
    model.Sources.Add(draftable);

    var vestiges = new EquipmentCategory("Vestiges", new());
    var consumables = new EquipmentCategory("Consumables", new());
    var trinkets = new EquipmentCategory("Trinkets", new());

    HashSet<string> UsedSets = new();

    foreach (var setData in MineDb.AssetsByType("VestigeSetData").OrderBy(x => x["m_Name"].String))
    {
        var helper = setData.Deref("helperData");

        List<VestigeSetRank> ranks = new();

        foreach (var rankData in setData["setBonusTiers"].Enumerate())
        {
            var rankHelper = rankData["helperData"].At(0).Deref();
            ranks.Add(new(
                rankData["statThreshold"].Int,
                rankHelper["descriptionKey"].Localized()
            ));
        }

        VestigeSet set = new(
            helper["titleKey"].Translated(),
            ranks);

        model.Sets.Add(setData.AssetGuid, set);
    }


    model.Equipment.Add(vestiges);
    model.Equipment.Add(consumables);
    model.Equipment.Add(trinkets);

    foreach (var trinket in MineDb.AssetsByType("TrinketData").OrderBy(x => x["trinketName"].String))
    {

        var icon = MineDb.Base64Icon(trinket["assetAddressIcon64"].Asset);

        Equipment trinketModel = new(
            trinket["trinketName"].Translated(),
            trinket["description"].Localized(),
            RarityType.Common,
            new(),
            null,
            icon,
            trinket.DataId);
        trinkets.All.Add(trinketModel);
        model.SearchIndex.Add(new(trinketModel.Name, "trinket", "equip", "Trinkets", trinketModel.Hash));
    }

    foreach (var vestige in MineDb.AssetsByType("EquipmentData").OrderBy(x => x["rarity"].Value).ThenBy(x => x["equipmentName"].String))
    {

        var eType = (EquipmentType)vestige["equipmentType"].Value;

        List<string>? sets = null;

        EquipmentCategory? equipCat = eType switch
        {
            EquipmentType.Accessory => vestiges,
            EquipmentType.WorldItem => consumables,
            _ => null,
        };


        if (equipCat == null)
        {
            continue;
        }

        List<StatGain> stats = new();

        if (eType == EquipmentType.Accessory)
        {
            sets = new();

            foreach (var x in vestige["vestigeSetDatas"].Enumerate())
            {
                var setGuid = x["guid"].String;
                UsedSets.Add(setGuid);
                sets.Add(setGuid);
            }

            foreach (var x in vestige["statEntries"]["statEntries"].Enumerate())
            {
                var stat = x.Deref("entryStatData");
                if (stat["statName"].IsNull || stat["replacementString"].IsNull || stat["replacementString"].IsEmptyLocString)
                {
                    continue;
                }
                stats.Add(new(x["entryValue"].Int, stat["statName"].Translated()));
            }
        }

        Equipment equipModel = new(
            vestige["equipmentName"].Translated(),
            vestige["description"].Localized(),
            (RarityType)vestige["rarity"].Value,
            stats,
            sets,
            MineDb.Base64Icon(vestige["assetAddressIcon"].Asset),
            vestige.DataId);
        equipCat.All.Add(equipModel);
        model.SearchIndex.Add(new(equipModel.Name, eType == EquipmentType.Accessory ? "vestige" : "consumable", "equip", equipCat.Name, equipModel.Hash));
    }

    var toRemove = model.Sets.Keys.Where(x => !UsedSets.Contains(x)).ToArray();
    foreach (var remove in toRemove)
    {
        model.Sets.Remove(remove);
    }

    HashSet<string> brainsDone = new();

    var baneList = MineDb.AssetsByType("StageMutatorData");

    foreach (var baneData in baneList)
    {
        if (baneData["isHiddenFromUI"].Bool) { continue; }

        var helper = baneData.Deref("helperData");

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
                baneData.AssetName.StartsWith("DailyChallenge"),
                baneData.DataId);
        model.Banes.Add(bane);
        model.SearchIndex.Add(new(bane.Name, "misc", "other", bane.IsRunMutator ? "run-mutators" : "banes", bane.Hash));
    }

    return model;
}


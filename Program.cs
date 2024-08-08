// See https://aka.ms/new-console-template for more information
using ShinyShoe;
using ShinyShoe.Ares;
using ShinyShoe.SharedDataLoader;
using System.Reflection;
using System.Text;

using MagmaDataMiner;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ShinyShoe.Ares.ConstantNodes;
using System.Drawing;
using BubbleAssets.Assets;

Console.WriteLine("Hello, World!");


Dictionary<long, MinedAsset> nodes = new();

using var log = File.Open(@"D:\out.txt", FileMode.Truncate);
using var output = new StreamWriter(log);
output.NewLine = "\n";

const string path = @"C:\Program Files (x86)\Steam\steamapps\common\Inkbound\Inkbound_Data\StreamingAssets\SharedScriptableObjects\SharedScriptableObjects.fb";

Catalog catalog = Catalog.FromJson(@"C:\Program Files (x86)\Steam\steamapps\common\Inkbound\Inkbound_Data\StreamingAssets\aa\catalog.json");
MineDb.ResourceMap = catalog.CreateLocator();

List<(string, Image)> trinketImages = new();
List<(string, Image)> vestigeImages = new();
List<(string, Image)> potImages = new();
List<(string, Image)> setImages = new();


MineDb.Init(path);

MineDb.LoadAll();

Loccer.Init();

var global = MineDb.AssetsByType("GlobalGameData").First();


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

    if (MineDb.generateWeb)
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
        //PrintAsset(charClass, output);
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

        var name = helper["titleKey"].Translated();

        var icon = MineDb.DecodeImage(helper["assetAddressSmall"].Asset);
        if (icon != null)
        {
            setImages.Add((name, icon));
        }

        VestigeSet set = new(
            name,
            ranks);

        model.Sets.Add(setData.AssetGuid, set);
    }


    model.Equipment.Add(vestiges);
    model.Equipment.Add(consumables);
    model.Equipment.Add(trinkets);

    List<(string, string, Image?)> equipIcons = new();

    foreach (var trinket in MineDb.AssetsByType("TrinketData").OrderBy(x => x["trinketName"].String))
    {
        var imageAsset = trinket["assetAddressIcon64"].Asset;
        var iconString = MineDb.Base64Icon(imageAsset);

        Console.WriteLine(trinket["trinketName"].Translated());
        var icon256 = MineDb.DecodeImage(trinket["assetAddressIcon256"].Asset, 256);
        Console.WriteLine("DONE");
        MineDb.TraceImageDecode = false;

        PrintAsset(trinket, output);

        Equipment trinketModel = new(
            trinket["trinketName"].Translated(),
            trinket["description"].Localized(),
            RarityType.Common,
            new(),
            null,
            iconString,
            trinket.DataId);
        trinkets.All.Add(trinketModel);
        model.SearchIndex.Add(new(trinketModel.Name, "trinket", "equip", "Trinkets", trinketModel.Hash));

        equipIcons.Add(("trinkets", trinketModel.Name, icon256));
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
        List<(string, Image)> imageList;

        var name = vestige["equipmentName"].Translated();
        string catName = "";

        if (eType == EquipmentType.Accessory)
        {
            imageList = vestigeImages;
            catName = "vestiges";
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
        else
        {
            catName = "consumables";
            imageList = potImages;
        }

        if (MineDb.generateSpriteSheet)
        {
            var icon = MineDb.DecodeImage(vestige["assetAddressIconLarge"].Asset);
            if (icon != null)
            {
                imageList.Add((name, icon));
                equipIcons.Add((catName, name, icon));
            }
        }


        Equipment equipModel = new(
            name,
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

    if (MineDb.generateSpriteSheet)
    {
        //Directory.Delete(@"D:\inkbound_icons", true);
        Directory.CreateDirectory(@"D:\inkbound_icons\bindings");
        //Directory.CreateDirectory(@"D:\inkbound_icons\vestiges");
        Directory.CreateDirectory(@"D:\inkbound_icons\equipment");
        Directory.CreateDirectory(@"D:\inkbound_icons\equipment\vestiges");
        Directory.CreateDirectory(@"D:\inkbound_icons\equipment\trinkets");
        Directory.CreateDirectory(@"D:\inkbound_icons\equipment\consumables");
        Directory.CreateDirectory(@"D:\inkbound_icons\sets");

        using Font font = new(FontFamily.GenericSansSerif, 24);
        StringFormat titleformat = new()
        {
            Alignment = StringAlignment.Center,
        };

        foreach (var source in model.Sources)
        {
            //DrawSpriteSheet(font, titleformat, "bindings", source.Source, source.AbilityIcons, 4, 4, 256);
            Directory.CreateDirectory($@"D:\inkbound_icons\bindings\{source.Source}");
            foreach (var (name, sprite) in source.AbilityIcons)
            {
                var path = $@"d:\inkbound_icons\bindings\{source.Source}\{name}.png";
                sprite.Save(path);
            }
        }

        //DrawSpriteSheet(font, titleformat, "vestiges", "vestiges", vestigeImages, 8, 8, 256);
        //DrawSpriteSheet(font, titleformat, "sets", "sets", setImages, 4, 16, 64, 256);
        //foreach (var (name, sprite) in vestigeImages)
        //{
        //    var path = $@"d:\inkbound_icons\vestiges\{name}.png";
        //    sprite.Save(path);
        //}
        foreach (var (name, sprite) in setImages)
        {
            var path = $@"d:\inkbound_icons\sets\{name}.png";
            sprite.Save(path);
        }

        foreach (var (cat, name, icon) in equipIcons)
        {
            var path = $@"d:\inkbound_icons\equipment\{cat}\{name}.png";
            icon?.Save(path);
        }
    }

    return model;
}

static void DrawSpriteSheet(Font font,
                            StringFormat titleformat,
                            string folder,
                            string title,
                            List<(string, Image)> images,
                            int cols,
                            int maxRows,
                            int iconSize,
                            int extraHorizontalPadding = 0)
{
    const int padding = 40;
    const int textHeight = 90;
    Size iconArea = new(iconSize + padding + extraHorizontalPadding, iconSize + padding + textHeight);
    Rectangle rect = new(0, 0, iconSize, iconSize);
    Rectangle textRect = new(0, 0, iconSize + extraHorizontalPadding, textHeight);

    // 4 rows of 4 icons (base ability + 3x ascensions)
    int iconsPerSheet = cols * maxRows;
    int sheetCount = (images.Count + iconsPerSheet - 1) / iconsPerSheet;
    for (int sheetIndex = 0; sheetIndex < sheetCount; sheetIndex++)
    {
        using Bitmap sheet = new(iconArea.Width * cols, iconArea.Height * maxRows);
        using var g = Graphics.FromImage(sheet);
        //g.FillRectangle(Brushes.Black, 0, 0, sheet.Width, sheet.Height);
        for (int i = 0; i < iconsPerSheet; i++)
        {
            int iconIndex = (sheetIndex * iconsPerSheet) + i;
            if (iconIndex >= images.Count)
                break;


            var (name, icon) = images[iconIndex];

            rect.Location = new((i % cols) * iconArea.Width, (i / cols) * iconArea.Height);
            rect.Offset(padding / 2 + extraHorizontalPadding / 2, padding / 2);

            textRect.Location = new((i % cols) * iconArea.Width, (i / cols) * iconArea.Height);
            textRect.Offset(padding / 2, padding / 2 + iconSize + 8);

            g.DrawImage(icon, rect);
            g.DrawString(name, font, Brushes.White, textRect, titleformat);

        }

        string sheetName = "";
        if (sheetCount > 1)
        {
            sheetName = $"_{sheetIndex + 1}";
        }
        var sheetPath = @$"D:\inkbound_icons\{folder}\{title.ToLower()}{sheetName}.png";
        if (File.Exists(sheetPath))
            File.Delete(sheetPath);
        sheet.Save(sheetPath);
    }
}
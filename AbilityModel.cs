using BubbleAssets;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ShinyShoe;
using ShinyShoe.Ares;
using ShinyShoe.SharedDataLoader;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagmaDataMiner
{
	public static class NonsenseExts
	{
		public static (bool Found, T Value) MaybeFirst<T>(this IEnumerable<T> input, Func<T, bool> predicate) where T : struct
		{
			foreach (var x in input)
				if (predicate(x))
					return (true, x);
			return (false, default(T));
		}
		public static string q(this string val) => '"' + val + '"';
		public static string Sanitized(this string val)
		{
			if (val == null)
			{
				return "";
			}
			else
			{
				return val.Replace("\"", "&quot").Trim();
			}
		}

	}
	public record class Augment(string Name, string Description, RarityType Rarity, bool Unique);
	public record class SimpleTargetInfo(RangeMode Mode, TeamAlignment Team, bool TargetRequired);
    public record class Ability(string Name,
								string Description,
								List<Ability> Ascensions,
								List<Augment> Augments,
								int Cost,
								int Cooldown,
								SimpleTargetInfo Target,
								string? IconBase64,
								string Hash,
								List<string> Tags)
	{

		public IEnumerable<string> TagNames => Tags.Select(LookupTagName);

		private static Dictionary<string, string> TagNameLookup = new()
		{
			{"[attackbinding]", "[Attack]" },
			{"[physicalactiontag]", "[Physical]" },
			{"[magicalactiontag]", "[Magic]" },
			{"[movementactiontag]", "[Movement]" },
			{"[basicbinding]", "[Basic]" },

		};
		private static string LookupTagName(string tag)
		{
			if (TagNameLookup.TryGetValue(tag, out var val))
			{
				return val;
			}
			return tag;
		}

	}
    public record class EquipmentCategory(string Name, List<Equipment> All);

    public record class StatGain(int Value, string Name)
	{
		public string Icon
		{
			get
			{
				if (Name == null)
					return "null";
				else
					return Name.ToLower().Replace(" ", "-");
			}
		}
	}

	public record class VestigeSetRank(int Count, string Description);
	public record class VestigeSet(string Name, List<VestigeSetRank> Ranks);
	public record class Equipment(string Name,
							   string Description,
							   RarityType Rarity,
							   List<StatGain> Stats,
							   List<string>? Sets,
							   string? Icon,
							   string Hash)
	{
		public string SetList
		{
			get
			{
				if (Sets != null)
					return "[" + string.Join(',', Sets.Select(x => '"' + x + '"')) + "]";
				else
					return "_empty_";
            }
        }
	}

	public record class AiStateTransition(AiState Target, string Condition);
	public record class AiAction(string Name, string Description, AiStateTransition? Transition);
	public record class AiActionBlock(List<AiAction> Candidates);
	public record class AiState(string Name, List<AiActionBlock> Blocks, List<AiStateTransition> Transitions)
	{
		public bool IsEmpty => Blocks.Count == 0;
	}

	public record class AiPhase(string Name, List<AiState> States);

	public record class Enemy(string Name, List<AiPhase> Phases, AiState SpawnAction);

	public record class Bane(string Name, string Description, int CurrencyGain, string CurrencyType, bool IsRunMutator, string Hash)
	{
		public string Reward => $"{CurrencyGain} {CurrencyType}";
	};

	public class AbilitySource
	{
		public readonly List<(string, Image)> AbilityIcons = new();

		public List<Ability> Abilities = new();
		public string Source = "";

		public const string TmpIconPath = @"D:\ability_icon_tmp.png";

		internal Ability MakeAbility(MinedAsset abilityData, List<SearchKey> index, bool isAscension)
		{
			var baseDamage = AresSimulator.ProcessAbilityGraph(abilityData);
			Loccer.Current = new(abilityData, baseDamage);

			var cost = abilityData["resourceCosts"].At(0)["amount"].Int;
			var cooldown = abilityData["cooldownTurnCount"].Int;

			var name = abilityData["abilityName"].Translated();
			string? base64icon = MineDb.Base64Icon(abilityData["assetAddressArt"].Asset);

			if (MineDb.generateSpriteSheet)
			{
				var img = MineDb.DecodeImage(abilityData["assetAddressArtLarge"].Asset);
				if (img != null)
				{
					AbilityIcons.Add((name, img));
				}
			}


            var targetType = (RangeMode)abilityData["targetInfo"]["rangeMode"].Value;
			var targetAlignment = (TeamAlignment)abilityData["targetInfo"]["teamAlignment"].Value;
			var requiresTarget = abilityData["targetInfo"]["requiresTarget"].Bool;

			SimpleTargetInfo target = new(targetType, targetAlignment, requiresTarget);

			List<string> tags = new();
			foreach (var tag in abilityData["actionTags"].EnumerateAssetLinks())
			{
				var tagHelper = tag.Deref("helperData");
				var tagName = tagHelper["titleKey"].Translated();
				if (tagName.Length > 0)
				{
					tags.Add(tagName);
				}
			}


			Ability ability = new(name, abilityData["description"].Localized(), new(), new(), cost, cooldown, target, base64icon, abilityData.DataId, tags);

            index.Add(new(ability.Name, isAscension ? "ascension" : "binding", "bindings", Source, ability.Hash));

            return ability;

		}

		internal void AddAbility(MinedAsset abilityData, List<SearchKey> index)
		{
			var name = abilityData["abilityName"].Translated();
			var ability = MakeAbility(abilityData, index, false);
			foreach (var upgrade in abilityData.EnumerateAssetLinks("mainUpgrades"))
			{
                var effect = upgrade.Deref("statusEffect");
                var help = effect.Deref("helperData");

                Augment augment = new(
					help["titleKey"].Translated(),
					help["descriptionKey"].Localized(),
					(RarityType)upgrade["rarity"].Value,
					!upgrade["allowDuplicates"].Bool);
                ability.Augments.Add(augment);
            }

			foreach (var ascensionData in abilityData.EnumerateAssetLinks("ascensions"))
			{
				var ascension = MakeAbility(ascensionData, index, true);

                ability.Ascensions.Add(ascension);

				Loccer.Current = null;

            }

            ability.Augments.Sort((a, b) => a.Rarity.CompareTo(b.Rarity));
            Abilities.Add(ability);
        }
    }

	public class AbilitiesModel
	{
		public List<AbilitySource> Sources = new();

		public List<EquipmentCategory> Equipment = new();

		public List<Enemy> Enemies = new();

		public List<Bane> Banes = new();

		public List<SearchKey> SearchIndex = new();

		public Dictionary<string, VestigeSet> Sets = new();
	}

	public record struct SearchKey(string Key, string Tag, string Page, string Sub, string Hash)
	{
		public string Render()
		{
			return $"{{ key: {Key.q()}, tag: {Tag.q()}, page: {Page.q()}, sub: {Sub.q()}, hash: {Hash.q()} }},\n";
		}
	}
}

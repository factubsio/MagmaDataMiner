using ShinyShoe.Ares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagmaDataMiner
{
	public record class Augment(string Name, string Description, RarityType Rarity, bool Unique);
	public record class Ability(string Name, string Description, List<Ability> Ascensions, List<Augment> Augments);
	public record class Vestige(string Name, string Description, RarityType Rarity);

	public class AbilitySource
	{
		public List<Ability> Abilities = new();
		public string Source = "";

		internal void AddAbility(MinedAsset abilityData)
		{
            Ability ability = new(abilityData["abilityName"].String, abilityData["description"].Localized(), new(), new());
			foreach (var upgrade in abilityData.EnumerateAssetLinks("mainUpgrades"))
			{
                var effect = upgrade.Deref("statusEffect");
                var help = effect.Deref("helperData");

                Augment augment = new(
					help["titleKey"].String,
					help["descriptionKey"].Localized(),
					(RarityType)upgrade["rarity"].Value,
					!upgrade["allowDuplicates"].Bool);
                ability.Augments.Add(augment);
            }

			foreach (var ascension in abilityData.EnumerateAssetLinks("ascensions"))
				ability.Ascensions.Add(new(ascension["abilityName"].String, ascension["description"].Localized(), new(), new()));

			ability.Augments.Sort((a, b) => a.Rarity.CompareTo(b.Rarity));
            Abilities.Add(ability);
        }
    }

	public class AbilitiesModel
	{
		public List<AbilitySource> Sources = new();

		public List<Vestige> Vestiges = new();
	}
}

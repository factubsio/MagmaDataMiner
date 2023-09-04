using ShinyShoe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagmaDataMiner
{
    internal class Loccer
    {
		private static int _applyParamsDepth = 0;
        public static void ApplyLocalizationParams(ref string translation, Func<string, object> getParam, bool allowLocalizedParameters = true)
		{
			if (translation == null)
			{
				return;
			}
			if (_applyParamsDepth >= 6)
			{
				return;
			}
			_applyParamsDepth++;
			LocalizationUtil.ApplyTranslationReplacements(ref translation);
			int length = translation.Length;
			int num = 0;
			while (num >= 0 && num < translation.Length)
			{
				int num2 = translation.IndexOf("{[", num);
				if (num2 < 0)
				{
					break;
				}
				int num3 = translation.IndexOf("]}", num2);
				if (num3 < 0)
				{
					break;
				}
				int num4 = translation.IndexOf("{[", num2 + 1);
				if (num4 > 0 && num4 < num3)
				{
					num = num4;
				}
				else
				{
					int num5 = (translation[num2 + 2] == '#') ? 3 : 2;
					string param = translation.Substring(num2 + num5, num3 - num2 - num5);
					string text = (string)getParam(param);
					if (text != null && allowLocalizedParameters)
					{
						string oldValue = translation.Substring(num2, num3 - num2 + 2);
						translation = translation.Replace(oldValue, text);
						num = num2 + text.Length;
					}
					else
					{
						num = num3 + 2;
					}
				}
			}
			_applyParamsDepth--;
		}
		public static bool TryGetLocalizationParameterActionParamAsString(string param, out string value)
		{
			if (TryParseLocalizationParameterActionParam(param, out var dataId, out var paramName) && MineDb.TryGetByData(dataId, out var action))
			{
				return TryGetActionDataStringOrIntParam(action, paramName, out value);
			}
			value = "";
			return false;
		}

		public static bool TryGetLocalizationParameterActionParamAsInt(string param, out int value)
		{
			if (TryParseLocalizationParameterActionParam(param, out var dataId, out var paramName) && MineDb.TryGetByData(dataId, out var action))
			{
                return TryGetActionDataIntParam(action, paramName, out value);
            }

			value = 0;
			return false;
        }
		private static bool TryGetActionDataStringOrIntParam(MinedAsset action, string paramName, out string value)
		{
			if (TryGetActionDataStringParam(action, paramName, out value))
				return true;

			if (TryGetActionDataIntParam(action, paramName, out int valInt))
			{
				value = valInt.ToString();
				return true;
			}

			value = "";
			return false;
		}

        private static bool TryParseLocalizationParameterActionParam(string param, out string actionDataId, out string paramName)
        {
            string empty;
            paramName = (empty = string.Empty);
            actionDataId = empty;
            if (!param.StartsWith("action", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            string[] array = param.Split('.', StringSplitOptions.None);
            if (array.Length != 3 || !array[0].Equals("action", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            actionDataId = array[1];
            paramName = array[2];
            return true;
        }
		private static bool TryGetActionDataIntParam(MinedAsset action, string paramName, out int value)
		{
			value = 0;
			if (paramName.Equals("paramInt", StringComparison.InvariantCultureIgnoreCase))
			{
				value = action["paramInt"].Int;
				return true;
			}
			if (paramName.Equals("paramInt2", StringComparison.InvariantCultureIgnoreCase))
			{
				value = action["paramInt2"].Int;
				return true;
			}
			if (paramName.Equals("paramPercent1", StringComparison.InvariantCultureIgnoreCase))
			{
				value = action["paramPercent1"].Int;
				return true;
			}
			if (paramName.Equals("paramPercent2", StringComparison.InvariantCultureIgnoreCase))
			{
				value = action["paramPercent2"].Int;
				return true;
			}
			return false;
		}


		private static bool TryGetActionDataStringParam(MinedAsset action, string paramName, out string value)
		{
			value = string.Empty;
			if (paramName.Equals("paramString", StringComparison.InvariantCultureIgnoreCase))
			{
				value = action["paramString"].String;
				return true;
			}
			return false;
		}

		// Token: 0x06002113 RID: 8467 RVA: 0x00085850 File Offset: 0x00083A50
		//public static bool TryGetLocalizationParameterAbilityDamage(string param, EntityHandle playerUnitHandle, [Nullable(new byte[]
		//{
		//	0,
		//	1
		//})] Handle<AbilityInUse> abilityInUseHandle, DamagePreviewType damagePreviewType, WorldState.IReadonly worldStateRo, AssetLibrary assetLibrary, out int paramAmount)
		//{
		//	int num;
		//	return LocalizationContextHelper.TryGetLocalizationParameterAbilityDamage(param, playerUnitHandle, abilityInUseHandle, damagePreviewType, worldStateRo, assetLibrary, out num, out paramAmount);
		//}

		// Token: 0x06002114 RID: 8468 RVA: 0x00085870 File Offset: 0x00083A70
		//public static bool TryGetLocalizationParameterAbilityDamage(string param, EntityHandle playerUnitHandle, [Nullable(new byte[]
		//{
		//	0,
		//	1
		//})] Handle<AbilityInUse> abilityInUseHandle, DamagePreviewType damagePreviewType, WorldState.IReadonly worldStateRo, AssetLibrary assetLibrary, out int baseDamageAmount, out int adjustedDamageAmount)
		//{
		//	bool result = false;
		//	baseDamageAmount = (adjustedDamageAmount = 0);
		//	string dataId;
		//	ShinyShoe.Ares.SharedSOs.AbilityData abilityData;
		//	if (LocalizationContextHelper.TryParseLocalizationParameterAbilityDamage(param, out dataId) && assetLibrary.TryGetData<ShinyShoe.Ares.SharedSOs.AbilityData>(dataId, out abilityData, true))
		//	{
		//		bool flag;
		//		AIIntentUISystemHelper.CalculatePredictedDamageFromAbility(playerUnitHandle, EntityHandle.InvalidEntity, abilityData, abilityInUseHandle, true, damagePreviewType, worldStateRo, assetLibrary, out baseDamageAmount, out adjustedDamageAmount, out flag);
		//		result = true;
		//	}
		//	return result;
		//}

		// Token: 0x06002115 RID: 8469 RVA: 0x000858BC File Offset: 0x00083ABC
		private static bool TryParseLocalizationParameterAbilityDamage(string param, out string abilityDataId)
		{
			abilityDataId = string.Empty;
			if (!param.StartsWith("ability", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			string[] array = param.Split('.', StringSplitOptions.None);
			if (array.Length != 3 || !array[0].Equals("ability", StringComparison.InvariantCultureIgnoreCase) || !array[2].Equals("damage", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			abilityDataId = array[1];
			return true;
		}

		// Token: 0x06002116 RID: 8470 RVA: 0x00085918 File Offset: 0x00083B18
		//public static bool TryGetLocalizationParameterStatusEffectActionDamage(string param, EntityHandle attachedUnitHandle, ShinyShoe.Ares.SharedSOs.StatusEffectData statusEffectData, WorldState.IReadonly worldStateRo, AssetLibrary assetLibrary, out int baseDamageAmount, out int adjustedDamageAmount)
		//{
		//	baseDamageAmount = 0;
		//	adjustedDamageAmount = 0;
		//	if (!param.StartsWith("action", StringComparison.InvariantCultureIgnoreCase))
		//	{
		//		return false;
		//	}
		//	string[] array = param.Split('.', StringSplitOptions.None);
		//	if (array.Length != 3 || !array[0].Equals("action", StringComparison.InvariantCultureIgnoreCase) || !array[2].Equals("damage", StringComparison.InvariantCultureIgnoreCase))
		//	{
		//		return false;
		//	}
		//	ShinyShoe.Ares.SharedSOs.ActionData actionData;
		//	if (assetLibrary.TryGetData<ShinyShoe.Ares.SharedSOs.ActionData>(array[1], out actionData, true))
		//	{
		//		bool flag;
		//		AIIntentUISystemHelper.CalculatePredictedDamageFromStatusEffectAction(attachedUnitHandle, statusEffectData, actionData, worldStateRo, assetLibrary, out baseDamageAmount, out adjustedDamageAmount, out flag);
		//		return true;
		//	}
		//	return false;
		//}

		// Token: 0x06002117 RID: 8471 RVA: 0x00085994 File Offset: 0x00083B94
		public static bool TryGetLocalizationParameterStatusEffectProcChance(string param, out int procChance)
		{
			if (TryParseLocalizationParameterStatusEffect(param, out var dataId, out var procIndex) && MineDb.TryGetByData(dataId, out var statusEffect))
			{
				return TryGetChanceToProc(statusEffect, procIndex, out procChance);
			}
			procChance = 0;
			return false;
		}

		// Token: 0x06002118 RID: 8472 RVA: 0x000859C8 File Offset: 0x00083BC8
		private static bool TryParseLocalizationParameterStatusEffect(string param, out string statusEffectDataId, out int procIndex)
		{
			statusEffectDataId = string.Empty;
			procIndex = 0;
			if (!param.StartsWith("statuseffect", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			string[] array = param.Split('.', StringSplitOptions.None);
			if (array.Length != 4 || !array[0].Equals("statuseffect", StringComparison.InvariantCultureIgnoreCase) || !array[3].Equals("chancetoproc", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (!int.TryParse(array[2], out procIndex))
			{
				return false;
			}
			statusEffectDataId = array[1];
			return true;
		}

		// Token: 0x06002119 RID: 8473 RVA: 0x00085A34 File Offset: 0x00083C34
		public static bool TryGetLocalizationParameterStatusEffectProcChance(string param, MinedAsset statusEffectData, out int procChance)
		{
			bool result = false;
			procChance = 0;
			int procIndex;
			if (TryParseLocalizationParameterStatusEffect(param, out procIndex))
			{
				result = TryGetChanceToProc(statusEffectData, procIndex, out procChance);
			}
			return result;
		}

		// Token: 0x0600211A RID: 8474 RVA: 0x00085A5C File Offset: 0x00083C5C
		private static bool TryParseLocalizationParameterStatusEffect(string param, out int procIndex)
		{
			procIndex = -1;
			if (!param.StartsWith("proc", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			string[] array = param.Split('.', StringSplitOptions.None);
			return array.Length == 3 && array[0].Equals("proc", StringComparison.InvariantCultureIgnoreCase) && array[2].Equals("chancetoproc", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(array[1], out procIndex);
		}

		// Token: 0x0600211B RID: 8475 RVA: 0x00085ABC File Offset: 0x00083CBC
		private static bool TryGetChanceToProc(MinedAsset statusEffect, int procIndex, out int value)
		{
			bool result = false;
			value = 0;
			if (statusEffect["procEffects"].Length > procIndex)
			{
				value = statusEffect["procEffects"].At(procIndex)["chanceToProc"].Int;
				result = true;
			}
			return result;
		}

		// Token: 0x0600211C RID: 8476 RVA: 0x00085AF4 File Offset: 0x00083CF4


    }
}

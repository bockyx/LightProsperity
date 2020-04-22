﻿using Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace LightProsperity
{
    internal class DefaultSettlementGarrisonModel : SettlementGarrisonModel
    {
        private static readonly TextObject _townWallsText = new TextObject("{=SlmhqqH8}Town Walls", (Dictionary<string, TextObject>)null);
        private static readonly TextObject _moraleText = new TextObject("{=UjL7jVYF}Morale", (Dictionary<string, TextObject>)null);
        private static readonly TextObject _foodShortageText = new TextObject("{=qTFKvGSg}Food Shortage", (Dictionary<string, TextObject>)null);
        private static readonly TextObject _surplusFoodText = GameTexts.FindText("str_surplus_food", (string)null);
        private static readonly TextObject _recruitFromCenterNotablesText = GameTexts.FindText("str_center_notables", (string)null);
        private static readonly TextObject _recruitFromVillageNotablesText = GameTexts.FindText("str_village_notables", (string)null);
        private static readonly TextObject _villageBeingRaided = GameTexts.FindText("str_village_being_raided", (string)null);
        private static readonly TextObject _villageLooted = GameTexts.FindText("str_village_looted", (string)null);
        private static readonly TextObject _townIsUnderSiege = GameTexts.FindText("str_villages_under_siege", (string)null);
        private static readonly TextObject _retiredText = GameTexts.FindText("str_retired", (string)null);
        private static readonly TextObject _paymentIsLess = GameTexts.FindText("str_payment_is_less", (string)null);
        private static readonly TextObject _issues = new TextObject("{=D7KllIPI}Issues", (Dictionary<string, TextObject>)null);

        public override int CalculateGarrisonChange(Settlement settlement, StatExplainer explanation = null)
        {
            return DefaultSettlementGarrisonModel.CalculateGarrisonChangeInternal(settlement, explanation);
        }

        private static int CalculateGarrisonChangeInternal(
          Settlement settlement,
          StatExplainer explanation = null)
        {
            ExplainedNumber result = new ExplainedNumber(0.0f, explanation, (TextObject)null);
            if (settlement.IsTown || settlement.IsCastle)
            {
                double loyalty = (double)settlement.Town.Loyalty;
                if (settlement.Town.GarrisonParty != null && ((double)settlement.Town.GarrisonParty.Party.NumberOfHealthyMembers + (double)result.ResultNumber) / (double)settlement.Town.GarrisonParty.Party.PartySizeLimit > (double)settlement.Town.GarrisonParty.PaymentRatio)
                {
                    int num = 0;
                    do
                    {
                        ++num;
                    }
                    while (((double)settlement.Town.GarrisonParty.Party.NumberOfHealthyMembers + (double)result.ResultNumber - (double)num) / (double)settlement.Town.GarrisonParty.Party.PartySizeLimit >= (double)settlement.Town.GarrisonParty.PaymentRatio && (double)settlement.Town.GarrisonParty.Party.NumberOfHealthyMembers + (double)result.ResultNumber - (double)num > 0.0 && num < 20);
                    result.Add((float)-num, DefaultSettlementGarrisonModel._paymentIsLess);
                }
            }
            DefaultSettlementGarrisonModel.GetSettlementGarrisonChangeDueToIssues(settlement, ref result);
            return (int)result.ResultNumber;
        }

        private static void GetSettlementGarrisonChangeDueToIssues(
          Settlement settlement,
          ref ExplainedNumber result)
        {
            float totalChange;
            if (!IssueManager.DoesSettlementHasIssueEffect(DefaultIssueEffects.SettlementGarrison, settlement, out totalChange))
                return;
            result.Add(totalChange, DefaultSettlementGarrisonModel._issues);
        }

        public override int FindNumberOfTroopsToTakeFromGarrison(
          MobileParty mobileParty,
          Settlement settlement)
        {
            MobileParty garrisonParty = settlement.Town.GarrisonParty;
            if (garrisonParty == null)
                return 0;
            float totalStrength = garrisonParty.Party.TotalStrength;
            float num1 = FactionHelper.FindIdealGarrisonStrengthPerWalledCenter(mobileParty.MapFaction as Kingdom, settlement.OwnerClan) * FactionHelper.OwnerClanEconomyEffectOnGarrisonSizeConstant(settlement.OwnerClan) * (settlement.IsTown ? 2f : 1f);
            double num2 = (double)Math.Min(12f, (float)mobileParty.Party.PartySizeLimit * mobileParty.PaymentRatio / (float)mobileParty.Party.NumberOfAllMembers) - 1.0;
            float num3 = (float)Math.Pow((double)totalStrength / (double)num1, 1.5);
            float num4 = mobileParty.LeaderHero.Clan.Leader == mobileParty.LeaderHero ? 2f : 1f;
            double num5 = (double)num3;
            int num6 = MBRandom.RoundRandomized((float)(num2 * num5) * num4);
            int num7 = 20 * (settlement.IsTown ? 2 : 1);
            if (num6 > garrisonParty.Party.MemberRoster.TotalRegulars - num7)
                num6 = garrisonParty.Party.MemberRoster.TotalRegulars - num7;
            return num6;
        }

        public override int FindNumberOfTroopsToLeaveToGarrison(
          MobileParty mobileParty,
          Settlement settlement)
        {
            MobileParty garrisonParty = settlement.Town.GarrisonParty;
            float num1 = 0.0f;
            if (garrisonParty != null)
                num1 = garrisonParty.Party.TotalStrength;
            float num2 = FactionHelper.FindIdealGarrisonStrengthPerWalledCenter(mobileParty.MapFaction as Kingdom, settlement.OwnerClan) * FactionHelper.OwnerClanEconomyEffectOnGarrisonSizeConstant(settlement.OwnerClan) * (settlement.IsTown ? 2f : 1f);
            if (settlement.OwnerClan.Leader == Hero.MainHero && (mobileParty.LeaderHero == null || mobileParty.LeaderHero.Clan != Clan.PlayerClan) || (double)num1 >= (double)num2)
                return 0;
            int ofRegularMembers = mobileParty.Party.NumberOfRegularMembers;
            float num3 = (float)(1.0 + (double)mobileParty.Party.NumberOfWoundedRegularMembers / (double)mobileParty.Party.NumberOfRegularMembers);
            float num4 = (float)mobileParty.Party.PartySizeLimit * mobileParty.PaymentRatio;
            float num5 = (float)(Math.Pow((double)Math.Min(2f, (float)ofRegularMembers / num4), 1.20000004768372) * 0.75);
            float val1 = (float)((1.0 - (double)num1 / (double)num2) * (1.0 - (double)num1 / (double)num2));
            if (mobileParty.Army != null)
                val1 = Math.Min(val1, 0.5f);
            float num6 = 0.5f;
            if (settlement.OwnerClan == mobileParty.Leader.HeroObject.Clan || settlement.OwnerClan == mobileParty.Party.Owner.MapFaction.Leader.Clan)
                num6 = 1f;
            float num7 = mobileParty.Army != null ? 1.25f : 1f;
            float num8 = 1f;
            List<float> floatList = new List<float>(5);
            for (int index = 0; index < 5; ++index)
                floatList.Add(Campaign.MapDiagonal * Campaign.MapDiagonal);
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom.IsKingdomFaction && mobileParty.MapFaction.IsAtWarWith((IFaction)kingdom))
                {
                    foreach (Settlement settlement1 in kingdom.Settlements)
                    {
                        float num9 = settlement1.Position2D.DistanceSquared(mobileParty.Position2D);
                        for (int index1 = 0; index1 < 5; ++index1)
                        {
                            if ((double)num9 < (double)floatList[index1])
                            {
                                for (int index2 = 4; index2 >= index1 + 1; --index2)
                                    floatList[index2] = floatList[index2 - 1];
                                floatList[index1] = num9;
                                break;
                            }
                        }
                    }
                }
            }
            float num10 = 0.0f;
            for (int index = 0; index < 5; ++index)
                num10 += (float)Math.Sqrt((double)floatList[index]);
            float num11 = num10 / 5f;
            double num12 = (double)Math.Max(0.0f, Math.Min((float)((double)Campaign.MapDiagonal / 15.0 - (double)Campaign.MapDiagonal / 30.0), num11 - Campaign.MapDiagonal / 30f)) / ((double)Campaign.MapDiagonal / 15.0 - (double)Campaign.MapDiagonal / 30.0);
            float num13 = Math.Min(0.7f, num8 * num5 * val1 * num6 * num7 * num3);
            return MBRandom.RoundRandomized((float)ofRegularMembers * num13);
        }
    }
}
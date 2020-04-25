﻿using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;

namespace LightProsperity
{
    public class SubModule : MBSubModuleBase
    {
        public static Settings Settings { get; private set; }

        private static void LoadSettings()
        {
            string configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");
            try
            {
                SubModule.Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(configPath));
            }
            catch
            {
            }
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            SubModule.LoadSettings();
            var harmony = new Harmony("mod.bannerlord.light");
            harmony.PatchAll();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            AddModels(gameStarterObject as CampaignGameStarter);
        }

        private void AddModels(CampaignGameStarter gameStarter)
        {
            gameStarter?.AddModel(new LightSettlementGarrisonModel());
            gameStarter?.AddModel(new LightSettlementProsperityModel());
        }
    }
}

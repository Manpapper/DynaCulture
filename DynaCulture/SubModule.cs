using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

using HarmonyLib;

using DynaCulture.Data;

namespace DynaCulture
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            Harmony harmony = new Harmony("mod.bannerlord.splintert");
            harmony.PatchAll();

            base.OnSubModuleLoad();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (!(game.GameType is Campaign))
                return;

            if (gameStarter is CampaignGameStarter)
                (gameStarter as CampaignGameStarter).AddBehavior(new DynaCultureBehavior());
        }
    }
}
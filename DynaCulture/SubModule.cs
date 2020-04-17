using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using ModLib;
using ModLib.Debugging;

using DynaCulture.Data;

namespace DynaCulture
{
    public class SubModule : MBSubModuleBase
    {
        public static readonly string ModuleFolderName = "DynaCulture";

        protected override void OnSubModuleLoad()
        {
            try
            {
                FileDatabase.Initialise(ModuleFolderName);
                SettingsDatabase.RegisterSettings(Settings.Instance);
            }
            catch (Exception ex)
            {
                ModDebug.ShowError($"Error Initializing DynaCulture:\n\n{ex.ToStringFull()}");
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (!(game.GameType is Campaign))
                return;

            this.AddBehaviors((CampaignGameStarter)gameStarter);
        }

        private void AddBehaviors(CampaignGameStarter gameStarter)
        {
            gameStarter.AddBehavior(new DynaCultureBehavior());
        }
    }
}
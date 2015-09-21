using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using UnityEngine;

namespace WG_BalancedPopMod
{
    public class PopBalanceMod : IUserMod
    {
        public string Name
        {
            get
            {
                swapVanillaHomeCount();
                return "WG Realistic Population";
            }
        }
        public string Description
        {
            get { return "Balances residential space and comsumption of water, power and production of waste to be more realistic. Wealth levels are edited to keep the game balanced."; }
        }

        private void swapVanillaHomeCount()
        {
            // Replace the one method call which is called when the city is loaded and EnsureCitizenUnits is used
            // ResidentialAI -> Game_ResidentialAI. This stops the buildings from going to game defaults on load.
            // This has no further effects on buildings as the templates are replaced by ResidentialAIMod
            var oldMethod = typeof(ResidentialBuildingAI).GetMethod("CalculateHomeCount");
            var newMethod = typeof(TrickResidentialAI).GetMethod("CalculateHomeCount");
            RedirectionHelper.RedirectCalls(oldMethod, newMethod);
        }
    }
}



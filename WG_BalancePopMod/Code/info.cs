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
            get { return "WG Realistic Population"; }
        }
        public string Description
        {
            get { return "Balances residential space and comsumption of water, power and production of waste to be more realistic. Wealth levels are edited to keep the game balanced."; }
        }
    }
}



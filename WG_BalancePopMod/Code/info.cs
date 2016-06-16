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
            get { return "WG Realistic Population v7.8"; }
        }
        public string Description
        {
            get { return "Related population to volume and utility needs are changed to be more realistic."; }
        }
    }
}



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
            get { return "WG Realistic Population v8.2.2"; }
        }
        public string Description
        {
            get { return "Building population are tied to volume and utility needs are changed to be more realistic."; }
        }
    }
}



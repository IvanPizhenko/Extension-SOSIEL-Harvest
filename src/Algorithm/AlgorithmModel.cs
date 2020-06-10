﻿using System.Collections.Generic;
using Landis.Extension.SOSIELHarvest.Models;

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    /// <summary>
    /// The model used to pass external data.
    /// </summary>
    public class AlgorithmModel
    {
        public List<NewDecisionOptionModel> NewDecisionOptions { get; set; }

        //Management area to selected decision options list
        public Dictionary<string, List<string>> SelectedDecisions { get; set; }

        public HarvestResults HarvestResults { get; set; }
    }
}

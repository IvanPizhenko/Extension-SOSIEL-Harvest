using System;
using System.Collections.Generic;
using System.Linq;
using Landis.Core;
using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Input;
using Landis.Library.BiomassCohorts;
using Landis.Library.HarvestManagement;
using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class Mode1 : Mode
    {
        private readonly ICore _core;
        private readonly SheParameters _sheParameters;
        private List<Prescription> _prescriptions;
        private ISiteVar<string> _harvestPrescriptionName;
        private ISiteVar<ISiteCohorts> _siteCohorts;
        private Dictionary<string, double> _harvested;

        public Mode1(ICore core, SheParameters sheParameters)
        {
            _core = core;
            _sheParameters = sheParameters;
            _harvested = new Dictionary<string, double>();
        }

        public override void Initialize()
        {
            _prescriptions = _sheParameters.Prescriptions;

            _harvestPrescriptionName = _core.GetSiteVar<string>("Harvest.PrescriptionName");

            _siteCohorts = _core.GetSiteVar<ISiteCohorts>("Succession.BiomassCohorts");

            var maDataSet = new ManagementAreaDataset();

            foreach (var agentToManagementArea in _sheParameters.AgentToManagementAreaList)
            {
                foreach (var managementAreaName in agentToManagementArea.ManagementAreas)
                {
                    if (Areas.ContainsKey(managementAreaName) == false)
                    {
                        var managementArea = new ManagementArea(ushort.Parse(managementAreaName));
                        maDataSet.Add(managementArea);
                        var newArea = new Area {Name = managementAreaName};
                        newArea.Initialize(managementArea);
                        Areas.Add(managementAreaName, newArea);
                    }

                    var area = Areas[managementAreaName];
                    area.AssignedAgents.Add(agentToManagementArea.Agent);
                }
            }

            ManagementAreas.ReadMap(_sheParameters.ManagementAreaFileName, maDataSet);
            Stands.ReadMap(_sheParameters.StandsFileName);
            SiteVars.GetExternalVars();
            foreach (ManagementArea mgmtArea in maDataSet)
                mgmtArea.FinishInitialization();
        }

        public override void Harvest()
        {
            ClearHarvested();
            
            foreach (var agent in Agents)
            {
                var areas = _sheParameters.AgentToManagementAreaList.First(map => map.Agent.Equals(agent.Id))
                    .ManagementAreas.Select(ma => Areas.First(area => area.Key.Equals(ma)).Value);
                
                foreach (var area in areas)
                {
                    var harvestManager =
                        new HarvestManager(area, _prescriptions, _harvestPrescriptionName, _siteCohorts);
                    _harvested[HarvestResults.GetKey(1, agent, area)] = harvestManager.Harvest();
                }
            }
        }

        public override HarvestResults AnalyzeHarvestingResult()
        {
            var results = new HarvestResults();

            foreach (var agent in Agents)
            {
                var areas = _sheParameters.AgentToManagementAreaList.First(map => map.Agent.Equals(agent.Id))
                    .ManagementAreas.Select(ma => Areas.First(area => area.Key.Equals(ma)).Value);
                
                foreach (var area in areas)
                {
                    results.ManageAreaBiomass[HarvestResults.GetKey(1, agent, area)] = 0;
                    results.ManageAreaMaturityPercent[HarvestResults.GetKey(1, agent, area)] = 0;

                    double manageAreaMaturityProportion = 0;

                    foreach (var stand in area.ManagementArea)
                    {
                        double standMaturityProportion = 0;

                        foreach (var site in stand)
                        {
                            double siteBiomass = 0;
                            double siteMaturity = 0;
                            double siteMaturityProportion;

                            foreach (var species in _core.Species)
                            {
                                var cohorts = _siteCohorts[site][species];

                                if (cohorts == null)
                                    continue;

                                double siteSpeciesMaturity = 0;

                                foreach (var cohort in cohorts)
                                {
                                    siteBiomass += cohort.Biomass;

                                    if (cohort.Age >= _core.Species[species.Name].Maturity)
                                        siteSpeciesMaturity += cohort.Biomass;
                                }

                                siteMaturity += siteSpeciesMaturity;
                            }

                            siteMaturityProportion = Math.Abs(siteBiomass) < 0.0001 ? 0 : siteMaturity / siteBiomass;
                            standMaturityProportion += siteMaturityProportion;

                            results.ManageAreaBiomass[HarvestResults.GetKey(1, agent, area)] += siteBiomass;
                        }

                        standMaturityProportion /= stand.Count();
                        manageAreaMaturityProportion += standMaturityProportion;
                    }

                    manageAreaMaturityProportion /= area.ManagementArea.StandCount;

                    results.ManageAreaBiomass[HarvestResults.GetKey(1, agent, area)] =
                        results.ManageAreaBiomass[HarvestResults.GetKey(1, agent, area)] / 100 * _core.CellArea;

                    results.ManageAreaHarvested[HarvestResults.GetKey(1, agent, area)] =
                        _harvested[HarvestResults.GetKey(1, agent, area)] / 100 * _core.CellArea;

                    results.ManageAreaMaturityPercent[HarvestResults.GetKey(1, agent, area)] = 100 * manageAreaMaturityProportion;
                }
            }

            return results;
        }

        protected override void OnAgentsSet()
        {
            base.OnAgentsSet();
            ClearHarvested();
        }

        private void ClearHarvested()
        {
            foreach (var agent in Agents)
            {
                var areas = _sheParameters.AgentToManagementAreaList.First(map => map.Agent.Equals(agent.Id))
                    .ManagementAreas.Select(ma => Areas.First(area => area.Key.Equals(ma)).Value);
                
                foreach (var area in areas)
                    _harvested[HarvestResults.GetKey(1, agent, area)] = 0;
            }
        }
    }
}
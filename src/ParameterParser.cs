// This file is part of the Social Human extension for LANDIS-II.

using Landis.Utilities;

namespace Landis.Extension.SOSIELHuman
{
    /// <summary>
    /// A parser that reads the extension's input and output parameters from
    /// a text file.
    /// </summary>
    public class ParameterParser
        : TextParser<Parameters> //BasicParameterParser<Parameters>
    {

        //---------------------------------------------------------------------

        public override string LandisDataValue
        {
            get {
                return PlugIn.ExtensionName;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ParameterParser()//ISpeciesDataset speciesDataset)
            //: base(speciesDataset, false)
        {
        }

        //---------------------------------------------------------------------

        protected override Parameters Parse()
        {
            ReadLandisDataVar();

            Parameters parameters = new Parameters();

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            InputVar<string> inputJsonFile = new InputVar<string>("InputJson");
            ReadVar(inputJsonFile);
            parameters.InputJson = inputJsonFile.Value;

            return parameters;
        }

    }
}
﻿using System;

namespace Landis.Extension.SOSIELHuman.Helpers
{
    public static class VariableTypeHelper
    {
        /// <summary>
        /// Converts type name to System.Type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Unknown variable type:" + type</exception>
        public static Type ConvertStringToType(string type)
        {
            switch (type)
            {
                case "integer": return typeof(int);
                case "number": return typeof(double);
                case "string": return typeof(string);
                default:
                    throw new ArgumentOutOfRangeException("Unknown variable type:" + type);
            }
        }

    }
}
// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Scada.Comm.Drivers.DrvSigModbus.Protocol
{
    /// <summary>
    /// The class contains utility methods for the Modbus protocol implementation.
    /// <para>Класс, содержащий вспомогательные константы и методы для реализации протокола Modbus.</para>
    /// </summary>
    internal static class SigModbusUtils
    {
        /// <summary>
        /// The array separator used for parsing.
        /// </summary>
        private static readonly char[] ParseArraySeparator = { ';', ',', ' ' };

        /// <summary>
        /// Converts the string representation of an array of double to its array equivalent.
        /// </summary>
        public static double[] ParseDoubleArray(string s)
        {
            try
            {
                string[] elems = (s ?? "").Split(ParseArraySeparator, StringSplitOptions.RemoveEmptyEntries);
                int len = elems.Length;
                double[] arr = new double[len];

                for (int i = 0; i < len; i++)
                {
                    arr[i] = double.Parse(elems[i]);
                }

                return arr;
            }
            catch (FormatException ex)
            {
                throw new FormatException("The specified string is not an array of double.", ex);
            }
        }

        /// <summary>
        /// Transforms a value from one range to another.
        /// </summary>
        public static double GetScaledValue(double value, double min1, double max1, double min2, double max2)
        {
            if (min1 == max1)
                throw new ArgumentException("The original range must have different minimum and maximum values.");

            if (min2 == max2)
                throw new ArgumentException("The new range must have different minimum and maximum values.");

            return (value - min1) / (max1 - min1) * (max2 - min2) + min2;
        }
    }
}

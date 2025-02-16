// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Drivers.DrvModbus.Protocol;
using System;

namespace Scada.Comm.Drivers.DrvSigModbus.Protocol
{
    /// <summary>
    /// Represents a Modbus command.
    /// <para>Представляет команду Modbus.</para>
    /// </summary>
    public class SigModbusCmd : ModbusCmd
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public SigModbusCmd(DataBlock dataBlock, bool multiple)
            : base(dataBlock, multiple)
        {
            Scaling = "";
        }


        /// <summary>
        /// Gets or sets the scaling double array.
        /// </summary>
        public virtual string Scaling { get; set; }

        /// <summary>
        /// Sets the command data, converted according to the command element type.
        /// </summary>
        public override void SetCmdData(double cmdVal)
        {
            double rawVal = cmdVal;

            switch (ElemType)
            {
                case ElemType.UShort:
                    break;
                case ElemType.Short:
                    break;
                default:
                    base.SetCmdData(rawVal);
                    return;
            }

            // Check if scaling is needed
            if (!string.IsNullOrWhiteSpace(Scaling))
            {
                double[] scaling = SigModbusUtils.ParseDoubleArray(Scaling);
                if (scaling.Length == 4 && scaling[0] != scaling[1] && scaling[2] != scaling[3])
                {
                    // reverse scaling for cmd
                    rawVal = SigModbusUtils.GetScaledValue(rawVal, scaling[2], scaling[3], scaling[0], scaling[1]);
                    rawVal = Math.Round(rawVal, 0);
                }
            }

            base.SetCmdData(rawVal);
        }
    }
}

// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Drivers.DrvModbus.Protocol;

namespace Scada.Comm.Drivers.DrvSigModbus.Protocol
{
    /// <summary>
    /// Represents a group of Modbus elements.
    /// <para>Представляет группу элементов Modbus.</para>
    /// </summary>
    public class SigElemGroup : ElemGroup
    {
         /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public SigElemGroup(DataBlock dataBlock)
            : base(dataBlock)
        {
        }


        /// <summary>
        /// Creates a new Modbus element.
        /// </summary>
        public override Elem CreateElem()
        {
            return new SigElem();
        }

        /// <summary>
        /// Gets the element value according to its type, converted to double.
        /// </summary>
        public override double GetElemVal(int elemIdx)
        {
            Elem elem = Elems[elemIdx];

            double elemValue = base.GetElemVal(elemIdx);
            switch (elem.ElemType)
            {
                case ElemType.UShort:
                    break;
                case ElemType.Short:
                    break;
                default:
                    return elemValue;
            }

            // Check if scaling is needed
            if (!string.IsNullOrWhiteSpace(((SigElem)elem).Scaling))
            {
                double[] scaling = SigModbusUtils.ParseDoubleArray(((SigElem)elem).Scaling);
                if (scaling.Length == 4 && scaling[0] != scaling[1] && scaling[2] != scaling[3])
                    return SigModbusUtils.GetScaledValue(elemValue, scaling[0], scaling[1], scaling[2], scaling[3]);
            }

            // Return the original value
            return elemValue;
        }
    }
}

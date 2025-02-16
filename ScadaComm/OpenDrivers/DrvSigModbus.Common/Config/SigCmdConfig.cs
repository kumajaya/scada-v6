// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Drivers.DrvSigModbus.Protocol;
using Scada.Comm.Drivers.DrvModbus.Config;
using Scada.Comm.Drivers.DrvModbus.Protocol;
using System;
using System.Xml;

namespace Scada.Comm.Drivers.DrvSigModbus.Config
{
    /// <summary>
    /// Represents a command configuration.
    /// <para>Представляет конфигурацию команды.</para>
    /// </summary>
    public class SigCmdConfig : CmdConfig
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public SigCmdConfig()
            : base()
        {
            Scaling = "";
        }


        /// <summary>
        /// Gets or sets the scaling double array.
        /// </summary>
        public virtual string Scaling { get; set; }

        /// <summary>
        /// Gets a value indicating whether a scaling is applicable for the elements.
        /// </summary>
        public virtual bool ScalingEnabled
        {
            get
            {
                return DataBlock == DataBlock.InputRegisters || DataBlock == DataBlock.HoldingRegisters;
            }
        }

        /// <summary>
        /// Loads the configuration from the XML node.
        /// </summary>
        public override void LoadFromXml(XmlElement xmlElem)
        {
            if (xmlElem == null)
                throw new ArgumentNullException(nameof(xmlElem));

            base.LoadFromXml(xmlElem);
            Scaling = xmlElem.GetAttrAsString("scaling");
        }

        /// <summary>
        /// Saves the configuration into the XML node.
        /// </summary>
        public override void SaveToXml(XmlElement xmlElem)
        {
            if (xmlElem == null)
                throw new ArgumentNullException(nameof(xmlElem));

            base.SaveToXml(xmlElem);

            if (DataBlock != DataBlock.Custom)
            {
                if (ScalingEnabled && !string.IsNullOrWhiteSpace(Scaling))
                {
                    double[] scaling = SigModbusUtils.ParseDoubleArray(Scaling);
                    if (scaling.Length == 4 && scaling[0] != scaling[1] && scaling[2] != scaling[3])
                        xmlElem.SetAttribute("scaling", Scaling);
                }
            }
        }
    }
}

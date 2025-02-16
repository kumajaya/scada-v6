// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Drivers.DrvSigModbus.Protocol;
using Scada.Comm.Drivers.DrvModbus.Config;
using Scada.Comm.Drivers.DrvModbus.Protocol;
using System;
using System.Xml;
using System.Linq;

namespace Scada.Comm.Drivers.DrvSigModbus.Config
{
    /// <summary>
    /// Represents an element group configuration.
    /// <para>Представляет конфигурацию группы элементов.</para>
    /// </summary>
    public class SigElemGroupConfig : ElemGroupConfig
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public SigElemGroupConfig()
            : base()
        {
        }


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
        /// Creates a new element configuration.
        /// </summary>
        public override ElemConfig CreateElemConfig()
        {
            return new SigElemConfig { ElemType = DefaultElemType };
        }

        /// <summary>
        /// Loads the configuration from the XML node.
        /// </summary>
        public override void LoadFromXml(XmlElement xmlElem)
        {
            if (xmlElem == null)
                throw new ArgumentNullException(nameof(xmlElem));

            Active = xmlElem.GetAttrAsBool("active");
            DataBlock = xmlElem.GetAttrAsEnum("dataBlock", xmlElem.GetAttrAsEnum<DataBlock>("tableType"));
            Address = xmlElem.GetAttrAsInt("address");
            Name = xmlElem.GetAttrAsString("name");

            ElemType defaultElemType = DefaultElemType;
            bool defaultReadOnly = !ReadOnlyEnabled;
            bool defaultBitMask = !BitMaskEnabled;
            int maxElemCnt = MaxElemCnt;

            foreach (XmlElement elemElem in xmlElem.SelectNodes("Elem"))
            {
                if (Elems.Count >= maxElemCnt)
                    break;

                ElemConfig elemConfig = CreateElemConfig();
                elemConfig.ElemType = elemElem.GetAttrAsEnum("type", defaultElemType);
                elemConfig.ByteOrder = elemElem.GetAttrAsString("byteOrder");
                elemConfig.ReadOnly = elemElem.GetAttrAsBool("readOnly", defaultReadOnly);
                elemConfig.IsBitMask = elemElem.GetAttrAsBool("isBitMask", defaultBitMask);
                ((SigElemConfig)elemConfig).Scaling = elemElem.GetAttrAsString("scaling", "");
                elemConfig.TagCode = elemElem.GetAttrAsString("tagCode");
                elemConfig.Name = elemElem.GetAttrAsString("name");
                Elems.Add(elemConfig);
            }
        }

        /// <summary>
        /// Saves the configuration into the XML node.
        /// </summary>
        public override void SaveToXml(XmlElement xmlElem)
        {
            if (xmlElem == null)
                throw new ArgumentNullException(nameof(xmlElem));

            xmlElem.SetAttribute("active", Active);
            xmlElem.SetAttribute("dataBlock", DataBlock);
            xmlElem.SetAttribute("address", Address);
            xmlElem.SetAttribute("name", Name);

            bool elemTypeEnabled = ElemTypeEnabled;
            bool byteOrderEnabled = ByteOrderEnabled;
            bool readOnlyEnabled = ReadOnlyEnabled;
            bool bitMaskEnabled = BitMaskEnabled;
            bool scalingEnabled = ScalingEnabled;

            foreach (ElemConfig elemConfig in Elems)
            {
                XmlElement elemElem = xmlElem.AppendElem("Elem");

                if (elemTypeEnabled)
                    elemElem.SetAttribute("type", elemConfig.ElemType.ToString().ToLowerInvariant());

                if (byteOrderEnabled && !string.IsNullOrEmpty(elemConfig.ByteOrder))
                    elemElem.SetAttribute("byteOrder", elemConfig.ByteOrder);

                if (readOnlyEnabled)
                    elemElem.SetAttribute("readOnly", elemConfig.ReadOnly);

                if (bitMaskEnabled)
                    elemElem.SetAttribute("isBitMask", elemConfig.IsBitMask);

                if (scalingEnabled && !string.IsNullOrWhiteSpace(((SigElemConfig)elemConfig).Scaling))
                {
                    double[] scaling = SigModbusUtils.ParseDoubleArray(((SigElemConfig)elemConfig).Scaling);
                    if (scaling.Length == 4 && scaling[0] != scaling[1] && scaling[2] != scaling[3])
                        elemElem.SetAttribute("scaling", ((SigElemConfig)elemConfig).Scaling);
                }

                elemElem.SetAttribute("tagCode", elemConfig.TagCode);
                elemElem.SetAttribute("name", elemConfig.Name);
            }
        }
    }
}

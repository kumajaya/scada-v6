// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Comm.Drivers.DrvSigModbus.Config;
using Scada.Comm.Drivers.DrvSigModbus.Protocol;
using Scada.Comm.Drivers.DrvModbus.Config;
using Scada.Comm.Drivers.DrvModbus.Protocol;
using Scada.Comm.Drivers.DrvModbus.Logic;
using System.Linq;
using System;

namespace Scada.Comm.Drivers.DrvSigModbus.Logic
{
    /// <summary>
    /// Implements the device logic.
    /// <para>Реализует логику устройства.</para>
    /// </summary>
    internal class DevSigModbusLogic : DevModbusLogic
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevSigModbusLogic(ICommContext commContext, ILineContext lineContext, DeviceConfig deviceConfig)
            : base(commContext, lineContext, deviceConfig)
        {
        }


        /// <summary>
        /// Gets the shared data key of the template dictionary.
        /// </summary>
        protected override string TemplateDictKey => "SigModbus.Templates";

        /// <summary>
        /// Creates a new Modbus command based on the element configuration.
        /// </summary>
        private ModbusCmd CreateModbusCmd(DeviceTemplateOptions options,
            ElemGroupConfig elemGroupConfig, ElemConfig elemConfig, int elemAddrOffset)
        {
            ModbusCmd modbusCmd = deviceModel.CreateModbusCmd(elemGroupConfig.DataBlock, elemConfig.Quantity > 1);
            modbusCmd.Name = elemConfig.Name;
            modbusCmd.Address = (ushort)(elemGroupConfig.Address + elemAddrOffset);
            modbusCmd.ElemType = elemConfig.ElemType;
            modbusCmd.ElemCnt = 1;
            modbusCmd.ByteOrder = ModbusUtils.ParseByteOrder(elemConfig.ByteOrder) ??
                options.GetDefaultByteOrder(ModbusUtils.GetDataLength(elemConfig.ElemType));
            ((SigModbusCmd)modbusCmd).Scaling = ((SigElemConfig)elemConfig).Scaling;
            modbusCmd.CmdNum = 0;
            modbusCmd.CmdCode = elemConfig.TagCode;

            modbusCmd.InitReqPDU();
            modbusCmd.InitReqADU(deviceModel.Addr, transMode);
            return modbusCmd;
        }

        /// <summary>
        /// Creates a new Modbus command based on the command configuration.
        /// </summary>
        private ModbusCmd CreateModbusCmd(DeviceTemplateOptions options, CmdConfig cmdConfig)
        {
            ModbusCmd modbusCmd = deviceModel.CreateModbusCmd(cmdConfig.DataBlock, cmdConfig.Multiple);
            modbusCmd.Name = cmdConfig.Name;
            modbusCmd.Address = (ushort)cmdConfig.Address;
            modbusCmd.ElemType = cmdConfig.ElemType;
            modbusCmd.ElemCnt = cmdConfig.ElemCnt;
            modbusCmd.ByteOrder = ModbusUtils.ParseByteOrder(cmdConfig.ByteOrder) ??
                options.GetDefaultByteOrder(ModbusUtils.GetDataLength(cmdConfig.ElemType) * cmdConfig.ElemCnt);
            ((SigModbusCmd)modbusCmd).Scaling = ((SigCmdConfig)cmdConfig).Scaling;
            modbusCmd.CmdNum = cmdConfig.CmdNum;
            modbusCmd.CmdCode = cmdConfig.CmdCode;

            if (cmdConfig.DataBlock == DataBlock.Custom)
                modbusCmd.SetFuncCode((byte)cmdConfig.CustomFuncCode);

            modbusCmd.InitReqPDU();
            modbusCmd.InitReqADU(deviceModel.Addr, transMode);
            return modbusCmd;
        }

        /// <summary>
        /// Gets the device tag format depending on the Modbus element type.
        /// </summary>
        private TagFormat GetSigTagFormat(ElemConfig elemConfig)
        {
            if (!string.IsNullOrWhiteSpace(((SigElemConfig)elemConfig).Scaling) &&
                    (elemConfig.ElemType == ElemType.UShort || elemConfig.ElemType == ElemType.Short))
                return TagFormat.FloatNumber;
            
            return base.GetTagFormat(elemConfig);
        }

        /// <summary>
        /// Create a new device template.
        /// </summary>
        protected override DeviceTemplate CreateDeviceTemplate()
        {
            return new SigDeviceTemplate();
        }

        /// <summary>
        /// Create a new device model.
        /// </summary>
        protected override DeviceModel CreateDeviceModel()
        {
            return new SigDeviceModel();
        }

        /// <summary>
        /// Initializes the device tags.
        /// </summary>
        public override void InitDeviceTags()
        {
            DeviceTemplate deviceTemplate = GetDeviceTemplate();

            if (deviceTemplate == null)
                return;

            // create device model
            deviceModel = CreateDeviceModel();
            deviceModel.Addr = (byte)NumAddress;

            // add model elements and device tags
            foreach (ElemGroupConfig elemGroupConfig in deviceTemplate.ElemGroups)
            {
                bool groupActive = elemGroupConfig.Active;
                bool groupCommands = groupActive && elemGroupConfig.ReadOnlyEnabled;
                ElemGroup elemGroup = null;
                TagGroup tagGroup = new TagGroup(elemGroupConfig.Name) { Hidden = !groupActive };
                int elemAddrOffset = 0;

                if (groupActive)
                {
                    elemGroup = deviceModel.CreateElemGroup(elemGroupConfig.DataBlock);
                    elemGroup.Name = elemGroupConfig.Name;
                    elemGroup.Address = (ushort)elemGroupConfig.Address;
                    elemGroup.StartTagIdx = DeviceTags.Count;
                }

                foreach (ElemConfig elemConfig in elemGroupConfig.Elems)
                {
                    // add model element
                    if (groupActive)
                    {
                        Elem elem = elemGroup.CreateElem();
                        elem.Name = elemConfig.Name;
                        elem.ElemType = elemConfig.ElemType;
                        elem.ByteOrder = ModbusUtils.ParseByteOrder(elemConfig.ByteOrder) ??
                            deviceTemplate.Options.GetDefaultByteOrder(ModbusUtils.GetDataLength(elemConfig.ElemType));
                        ((SigElem)elem).Scaling = ((SigElemConfig)elemConfig).Scaling;
                        elemGroup.Elems.Add(elem);
                    }

                    // add model command
                    if (groupCommands && !elemConfig.ReadOnly && !string.IsNullOrEmpty(elemConfig.TagCode))
                    {
                        deviceModel.Cmds.Add(
                            CreateModbusCmd(deviceTemplate.Options, elemGroupConfig, elemConfig, elemAddrOffset));
                    }

                    // add device tag
                    tagGroup.AddTag(elemConfig.TagCode, elemConfig.Name).SetFormat(GetSigTagFormat(elemConfig));
                    elemAddrOffset += elemConfig.Quantity;
                }

                if (groupActive)
                {
                    elemGroup.InitReqPDU();
                    elemGroup.InitReqADU(deviceModel.Addr, transMode);
                    deviceModel.ElemGroups.Add(elemGroup);
                }

                DeviceTags.AddGroup(tagGroup);
            }

            // add model commands
            foreach (CmdConfig cmdConfig in deviceTemplate.Cmds)
            {
                deviceModel.Cmds.Add(CreateModbusCmd(deviceTemplate.Options, cmdConfig));
            }

            deviceModel.InitCmdMap();
            CanSendCommands = deviceModel.Cmds.Count > 0;
            InitModbusPoll();
        }
    }
}

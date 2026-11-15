// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Buffers.Binary;

namespace Scada.Comm.Drivers.DrvSigModbus.Protocol
{
    /// <summary>
    /// Represents a Modbus command.
    /// <para>Представляет команду Modbus.</para>
    /// </summary>
    public class ModbusCmd : DataUnit
    {
        private string reqDescr; // the command description


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ModbusCmd(DataBlock dataBlock, bool multiple)
            : base(dataBlock)
        {
            if (dataBlock != DataBlock.Coils &&
                dataBlock != DataBlock.HoldingRegisters &&
                dataBlock != DataBlock.Custom)
            {
                throw new InvalidOperationException(ModbusPhrases.IllegalDataBlock);
            }

            reqDescr = "";
            Multiple = multiple;
            ElemType = ElemType.Undefined;
            ElemCnt = 1;
            ByteOrder = null;
            Scaling = "";
            CmdNum = 0;
            CmdCode = "";
            Value = 0;
            Data = null;

            SetFuncCode(ModbusUtils.GetWriteFuncCode(dataBlock, multiple));
        }


        /// <summary>
        /// Gets a description of the request that writes the command.
        /// </summary>
        public override string ReqDescr
        {
            get
            {
                if (string.IsNullOrEmpty(reqDescr))
                    reqDescr = string.Format(ModbusPhrases.Command, Name);
                return reqDescr;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command writes multiple elements.
        /// </summary>
        public bool Multiple { get; set; }

        /// <summary>
        /// Gets or sets the command element type.
        /// </summary>
        public ElemType ElemType { get; set; }

        /// <summary>
        /// Gets or sets the number of elements written by the command.
        /// </summary>
        public int ElemCnt { get; set; }

        /// <summary>
        /// Gets or sets the byte order.
        /// </summary>
        public int[]? ByteOrder { get; set; }

        /// <summary>
        /// Gets or sets the scaling double array.
        /// </summary>
        public string Scaling { get; set; }

        /// <summary>
        /// Gets or sets the command number.
        /// </summary>
        public int CmdNum { get; set; }

        /// <summary>
        /// Gets or sets the command code.
        /// </summary>
        public string CmdCode { get; set; }

        /// <summary>
        /// Gets or sets the command value.
        /// </summary>
        public ushort Value { get; set; }

        /// <summary>
        /// Gets or sets the data of the multiple command.
        /// </summary>
        public byte[]? Data { get; set; }


        /// <summary>
        /// Initializes the request PDU and calculates the response length.
        /// </summary>
        public override void InitReqPDU()
        {
            if (DataBlock == DataBlock.Custom)
            {
                // build PDU for custom command
                byte[]? data = Data;
                int dataLength = data == null ? 0 : data.Length;
                ReqPDU = new byte[1 + dataLength];
                ReqPDU[0] = FuncCode;

                if (data != null && dataLength > 0)
                    Buffer.BlockCopy(data, 0, ReqPDU, 1, dataLength);

                RespPduLen = ReqPDU.Length; // assuming echo
            }
            else if (Multiple)
            {
                // build PDU for WriteMultipleCoils and WriteMultipleRegisters commands
                int quantity;   // quantity of registers
                int dataLength; // data length in bytes

                if (DataBlock == DataBlock.Coils)
                {
                    quantity = ElemCnt;
                    dataLength = (ElemCnt % 8 == 0) ? ElemCnt / 8 : ElemCnt / 8 + 1;
                }
                else
                {
                    quantity = ElemCnt * ModbusUtils.GetQuantity(ElemType);
                    dataLength = quantity * 2;
                }

                // Ensure Data is allocated even if InitReqPDU runs before SetCmdData
                // (e.g. during device tag initialization), and to guard against
                // ApplyByteOrder implementations that may not tolerate a null src.
                if (Data == null)
                    Data = new byte[dataLength];

                ReqPDU = new byte[6 + dataLength];
                ReqPDU[0] = FuncCode;
                ReqPDU[1] = (byte)(Address / 256);
                ReqPDU[2] = (byte)(Address % 256);
                ReqPDU[3] = (byte)(quantity / 256);
                ReqPDU[4] = (byte)(quantity % 256);
                ReqPDU[5] = (byte)dataLength;

                ModbusUtils.ApplyByteOrder(Data, 0, ReqPDU, 6, dataLength, ByteOrder, false);

                // set response length
                RespPduLen = 5;
            }
            else
            {
                // build PDU for WriteSingleCoil and WriteSingleRegister commands
                int dataLength = DataBlock == DataBlock.Coils ? 2 : ModbusUtils.GetDataLength(ElemType);
                ReqPDU = new byte[3 + dataLength];
                ReqPDU[0] = FuncCode;
                ReqPDU[1] = (byte)(Address / 256);
                ReqPDU[2] = (byte)(Address % 256);

                if (DataBlock == DataBlock.Coils)
                {
                    ReqPDU[3] = Value > 0 ? (byte)0xFF : (byte)0x00;
                    ReqPDU[4] = 0x00;
                }
                else
                {
                    // Ensure Data is allocated even if InitReqPDU runs before SetCmdData.
                    if (Data == null && dataLength != 2)
                        Data = new byte[dataLength];

                    byte[]? data = dataLength == 2 ?
                        new byte[] // standard Modbus
                        {
                            (byte)(Value / 256),
                            (byte)(Value % 256)
                        } :
                        Data;

                    ModbusUtils.ApplyByteOrder(data, 0, ReqPDU, 3, dataLength, ByteOrder, false);
                }

                // set response length
                RespPduLen = ReqPDU.Length; // echo
            }
        }

        /// <summary>
        /// Decodes the response PDU.
        /// </summary>
        public override bool DecodeRespPDU(byte[] buffer, int offset, int length, out string errMsg)
        {
            if (base.DecodeRespPDU(buffer, offset, length, out errMsg))
            {
                if (DataBlock == DataBlock.Custom ||
                    buffer[offset + 1] == ReqPDU![1] && buffer[offset + 2] == ReqPDU[2] &&
                    buffer[offset + 3] == ReqPDU[3] && buffer[offset + 4] == ReqPDU[4])
                {
                    return true;
                }
                else
                {
                    errMsg = ModbusPhrases.InvalidPduData;
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the command data, converted according to the command element type.
        /// </summary>
        public void SetCmdData(double cmdVal)
        {
            double rawVal = cmdVal;

            // Apply scaling only for numeric types
            if (ElemType != ElemType.Undefined && ElemType != ElemType.Bool)
            {
                double[]? scaling = GetParsedScaling();
                if (scaling != null)
                {
                    rawVal = ModbusUtils.GetScaledValue(cmdVal, scaling[2], scaling[3], scaling[0], scaling[1]);

                    // Round only for integer types
                    if (ElemType == ElemType.UShort || ElemType == ElemType.Short ||
                        ElemType == ElemType.UInt || ElemType == ElemType.Int ||
                        ElemType == ElemType.ULong || ElemType == ElemType.Long)
                    {
                        rawVal = Math.Round(rawVal, 0);
                    }

                    // Range validation for integer types
                    if (ElemType == ElemType.UShort && (rawVal < 0 || rawVal > 65535))
                        throw new ArgumentException($"Scaled value {rawVal} is out of range for UShort (0-65535).");
                    if (ElemType == ElemType.Short && (rawVal < -32768 || rawVal > 32767))
                        throw new ArgumentException($"Scaled value {rawVal} is out of range for Short (-32768-32767).");
                    if (ElemType == ElemType.UInt && (rawVal < 0 || rawVal > 4294967295))
                        throw new ArgumentException($"Scaled value {rawVal} is out of range for UInt.");
                    if (ElemType == ElemType.Int && (rawVal < -2147483648 || rawVal > 2147483647))
                        throw new ArgumentException($"Scaled value {rawVal} is out of range for Int.");

                    // Use decimal with try-catch for accurate ULong/Long range validation
                    if (ElemType == ElemType.ULong)
                    {
                        try
                        {
                            decimal d = (decimal)rawVal;
                            if (d < 0 || d > ulong.MaxValue)
                                throw new ArgumentException($"Scaled value {rawVal} is out of range for ULong.");
                        }
                        catch (OverflowException)
                        {
                            throw new ArgumentException($"Scaled value {rawVal} is out of range for ULong.");
                        }
                    }
                    if (ElemType == ElemType.Long)
                    {
                        try
                        {
                            decimal d = (decimal)rawVal;
                            if (d < long.MinValue || d > long.MaxValue)
                                throw new ArgumentException($"Scaled value {rawVal} is out of range for Long.");
                        }
                        catch (OverflowException)
                        {
                            throw new ArgumentException($"Scaled value {rawVal} is out of range for Long.");
                        }
                    }
                }
            }

            // Reject values that cannot be encoded: NaN or infinity would be sent as 0 or an
            // arbitrary value, and a Float beyond its range would be sent as infinity
            if (ElemType != ElemType.Undefined && ElemType != ElemType.Bool)
            {
                if (!double.IsFinite(rawVal))
                    throw new ArgumentException($"Value {rawVal} is not a finite number.");
                if (ElemType == ElemType.Float && Math.Abs(rawVal) > float.MaxValue)
                    throw new ArgumentException($"Value {rawVal} is out of range for Float.");
            }

            // Directly allocate Data with correct endianness (Big-Endian, no reverse needed)
            switch (ElemType)
            {
                case ElemType.UShort:
                    Data = new byte[2];
                    BinaryPrimitives.WriteUInt16BigEndian(Data, (ushort)rawVal);
                    break;
                case ElemType.Short:
                    Data = new byte[2];
                    BinaryPrimitives.WriteInt16BigEndian(Data, (short)rawVal);
                    break;
                case ElemType.UInt:
                    Data = new byte[4];
                    BinaryPrimitives.WriteUInt32BigEndian(Data, (uint)rawVal);
                    break;
                case ElemType.Int:
                    Data = new byte[4];
                    BinaryPrimitives.WriteInt32BigEndian(Data, (int)rawVal);
                    break;
                case ElemType.Float:
                    Data = new byte[4];
                    BinaryPrimitives.WriteSingleBigEndian(Data, (float)rawVal);
                    break;
                case ElemType.ULong:
                    Data = new byte[8];
                    BinaryPrimitives.WriteUInt64BigEndian(Data, (ulong)rawVal);
                    break;
                case ElemType.Long:
                    Data = new byte[8];
                    BinaryPrimitives.WriteInt64BigEndian(Data, (long)rawVal);
                    break;
                case ElemType.Double:
                    Data = new byte[8];
                    BinaryPrimitives.WriteDoubleBigEndian(Data, rawVal);
                    break;
                default:
                    // Fallback for Undefined, Bool, or any other type: keep native byte order
                    // (matches original behavior, which never reversed this case).
                    Data = BitConverter.GetBytes(rawVal);
                    break;
            }
        }

        private double[]? _parsedScaling; // cached parsed scaling

        /// <summary>
        /// Gets the parsed scaling array, or null if scaling is not applicable or invalid.
        /// </summary>
        public double[]? GetParsedScaling()
        {
            return ModbusUtils.GetParsedScaling(Scaling, ref _parsedScaling);
        }
    }
}

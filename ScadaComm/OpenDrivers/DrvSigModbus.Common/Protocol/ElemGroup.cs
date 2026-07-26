// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Scada.Comm.Drivers.DrvSigModbus.Protocol
{
    /// <summary>
    /// Represents a group of Modbus elements.
    /// <para>Представляет группу элементов Modbus.</para>
    /// </summary>
    public class ElemGroup : DataUnit
    {
        private string reqDescr; // the request description


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ElemGroup(DataBlock dataBlock)
            : base(dataBlock)
        {
            reqDescr = "";
            Elems = new List<Elem>();
            ElemData = Array.Empty<byte[]>();
            StartTagIdx = -1;

            SetFuncCode(ModbusUtils.GetReadFuncCode(dataBlock));
        }


        /// <summary>
        /// Gets a description of the request that reads the element group.
        /// </summary>
        public override string ReqDescr
        {
            get
            {
                if (string.IsNullOrEmpty(reqDescr))
                    reqDescr = string.Format(ModbusPhrases.Request, Name);
                return reqDescr;
            }
        }

        /// <summary>
        /// Gets the elements of the group.
        /// </summary>
        public List<Elem> Elems { get; private set; }

        /// <summary>
        /// Gets the raw data of the group elements.
        /// </summary>
        public byte[][] ElemData { get; private set; }

        /// <summary>
        /// Gets or sets the device tag index that corresponds to the start element.
        /// </summary>
        public int StartTagIdx { get; set; }


        /// <summary>
        /// Initializes the request PDU and calculates the response length.
        /// </summary>
        public override void InitReqPDU()
        {
            // get total request amount
            int totalQuantity = 0;
            int totalDataLength = 0;

            foreach (Elem elem in Elems)
            {
                totalQuantity += elem.Quantity;
                totalDataLength += elem.DataLength;
            }

            // build PDU
            ReqPDU = new byte[5];
            ReqPDU[0] = FuncCode;
            ReqPDU[1] = (byte)(Address / 256);
            ReqPDU[2] = (byte)(Address % 256);
            ReqPDU[3] = (byte)(totalQuantity / 256);
            ReqPDU[4] = (byte)(totalQuantity % 256);

            // calculate response length
            if (DataBlock == DataBlock.DiscreteInputs || DataBlock == DataBlock.Coils)
            {
                int n = totalQuantity / 8;
                if ((totalQuantity % 8) > 0)
                    n++;
                RespPduLen = 2 + n;
                RespByteCnt = (byte)n;
            }
            else
            {
                RespPduLen = 2 + totalDataLength;
                RespByteCnt = (byte)totalDataLength;
            }

            // initialize array of element values
            int elemCnt = Elems.Count;
            ElemData = new byte[elemCnt][];

            for (int i = 0; i < elemCnt; i++)
            {
                Elem elem = Elems[i];
                // Remove Array.Clear as new byte array is already zero-initialized
                ElemData[i] = new byte[elem.DataLength];
            }
        }

        /// <summary>
        /// Decodes the response PDU.
        /// </summary>
        public override bool DecodeRespPDU(byte[] buffer, int offset, int length, out string errMsg)
        {
            if (base.DecodeRespPDU(buffer, offset, length, out errMsg))
            {
                if (buffer[offset + 1] == RespByteCnt)
                {
                    int len = ElemData.Length;
                    int byteNum = offset + 2;

                    if (DataBlock == DataBlock.DiscreteInputs || DataBlock == DataBlock.Coils)
                    {
                        int bitNum = 0;
                        for (int elemInd = 0; elemInd < len; elemInd++)
                        {
                            ElemData[elemInd][0] = ((buffer[byteNum] >> bitNum) & 0x01) > 0 ? (byte)1 : (byte)0;

                            if (++bitNum == 8)
                            {
                                bitNum = 0;
                                byteNum++;
                            }
                        }
                    }
                    else
                    {
                        for (int elemInd = 0; elemInd < len; elemInd++)
                        {
                            byte[] elemVal = ElemData[elemInd];
                            int elemDataLen = Elems[elemInd].DataLength;

                            // copy the read bytes in reverse order
                            for (int i = elemDataLen - 1, j = byteNum; i >= 0; i--, j++)
                            {
                                elemVal[i] = buffer[j];
                            }

                            byteNum += elemDataLen;
                        }
                    }

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
        /// Creates a new Modbus element.
        /// </summary>
        public virtual Elem CreateElem()
        {
            return new Elem();
        }

        /// <summary>
        /// Gets the element value according to its type, converted to double.
        /// </summary>
        public double GetElemVal(int elemIdx)
        {
            Elem elem = Elems[elemIdx];
            ReadOnlySpan<byte> src = ElemData[elemIdx].AsSpan();

            // order bytes if needed
            double elemValue;
            if (elem.ByteOrder == null)
            {
                elemValue = GetValueFromBuffer(src, elem.ElemType);
            }
            else
            {
                Span<byte> ordered = stackalloc byte[src.Length];
                ModbusUtils.ApplyByteOrder(src, ordered, elem.ByteOrder);
                elemValue = GetValueFromBuffer(ordered, elem.ElemType);
            }

            // Apply scaling if needed (cached parsing)
            double[]? scaling = elem.GetParsedScaling();
            if (scaling != null)
                return ModbusUtils.GetScaledValue(elemValue, scaling[0], scaling[1], scaling[2], scaling[3]);

            // Return the original value
            return elemValue;
        }

        /// <summary>
        /// Converts the buffer content to a double according to the element type.
        /// </summary>
        private static double GetValueFromBuffer(ReadOnlySpan<byte> buffer, ElemType elemType)
        {
            switch (elemType)
            {
                case ElemType.UShort:
                    return BitConverter.ToUInt16(buffer);
                case ElemType.Short:
                    return BitConverter.ToInt16(buffer);
                case ElemType.UInt:
                    return BitConverter.ToUInt32(buffer);
                case ElemType.Int:
                    return BitConverter.ToInt32(buffer);
                case ElemType.ULong: // possible data loss
                    return BitConverter.ToUInt64(buffer);
                case ElemType.Long:  // possible data loss
                    return BitConverter.ToInt64(buffer);
                case ElemType.Float:
                    return BitConverter.ToSingle(buffer);
                case ElemType.Double:
                    return BitConverter.ToDouble(buffer);
                case ElemType.Bool:
                    return buffer[0] > 0 ? 1.0 : 0.0;
                default:
                    return 0.0;
            }
        }
    }
}

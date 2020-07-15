﻿/*
 * Copyright 2020 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : ScadaServerEngine
 * Summary  : Represents a TCP listener which waits for client connections
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2020
 * Modified : 2020
 */

using Scada.Data.Models;
using Scada.Log;
using Scada.Protocol;
using System;
using static Scada.Protocol.ProtocolUtils;

namespace Scada.Server.Engine
{
    /// <summary>
    /// Represents a TCP listener which waits for client connections.
    /// <para>Представляет TCP-прослушиватель, который ожидает подключения клиентов.</para>
    /// </summary>
    internal class ServerListener : BaseListener
    {
        private readonly CoreLogic coreLogic; // the server logic instance


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ServerListener(CoreLogic coreLogic, ListenerOptions listenerOptions, ILog log)
            : base(listenerOptions, log)
        {
            this.coreLogic = coreLogic ?? throw new ArgumentNullException("coreLogic");
        }


        /// <summary>
        /// Gets the current data.
        /// </summary>
        protected void GetCurrentData(ConnectedClient client, DataPacket request, out ResponsePacket response)
        {
            byte[] buffer = request.Buffer;
            int index = ArgumentIndex;
            int cnlListID = BitConverter.ToInt32(buffer, index);
            CnlData[] cnlData;

            if (cnlListID > 0)
            {
                cnlData = coreLogic.GetCurrentData(cnlListID);
            }
            else
            {
                int[] cnlNums = GetIntArray(buffer, index + 8, out index);
                bool useCache = buffer[index] > 0;
                cnlData = coreLogic.GetCurrentData(cnlNums, useCache, out cnlListID);
            }

            byte[] outBuf = client.OutBuf;
            response = new ResponsePacket(request, outBuf);
            BitConverter.GetBytes(cnlData == null ? 0 : cnlListID).CopyTo(outBuf, ArgumentIndex);
            CopyCnlDataArray(cnlData, outBuf, ArgumentIndex + 4, out index);
            response.BufferLength = index;
            response.Encode();
        }

        /// <summary>
        /// Writes the current data.
        /// </summary>
        protected void WriteCurrentData(ConnectedClient client, DataPacket request, out ResponsePacket response)
        {
            byte[] buffer = request.Buffer;
            int index = ArgumentIndex;
            int deviceNum = BitConverter.ToInt32(buffer, index);
            int cnlCnt = BitConverter.ToInt32(buffer, index + 4);

            int[] cnlNums = new int[cnlCnt];
            CnlData[] cnlData = new CnlData[cnlCnt];

            for (int i = 0, idx1 = index + 8, idx2 = idx1 + cnlCnt * 4; i < cnlCnt; i++, idx1 += 4, idx2 += 12)
            {
                cnlNums[i] = BitConverter.ToInt32(buffer, idx1);
                cnlData[i] = new CnlData(
                    BitConverter.ToDouble(buffer, idx2),
                    BitConverter.ToInt32(buffer, idx2 + 8));
            }

            coreLogic.WriteCurrentData(deviceNum, cnlNums, cnlData);

            response = new ResponsePacket(request, client.OutBuf);
            response.Encode();
        }

        /// <summary>
        /// Gets the server name and version.
        /// </summary>
        protected override string GetServerName()
        {
            return Locale.IsRussian ?
                "Сервер " + ServerUtils.AppVersion :
                "Server " + ServerUtils.AppVersion;
        }

        /// <summary>
        /// Gets a value indicating whether the server is ready.
        /// </summary>
        protected override bool ServerIsReady()
        {
            return coreLogic.IsReady;
        }

        /// <summary>
        /// Validates the username and password.
        /// </summary>
        protected override bool ValidateUser(ConnectedClient client, string username, string password, string instance,
            out int roleID, out string errMsg)
        {
            // TODO: only the application role is acceptable
            client.IsLoggedIn = true;
            client.Username = username;

            roleID = 0;
            errMsg = "";
            return true;
        }

        /// <summary>
        /// Gets the directory name by ID.
        /// </summary>
        protected override string GetDirectory(ushort directoryID)
        {
            return @"C:\SCADA\";
        }

        /// <summary>
        /// Accepts or rejects the file upload.
        /// </summary>
        protected override bool AcceptFileUpload(string fileName)
        {
            return false;
        }

        /// <summary>
        /// Processes the incoming request.
        /// </summary>
        protected override void ProcessCustomRequest(ConnectedClient client, DataPacket request,
            out ResponsePacket response, out bool handled)
        {
            response = null;
            handled = true;

            switch (request.FunctionID)
            {
                case FunctionID.GetCurrentData:
                    GetCurrentData(client, request, out response);
                    break;

                case FunctionID.WriteCurrentData:
                    WriteCurrentData(client, request, out response);
                    break;

                default:
                    handled = false;
                    break;
            }
        }
    }
}

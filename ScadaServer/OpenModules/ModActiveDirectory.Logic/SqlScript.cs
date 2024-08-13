﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Scada.Server.Modules.ModActiveDirectory.Logic
{
    /// <summary>
    /// Contains SQL scripts.
    /// <para>Содержит SQL-скрипты.</para>
    /// </summary>
    internal static class SqlScript
    {
        private const string StartUserID = "1001";

        private const string Schema = "mod_active_directory";

        public const string CreateSchema =
            "CREATE SCHEMA IF NOT EXISTS " + Schema;

        public const string CreateUserTable =
            $"CREATE TABLE IF NOT EXISTS {Schema}.ad_user (" +
            $"ad_user_id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY (START {StartUserID})," +
            "username character varying NOT NULL," +
            "role_id integer NOT NULL," +
            "update_time time with time zone NOT NULL," +
            "CONSTRAINT ad_user_pkey PRIMARY KEY(ad_user_id)," +
            "CONSTRAINT ad_user_username_key UNIQUE(username))";

        public const string SelectUserByID =
            "SELECT ad_user_id, username, role_id " +
            $"FROM {Schema}.ad_user " +
            "WHERE ad_user_id = @adUserID";

        public const string SelectUserID =
            $"SELECT ad_user_id FROM {Schema}.ad_user WHERE username = @username";

        public const string UpdateUser =
            $"INSERT INTO {Schema}.ad_user (username, role_id, update_time) " +
            "VALUES (@username, @roleID, @updateTime) " +
            "ON CONFLICT (username) DO UPDATE " +
            "SET role_id = EXCLUDED.role_id, update_time = EXCLUDED.update_time";
    }
}

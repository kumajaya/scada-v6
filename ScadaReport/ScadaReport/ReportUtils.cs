﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Scada.Report
{
    /// <summary>
    /// The class contains utility methods for reports.
    /// <para>Класс, содержащий вспомогательные методы для отчётов.</para>
    /// </summary>
    public static class ReportUtils
    {
        /// <summary>
        /// Gets the report start time if not specified by a user.
        /// </summary>
        public static DateTime GetStartTime(DateTime utcNow, TimeZoneInfo timeZone, PeriodUnit unit)
        {
            ArgumentNullException.ThrowIfNull(timeZone, nameof(timeZone));
            DateTime startDate = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);

            startDate = unit == PeriodUnit.Month
                ? new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Local)
                : startDate.Date;

            return TimeZoneInfo.ConvertTimeToUtc(startDate, timeZone);
        }

        /// <summary>
        /// Normalizes the report time range.
        /// </summary>
        /// <remarks>
        /// Makes the startTime a left point of the time range, and makes the period positive.
        /// </remarks>
        public static void NormalizeTimeRange(ref DateTime startTime, ref int period, PeriodUnit unit)
        {
            if (startTime == DateTime.MinValue)
                throw new ArgumentException("Start time is not specified.", nameof(startTime));

            if (unit == PeriodUnit.Month)
            {
                if (period < 0)
                {
                    startTime = startTime.AddMonths(period);
                    period = -period;
                }
            }
            else
            {
                // Examples:
                // If the period is -1, 0 or 1, it means the single day, the startTime.
                // If the period is 2, it means 2 days starting from the startTime.
                // If the period is -2, it means 2 days ending with the startTime and including it.
                if (period <= -2)
                {
                    startTime = startTime.AddDays(period + 1);
                    period = -period;
                }
                else if (period < 1)
                {
                    period = 1;
                }
            }
        }

        /// <summary>
        /// Normalizes the report time range.
        /// </summary>
        public static void NormalizeTimeRange(ref DateTime startTime, ref DateTime endTime, ref int period, 
            PeriodUnit unit)
        {
            bool periodInMonths = unit == PeriodUnit.Month;

            if (startTime > DateTime.MinValue && endTime > DateTime.MinValue)
            {
                if (endTime < startTime)
                    endTime = startTime;
                period = periodInMonths ?
                    ((endTime.Year - startTime.Year) * 12) + endTime.Month - startTime.Month :
                    (int)(endTime - startTime).TotalDays + 1;
            }
            else if (startTime > DateTime.MinValue)
            {
                NormalizeTimeRange(ref startTime, ref period, unit);
                endTime = periodInMonths ?
                    startTime.AddMonths(period) :
                    startTime.AddDays(period - 1);
            }
            else if (endTime > DateTime.MinValue)
            {
                period = Math.Abs(period);
                NormalizeTimeRange(ref endTime, ref period, unit);
                startTime = periodInMonths ?
                    endTime.AddMonths(-period) :
                    endTime.AddDays(-period + 1);
            }
            else
            {
                throw new ArgumentException("Neither start time nor end time is not specified.");
            }
        }

        /// <summary>
        /// Builds a report file name to save or download.
        /// </summary>
        public static string BuildFileName(string prefix, DateTime generateTime, OutputFormat format)
        {
            return prefix + "_" +
                generateTime.ToLocalTime().ToString("yyyy-MM-dd_HH-mm-ss") + 
                format.GetExtension();
        }

        /// <summary>
        /// Gets the file extension, including the period, according to the output format.
        /// </summary>
        public static string GetExtension(this OutputFormat format)
        {
            return format switch
            {
                OutputFormat.Pdf => ".pdf",
                OutputFormat.Xml2003 => ".xml",
                OutputFormat.OpenXml => ".xlsx",
                OutputFormat.Html => ".html",
                _ => ""
            };
        }
    }
}

using System;
using System.Collections.Generic;

namespace Hrms.Common.AttendanceModels
{
    public partial class DeviceLogsInfo
    {
        public long DeviceLogId { get; set; }
        public DateTime DownloadDate { get; set; }
        public long DeviceId { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime LogDate { get; set; }
        public string? Direction { get; set; }
        public string? AttDirection { get; set; }
        public string? C1 { get; set; }
        public string? C2 { get; set; }
        public string? C3 { get; set; }
        public string? C4 { get; set; }
        public string? C5 { get; set; }
        public string? C6 { get; set; }
        public string? C7 { get; set; }
        public string? WorkCode { get; set; }
        public sbyte? HrappSyncstatus { get; set; }
    }
}

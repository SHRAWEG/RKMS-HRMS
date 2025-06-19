using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hrms.Common.AttendanceModels
{
    public partial class AttendanceSyncStatus
    {
        public long Id { get; set; }

        public long? DeviceLogId { get; set; }
    }
}

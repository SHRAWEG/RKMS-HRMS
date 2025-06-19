using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hrms.DatabaseWorker.AttendanceModels
{
    public partial class AttendanceSyncStatus
    {
        public long Id { get; set; }

        public long? DeviceLogId { get; set; }
    }
}

using System;
using System.Text.Json.Serialization;

namespace ToolApp.Models
{
    public class PomodoreRecord
    {
        public string Guid { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Remark { get; set; }
    }
}

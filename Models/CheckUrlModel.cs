using System.Collections.Generic;

namespace ToolApp.Models
{
    public class CheckUrlModel
    {
        public string Url { get; set; }
        public bool IsChecked { get; set; }
        public List<CheckUrlModel> CheckUrls { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ToolApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IStringLocalizer<IndexModel> _stringLocalizer;

        public IndexModel(ILogger<IndexModel> logger,IStringLocalizer<IndexModel> stringLocalizer)
        {
            _logger = logger;
            _stringLocalizer = stringLocalizer;
        }
        public void OnGet()
        {
            ViewData["CommonComparisonTable"] = _stringLocalizer["CommonComparisonTable"].Value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ToolApp.Pages
{
    public class ToggleCaseModel : PageModel
    {
        private readonly ILogger<ToggleCaseModel> _logger;

        public ToggleCaseModel(ILogger<ToggleCaseModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ToolApp.Pages
{
    public class GuidCreateModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;

        public GuidCreateModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        [HttpPost]
        public void OnPost(){
            ViewData["Guid"]=System.Guid.NewGuid().ToString();
        }
    }
}

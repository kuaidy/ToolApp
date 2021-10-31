using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ToolApp.Pages
{
    public class BinaryConversionModel : PageModel
    {
        private readonly ILogger<BinaryConversionModel> _logger;

        public BinaryConversionModel(ILogger<BinaryConversionModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}

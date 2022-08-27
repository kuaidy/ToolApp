using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ToolApp.Pages
{
    public class BinaryConvertModel : PageModel
    {
        private readonly ILogger<BinaryConvertModel> _logger;

        public BinaryConvertModel(ILogger<BinaryConvertModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}

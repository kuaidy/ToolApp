using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ToolApp.Pages
{
    public class ShowIpModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public ShowIpModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        public void OnGet()
        {
            WebRequest request = WebRequest.Create("https://ip.tool.lu/");
            WebResponse response=request.GetResponse();
            using(Stream dataStream=response.GetResponseStream()){
                StreamReader reader=new StreamReader(dataStream);
                string strResponse=reader.ReadToEnd();
                ViewData["ip"]=strResponse;
            }
        }
    }
}

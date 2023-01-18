using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Policy;
using Microsoft.Extensions.Configuration;

namespace ToolApp.Pages.QueryTools
{
    public class GetPreferentialProductsModel : PageModel
    {
        private readonly IConfiguration _Configuration;
        public GetPreferentialProductsModel(IConfiguration configuration) 
        {
            _Configuration=configuration;
        }
        public void OnGet()
        {
            ViewData["GetTaobaoGoods"] = _Configuration["AllianceService:GetTaobaoGoods"];
        }
    }
}

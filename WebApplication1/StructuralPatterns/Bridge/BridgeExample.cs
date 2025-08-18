using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using WebApplication1.StructuralPatterns.Bridge;

// Alias để tránh trùng với Microsoft.AspNetCore.Components.Web.HtmlRenderer (nếu có)
using BridgeHtmlRenderer = WebApplication1.StructuralPatterns.Bridge.HtmlRenderer;
using BridgePdfRenderer = WebApplication1.StructuralPatterns.Bridge.PdfRenderer;

namespace Design_Patterns.StructuralPatterns.Bridge
{
    // DTO cho Postman
    public sealed class ReportJobDto
    {
        // "sales" | "inventory"
        public string Type { get; set; } = "sales";
        // "html" | "pdf"
        public string Renderer { get; set; } = "html";

        // Cho Sales
        public List<decimal>? SalesData { get; set; }
        public string? CurrencyCulture { get; set; } = "en-US";

        // Cho Inventory
        public Dictionary<string, int>? Stock { get; set; }
    }

    public static class BridgeExample
    {
        public static object Run(List<ReportJobDto> jobs)
        {
            var results = new List<object>();

            foreach (var j in jobs)
            {
                try
                {
                    IReportRenderer renderer = j.Renderer?.ToLowerInvariant() switch
                    {
                        "pdf" => new BridgePdfRenderer(),
                        "html" => new BridgeHtmlRenderer(),
                        _ => throw new ArgumentException("Unsupported renderer: " + j.Renderer)
                    };

                    Report report = j.Type?.ToLowerInvariant() switch
                    {
                        "sales" => new SalesReport(
                            renderer,
                            j.SalesData ?? throw new ArgumentException("SalesData is required for sales report"),
                            j.CurrencyCulture ?? "en-US"
                        ),
                        "inventory" => new InventoryReport(
                            renderer,
                            j.Stock ?? throw new ArgumentException("Stock is required for inventory report")
                        ),
                        _ => throw new ArgumentException("Unsupported report type: " + j.Type)
                    };

                    var output = report.Export();

                    results.Add(new
                    {
                        type = j.Type,
                        renderer = j.Renderer,
                        title = (output.StartsWith("<html>", StringComparison.OrdinalIgnoreCase) ? "HTML" : "PDF") + " Output",
                        output
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        type = j.Type,
                        renderer = j.Renderer,
                        error = ex.Message
                    });
                }
            }

            return new
            {
                count = jobs.Count,
                results
            };
        }
    }
}

//GET http://localhost:5102/api/bridge/demo
//POST http://localhost:5102/api/bridge/export

/*[
  {
    "type": "sales",
    "renderer": "html",
    "currencyCulture": "en-US",
    "salesData": [100, 250.5, 75]
  },
  {
    "type": "sales",
    "renderer": "pdf",
    "currencyCulture": "vi-VN",
    "salesData": [1200000, 350000]
  },
  {
    "type": "inventory",
    "renderer": "html",
    "stock": { "A001": 50, "B002": 20, "C003": 0 }
  },
  {
    "type": "inventory",
    "renderer": "pdf",
    "stock": { "X01": 5, "Y02": 7 }
  }
]
[
  {
    "type": "sales",
    "renderer": "html",
    "currencyCulture": "en-US",
    "salesData": [100, 250.5, 75]
  },
  {
    "type": "sales",
    "renderer": "pdf",
    "currencyCulture": "vi-VN",
    "salesData": [1200000, 350000]
  },
  {
    "type": "inventory",
    "renderer": "html",
    "stock": { "A001": 50, "B002": 20, "C003": 0 }
  },
  {
    "type": "inventory",
    "renderer": "pdf",
    "stock": { "X01": 5, "Y02": 7 }
  }
]
*/
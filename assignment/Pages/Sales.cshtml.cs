using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using SalesForecastingApp.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text;

namespace SalesForecastingApp.Pages
{
    public class SalesModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly string? _connectionString;

        public SalesModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            States = new List<string>();
            BreakdownSalesData = new List<SalesData>();
            NoDataMessage = "";

        }

        [BindProperty]
        [Range(2000, int.MaxValue, ErrorMessage = "please enter a year starting from 2000.")]
        public int Year { get; set; } = 2000;

        [BindProperty]
        public decimal PercentageIncrease { get; set; }

        [BindProperty]
        public string? SelectedState { get; set; }
        public string NoDataMessage { get; set; }

        public List<SalesData>? SalesData { get; set; }
        public List<string> States { get; set; }
        

        public List<SalesData> BreakdownSalesData { get; set; }
        public decimal AggregatedSeedingYearSales { get; set; }
        public decimal AggregatedForecastedYearSales { get; set; }

        public void OnGet()
        {
            States = GetDistinctStates();
        }

        public void OnPost()
        {
            SalesData = GetYearlySalesWithIncrement(Year, PercentageIncrease, SelectedState);
            States = GetDistinctStates();
            if (SalesData.Count == 0)
            {
                NoDataMessage = $"No data available for the year {Year}.";

            }
            else
            {
                NoDataMessage = "";
            }
        }
        public IActionResult OnPostRefresh()
        {
            SalesData = null;
            Year = 0;
            PercentageIncrease = 0;
            States = GetDistinctStates();
            return Page();
        }

        public IActionResult OnPostExportToCsv()
        {
            var salesData = GetYearlySalesWithIncrement(Year, PercentageIncrease, SelectedState);

            var csv = new StringBuilder();
            csv.AppendLine("State,Percentage Increase,Sales Value");
            foreach (var item in salesData)
            {
                csv.AppendLine($"{item.State},{PercentageIncrease},{item.IncreasedSales}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "SalesForecast.csv");
        }

        private List<SalesData> GetYearlySalesWithIncrement(int year, decimal percentageIncrease, string? state)
        {
            var salesData = new List<SalesData>();

            if (string.IsNullOrEmpty(_connectionString))
                return salesData;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("GetYearlySalesWithIncrement", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Year", year);
                cmd.Parameters.AddWithValue("@OverallIncrementPercentage", percentageIncrease);
                cmd.Parameters.AddWithValue("@State", (object)state ?? DBNull.Value);
                cmd.CommandTimeout = 300;

                var stateIncrementTable = new DataTable();
                stateIncrementTable.Columns.Add("State", typeof(string));
                stateIncrementTable.Columns.Add("IncrementPercentage", typeof(decimal));
                if (!string.IsNullOrEmpty(state))
                {
                    stateIncrementTable.Rows.Add(state, percentageIncrease); // Add only the selected state
                }
                var stateIncrementParam = cmd.Parameters.AddWithValue("@StateIncrements", stateIncrementTable);
                stateIncrementParam.SqlDbType = SqlDbType.Structured;

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        salesData.Add(new SalesData
                        {
                            State = reader["State"].ToString() ?? string.Empty,
                            OriginalSales = reader.IsDBNull(reader.GetOrdinal("TotalSales")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalSales")),
                            IncreasedSales = reader.IsDBNull(reader.GetOrdinal("IncrementedSales")) ? 0 : reader.GetDecimal(reader.GetOrdinal("IncrementedSales"))
                        });
                    }
                }
            }
            AggregatedSeedingYearSales = salesData.Sum(s => s.OriginalSales);
            AggregatedForecastedYearSales = salesData.Sum(s => s.IncreasedSales);
            BreakdownSalesData = salesData;

            return salesData;
        }

        private List<string> GetDistinctStates()
        {
            var states = new List<string>();
            if (string.IsNullOrEmpty(_connectionString))
                return states;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT DISTINCT State FROM Orders$", conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        states.Add(reader["State"].ToString() ?? string.Empty);
                    }
                }
            }
            return states;
        }
    }

    public class SalesData
    {
        public string State { get; set; } = string.Empty;
        public decimal OriginalSales { get; set; }
        public decimal IncreasedSales { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace demo.Pages
{
    public class HomeModel : PageModel
    {
        private readonly string connectionString = "Server=localhost;Database=organizationdb;User=root;Password=Alwin#2001;";

        //public List<District> Districts { get; set; } = new List<District>();
        //public List<Taluka> Talukas { get; set; } = new List<Taluka>();



        //[BindProperty]
        //[Required(ErrorMessage = "Select District Must be required")]
        //public int? SelectedDistrictId { get; set; }

        //[BindProperty]
        //public int? SelectedTalukaId { get; set; }



        //public async Task<IActionResult> OnGetAsync()
        //{
        //    // Fetch districts from the database to populate the dropdown
        //    await FetchDistrictsAsync();
        //    return Page();
        //}




        //public async Task<JsonResult> OnGetTalukasAsync(int districtId)
        //{
        //    Talukas = await FetchTalukasByDistrictIdAsync(districtId);
        //    return new JsonResult(Talukas.Select(t => new { id = t.Id, name = t.Name }));
        //}


        //private async Task FetchDistrictsAsync()
        //{
        //    using (var connection = new MySqlConnection(connectionString))
        //    {
        //        await connection.OpenAsync();
        //        using (var command = new MySqlCommand("SELECT district_id, district_name FROM Districts", connection))
        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                Districts.Add(new District
        //                {
        //                    Id = reader.GetInt32("district_id"),
        //                    Name = reader.GetString("district_name")
        //                });
        //            }
        //        }
        //    }
        //}

        //private async Task<List<Taluka>> FetchTalukasByDistrictIdAsync(int districtId)
        //{
        //    var talukas = new List<Taluka>();

        //    using (var connection = new MySqlConnection(connectionString))
        //    {
        //        await connection.OpenAsync();
        //        using (var command = new MySqlCommand("SELECT taluka_id, taluka_name FROM Talukas WHERE district_id = @districtId", connection))
        //        {
        //            command.Parameters.AddWithValue("@districtId", districtId);

        //            using (var reader = await command.ExecuteReaderAsync())
        //            {
        //                while (await reader.ReadAsync())
        //                {
        //                    talukas.Add(new Taluka
        //                    {
        //                        Id = reader.GetInt32("taluka_id"),
        //                        Name = reader.GetString("taluka_name")
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return talukas;
        //}

        //public class District
        //{
        //    public int Id { get; set; }
        //    public string Name { get; set; }
        //}

        //public class Taluka
        //{
        //    public int Id { get; set; }
        //    public string Name { get; set; }
        //}

        
    }
}

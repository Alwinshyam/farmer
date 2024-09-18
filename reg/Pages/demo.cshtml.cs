using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace demo.Pages
{
    public class DemoModel : PageModel
    {
        private readonly string connectionString = "Server=localhost;Database=organizationdb;User=root;Password=6gcomm;";

        public List<District> Districts { get; set; } = new List<District>();
        public List<Taluka> Talukas { get; set; } = new List<Taluka>();

        [BindProperty]
        [Required(ErrorMessage = "Name Must be required")]
        public string Name { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Registration Number Must be required")]
        public string RegNo { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Address Must be required")]
        public string AddressLine1 { get; set; }

        [BindProperty]
        public string AddressLine2 { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "City Must be required")]
        public string City { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Pincode Must be required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be exactly 6 digits")]
        public string Pincode { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Select District Must be required")]
        public int? SelectedDistrictId { get; set; }

        [BindProperty]
        public int? SelectedTalukaId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Name Must be required")]
        public string CName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Mobile number Must be required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits")]
        public string CMobile { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email Id Must be required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string CEmail { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password Must be required")]
        public string Password { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Confirm Password Must be required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Fetch districts from the database to populate the dropdown
            await FetchDistrictsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate all required fields
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(RegNo) ||
                string.IsNullOrWhiteSpace(AddressLine1) || string.IsNullOrWhiteSpace(City) ||
                !SelectedDistrictId.HasValue || !SelectedTalukaId.HasValue || string.IsNullOrWhiteSpace(Pincode) ||
                string.IsNullOrWhiteSpace(CName) || string.IsNullOrWhiteSpace(CMobile) || string.IsNullOrWhiteSpace(CEmail) ||
                string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError(string.Empty, "Please fill all required fields.");
                await FetchDistrictsAsync();
                return Page();
            }

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand("sp_organizationdb", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters for the stored procedure
                        command.Parameters.Add(new MySqlParameter("icmd", MySqlDbType.VarChar) { Value = "I" }); // Assuming 'I' for insert
                        command.Parameters.Add(new MySqlParameter("ifpo_id", MySqlDbType.Int32) { Value = DBNull.Value }); // Assuming new entry, so no ID
                        command.Parameters.Add(new MySqlParameter("iorg_name", MySqlDbType.VarChar) { Value = Name });
                        command.Parameters.Add(new MySqlParameter("iorg_regno", MySqlDbType.VarChar) { Value = RegNo });
                        command.Parameters.Add(new MySqlParameter("iorg_a1", MySqlDbType.VarChar) { Value = AddressLine1 });
                        command.Parameters.Add(new MySqlParameter("iorg_a2", MySqlDbType.VarChar) { Value = AddressLine2 });
                        command.Parameters.Add(new MySqlParameter("iorg_city", MySqlDbType.VarChar) { Value = City });
                        command.Parameters.Add(new MySqlParameter("idistrict_id", MySqlDbType.Int32) { Value = SelectedDistrictId });
                        command.Parameters.Add(new MySqlParameter("italuka_id", MySqlDbType.Int32) { Value = SelectedTalukaId });
                        command.Parameters.Add(new MySqlParameter("iorg_pincode", MySqlDbType.VarChar) { Value = Pincode });
                        command.Parameters.Add(new MySqlParameter("iorg_cname", MySqlDbType.VarChar) { Value = CName });
                        command.Parameters.Add(new MySqlParameter("iorg_cmob", MySqlDbType.VarChar) { Value = CMobile });
                        command.Parameters.Add(new MySqlParameter("iorg_cmail", MySqlDbType.VarChar) { Value = CEmail });
                        command.Parameters.Add(new MySqlParameter("iorg_pname", MySqlDbType.VarChar) { Value = DBNull.Value }); // Assuming optional
                        command.Parameters.Add(new MySqlParameter("iorg_pmob", MySqlDbType.VarChar) { Value = DBNull.Value }); // Assuming optional
                        command.Parameters.Add(new MySqlParameter("iorg_pmail", MySqlDbType.VarChar) { Value = DBNull.Value }); // Assuming optional
                        command.Parameters.Add(new MySqlParameter("iorg_pwd", MySqlDbType.VarChar) { Value = Password });
                        
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // Redirect or show a success message
                return RedirectToPage("/Login");
                


            }
            catch (Exception ex)
            {
                // Log the error (uncomment ex variable name after DataAnnotations)
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request: " + ex.Message);
                await FetchDistrictsAsync();
                return Page();
            }
        }


        public async Task<JsonResult> OnGetTalukasAsync(int districtId)
        {
            Talukas = await FetchTalukasByDistrictIdAsync(districtId);
            return new JsonResult(Talukas.Select(t => new { id = t.Id, name = t.Name }));
        }

        private async Task FetchDistrictsAsync()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT district_id, district_name FROM Districts", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Districts.Add(new District
                        {
                            Id = reader.GetInt32("district_id"),
                            Name = reader.GetString("district_name")
                        });
                    }
                }
            }
        }

        private async Task<List<Taluka>> FetchTalukasByDistrictIdAsync(int districtId)
        {
            var talukas = new List<Taluka>();

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT taluka_id, taluka_name FROM Talukas WHERE district_id = @districtId", connection))
                {
                    command.Parameters.AddWithValue("@districtId", districtId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            talukas.Add(new Taluka
                            {
                                Id = reader.GetInt32("taluka_id"),
                                Name = reader.GetString("taluka_name")
                            });
                        }
                    }
                }
            }

            return talukas;
        }

        public class District
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Taluka
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Organization
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string RegNo { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string City { get; set; }
            public int? DistrictId { get; set; }
            public int? TalukaId { get; set; }
            public string Pincode { get; set; }
            public string CName { get; set; }
            public string CMobile { get; set; }
            public string CEmail { get; set; }
            public string Password { get; set; }
        }
    }
}

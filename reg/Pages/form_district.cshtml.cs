using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace reg.Pages
{
    public class form_districtModel : PageModel
    {
        //private readonly string _connectionString = "Server=localhost;Database=intern_alwin;User=root;Password=Alwin#2001;";

        private readonly string _connectionString = "Server=148.72.232.171;Database=intern_alwin;User=cog_intern;Password=intern@2024;";

        [BindProperty, Required]
        public string DistrictEn { get; set; } = string.Empty;

        [BindProperty, Required]
        public string DistrictTam { get; set; } = string.Empty;

        [BindProperty]
        public int? DistrictId { get; set; }

        [BindProperty]
        public int? StateId { get; set; }

        public IList<District> Districts { get; set; } = new List<District>();
        public List<State> States { get; set; } = new List<State>();

        public async Task<IActionResult> OnGetAsync()
        {
            States = await GetStatesAsync();
            Districts = await GetDistrictsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                States = await GetStatesAsync();
                return Page();
            }

            try
            {
                if (DistrictId.HasValue)
                {
                    await UpdateDistrictAsync(DistrictId.Value, DistrictEn, DistrictTam, StateId.Value);
                }
                else
                {
                    await SaveDistrictAsync(DistrictEn, DistrictTam, StateId.Value);
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                States = await GetStatesAsync();
                Districts = await GetDistrictsAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int districtId)
        {
            try
            {
                await DeleteDistrictAsync(districtId);
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while deleting the district: {ex.Message}");
                Districts = await GetDistrictsAsync();
                States = await GetStatesAsync();
                return Page();
            }
        }

        private async Task SaveDistrictAsync(string districtEn, string districtTam, int stateId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("sp_district", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("icmd", "I");
                    command.Parameters.AddWithValue("idistrict_id", DBNull.Value); // Use DBNull for null values
                    command.Parameters.AddWithValue("idistrict_name_en", districtEn);
                    command.Parameters.AddWithValue("idistrict_name_ta", districtTam);
                    command.Parameters.AddWithValue("istate_id", stateId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task UpdateDistrictAsync(int districtId, string districtEn, string districtTam, int stateId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("sp_district", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("icmd", "U");
                    command.Parameters.AddWithValue("idistrict_id", districtId);
                    command.Parameters.AddWithValue("idistrict_name_en", districtEn);
                    command.Parameters.AddWithValue("idistrict_name_ta", districtTam);
                    command.Parameters.AddWithValue("istate_id", stateId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task DeleteDistrictAsync(int districtId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("sp_district", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("icmd", "D");
                    command.Parameters.AddWithValue("idistrict_id", districtId);
                    command.Parameters.AddWithValue("idistrict_name_en", DBNull.Value);
                    command.Parameters.AddWithValue("idistrict_name_ta", DBNull.Value);
                    command.Parameters.AddWithValue("istate_id", DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<List<District>> GetDistrictsAsync()
        {
            var districts = new List<District>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("sp_district", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("icmd", "A");
                    command.Parameters.AddWithValue("idistrict_id", DBNull.Value);
                    command.Parameters.AddWithValue("idistrict_name_en", DBNull.Value);
                    command.Parameters.AddWithValue("idistrict_name_ta", DBNull.Value);
                    command.Parameters.AddWithValue("istate_id", DBNull.Value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            districts.Add(new District
                            {
                                DistrictId = reader.GetInt32("district_id"),
                                DistrictEn = reader.GetString("district_name_en"),
                                DistrictTam = reader.GetString("district_name_ta"),
                                StateId = reader.GetInt32("state_id"),
                                State = new State
                                {
                                    StateId = reader.GetInt32("state_id"),
                                    StateEn = reader.GetString("state_name_en") // Ensure this column exists
                                }
                            });
                        }
                    }
                }
            }
            return districts;
        }


        private async Task<List<State>> GetStatesAsync()
        {
            var states = new List<State>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT state_id, state_name_en FROM tbl_state", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            states.Add(new State
                            {
                                StateId = reader.GetInt32("state_id"),
                                StateEn = reader.GetString("state_name_en")
                            });
                        }
                    }
                }
            }
            return states;
        }

        public async Task<IActionResult> OnGetGetDistrictsAsync(int stateId)
        {
            var districts = new List<dynamic>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("sp_district", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("icmd", "S");
                    command.Parameters.AddWithValue("idistrict_id", DBNull.Value);
                    command.Parameters.AddWithValue("idistrict_name_en", DBNull.Value);
                    command.Parameters.AddWithValue("idistrict_name_ta", DBNull.Value);
                    command.Parameters.AddWithValue("istate_id", stateId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            districts.Add(new
                            {
                                DistrictId = reader.GetInt32(reader.GetOrdinal("district_id")),
                                DistrictEn = reader.GetString(reader.GetOrdinal("district_name_en")),
                                DistrictTam = reader.GetString(reader.GetOrdinal("district_name_ta")),
                                StateId = reader.GetInt32(reader.GetOrdinal("state_id")),
                                StateEn = reader.GetString(reader.GetOrdinal("state_name_en"))
                            });
                        }
                    }
                }
            }

            return new JsonResult(districts);
        }

        public class District
        {
            public int DistrictId { get; set; }
            public string DistrictEn { get; set; }
            public string DistrictTam { get; set; }
            public int StateId { get; set; }
            public State State { get; set; }
        }

        public class State
        {
            public int StateId { get; set; }
            public string StateEn { get; set; }
        }
    }
}

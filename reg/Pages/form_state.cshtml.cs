using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace reg.Pages
{
    public class form_stateModel : PageModel
    {
        private readonly string _connectionString = "Server=148.72.232.171;Database=intern_alwin;User=cog_intern;Password=intern@2024;";


        //private readonly string _connectionString = "Server=localhost;Database=intern_alwin;User=root;Password=Alwin#2001;";


        [BindProperty]
        public string StateEn { get; set; } = string.Empty;

        [BindProperty]
        public string StateTam { get; set; } = string.Empty;

        [BindProperty]
        public int? StateId { get; set; }

        public List<State> States { get; set; } = new List<State>();

        public async Task<IActionResult> OnGetAsync()
        {
            States = await GetStatesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                if (StateId.HasValue)
                {
                    await CallStoredProcedureAsync("U", StateId.Value, StateEn, StateTam);
                }
                else
                {
                    await CallStoredProcedureAsync("I", null, StateEn, StateTam);
                }

                States = await GetStatesAsync();
                return RedirectToPage(); // Redirect to refresh the page after successful update or save
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                // Log to verify deletion
                Console.WriteLine($"Deleting state with ID: {id}");

                await CallStoredProcedureAsync("D", id, null, null);

                States = await GetStatesAsync();
                return RedirectToPage(); // Redirect to refresh the page after successful delete
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while deleting the state: {ex.Message}");
                States = new List<State>(); // Optionally reset states list
                return Page();
            }
        }

        private async Task CallStoredProcedureAsync(string command, int? id, string stateEn, string stateTam)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new MySqlCommand("sp_state", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("icmd", command);
                    cmd.Parameters.AddWithValue("istate_id", id.HasValue ? (object)id.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("istate_name_en", stateEn ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("istate_name_ta", stateTam ?? (object)DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<List<State>> GetStatesAsync()
        {
            var states = new List<State>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new MySqlCommand("sp_state", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("icmd", "A"); // Command to fetch all states
                    cmd.Parameters.AddWithValue("istate_id", DBNull.Value); // Not needed for 'A' command
                    cmd.Parameters.AddWithValue("istate_name_en", DBNull.Value); // Not needed for 'A' command
                    cmd.Parameters.AddWithValue("istate_name_ta", DBNull.Value); // Not needed for 'A' command

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            states.Add(new State
                            {
                                StateId = reader.GetInt32("state_id"),
                                StateEn = reader.GetString("state_name_en"),
                                StateTam = reader.GetString("state_name_ta")
                            });
                        }
                    }
                }
            }
            return states;
        }


        public class State
        {
            public int StateId { get; set; }
            public string StateEn { get; set; }
            public string StateTam { get; set; }
        }

    }
}

public class form_talukaModel : PageModel
{
    private readonly string _connectionString = "Server=148.72.232.171;Database=intern_alwin;User=cog_intern;Password=intern@2024;";

    [BindProperty, Required]
    public string TalukaEn { get; set; } = string.Empty;

    [BindProperty, Required]
    public string TalukaTam { get; set; } = string.Empty;

    [BindProperty]
    public int? TalukaId { get; set; }

    [BindProperty]
    public int? DistrictId { get; set; }

    [BindProperty]
    public int? StateId { get; set; }

    public IList<Taluka> Talukas { get; set; } = new List<Taluka>();
    public List<State> States { get; set; } = new List<State>();
    public List<District> Districts { get; set; } = new List<District>();

    public async Task<IActionResult> OnGetAsync(int? stateId)
    {
        States = await GetStatesAsync();
        Talukas = await GetTalukasAsync();

        if (stateId.HasValue)
        {
            StateId = stateId;
            Districts = await GetDistrictsAsync(stateId.Value);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            States = await GetStatesAsync();
            Districts = await GetDistrictsAsync(StateId ?? 0);
            return Page();
        }

        try
        {
            if (TalukaId.HasValue)
            {
                await UpdateTalukaAsync(TalukaId.Value, TalukaEn, TalukaTam, DistrictId.Value, StateId.Value);
            }
            else
            {
                await SaveTalukaAsync(TalukaEn, TalukaTam, DistrictId.Value, StateId.Value);
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            States = await GetStatesAsync();
            Districts = await GetDistrictsAsync(StateId ?? 0);
            Talukas = await GetTalukasAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int talukaId)
    {
        try
        {
            await DeleteTalukaAsync(talukaId);
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while deleting the taluka: {ex.Message}");
            Talukas = await GetTalukasAsync();
            States = await GetStatesAsync();
            return Page();
        }
    }

    private async Task SaveTalukaAsync(string talukaEn, string talukaTam, int districtId, int stateId)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand("sp_taluka", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("icmd", "I");
                command.Parameters.AddWithValue("italuka_id", DBNull.Value); // Use DBNull for null values
                command.Parameters.AddWithValue("italuka_name_en", talukaEn);
                command.Parameters.AddWithValue("italuka_name_ta", talukaTam);
                command.Parameters.AddWithValue("idistrict_id", districtId);
                command.Parameters.AddWithValue("istate_id", stateId);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    private async Task UpdateTalukaAsync(int talukaId, string talukaEn, string talukaTam, int districtId, int stateId)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand("sp_taluka", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("icmd", "U");
                command.Parameters.AddWithValue("italuka_id", talukaId);
                command.Parameters.AddWithValue("italuka_name_en", talukaEn);
                command.Parameters.AddWithValue("italuka_name_ta", talukaTam);
                command.Parameters.AddWithValue("idistrict_id", districtId);
                command.Parameters.AddWithValue("istate_id", stateId);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    private async Task DeleteTalukaAsync(int talukaId)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand("sp_taluka", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("icmd", "D");
                command.Parameters.AddWithValue("italuka_id", talukaId);
                command.Parameters.AddWithValue("italuka_name_en", DBNull.Value);
                command.Parameters.AddWithValue("italuka_name_ta", DBNull.Value);
                command.Parameters.AddWithValue("idistrict_id", DBNull.Value);
                command.Parameters.AddWithValue("istate_id", DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    private async Task<List<Taluka>> GetTalukasAsync()
    {
        var talukas = new List<Taluka>();
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand(@"
                SELECT 
                    t.taluka_id, 
                    t.taluka_name_en, 
                    t.taluka_name_ta, 
                    t.district_id, 
                    t.state_id, 
                    s.state_name_en, 
                    d.district_name_en
                FROM 
                    tbl_taluka t
                JOIN 
                    tbl_state s ON t.state_id = s.state_id
                JOIN 
                    tbl_district d ON t.district_id = d.district_id", connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        talukas.Add(new Taluka
                        {
                            TalukaId = reader.GetInt32("taluka_id"),
                            TalukaEn = reader.GetString("taluka_name_en"),
                            TalukaTam = reader.GetString("taluka_name_ta"),
                            DistrictId = reader.GetInt32("district_id"),
                            StateId = reader.GetInt32("state_id"),
                            State = new State
                            {
                                StateId = reader.GetInt32("state_id"),
                                StateEn = reader.GetString("state_name_en")
                            },
                            District = new District
                            {
                                DistrictId = reader.GetInt32("district_id"),
                                DistrictEn = reader.GetString("district_name_en")
                            }
                        });
                    }
                }
            }
        }
        return talukas;
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

    public async Task<List<District>> GetDistrictsAsync(int stateId)
    {
        var districts = new List<District>();
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand("SELECT district_id, district_name_en FROM tbl_district WHERE state_id = @stateId", connection))
            {
                command.Parameters.AddWithValue("@stateId", stateId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        districts.Add(new District
                        {
                            DistrictId = reader.GetInt32("district_id"),
                            DistrictEn = reader.GetString("district_name_en")
                        });
                    }
                }
            }
        }
        return districts;
    }

    public class Taluka
    {
        public int TalukaId { get; set; }
        public string TalukaEn { get; set; }
        public string TalukaTam { get; set; }
        public int DistrictId { get; set; }
        public int StateId { get; set; }
        public District District { get; set; }
        public State State { get; set; }
    }

    public class District
    {
        public int DistrictId { get; set; }
        public string DistrictEn { get; set; }
    }

    public class State
    {
        public int StateId { get; set; }
        public string StateEn { get; set; }
    }
}

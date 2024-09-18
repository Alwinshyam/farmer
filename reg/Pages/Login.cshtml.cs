using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace demo.Pages
{
    public class LoginModel : PageModel
    {
        private readonly string connectionString = "Server=localhost;Database=organizationdb;User=root;Password=6gcomm;";

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate Username
            if (string.IsNullOrWhiteSpace(Username))
            {
                ModelState.AddModelError(string.Empty, "Username is required.");
                return Page();
            }

            try
            {
                // Connect to the database
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Prepare SQL command to validate user credentials
                    var query = "SELECT COUNT(*) FROM Organizations WHERE org_cmob = @username";
                    if (!string.IsNullOrWhiteSpace(Password))
                    {
                        query += " AND org_pwd = @password";
                    }

                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", Username);

                    if (!string.IsNullOrWhiteSpace(Password))
                    {
                        command.Parameters.AddWithValue("@password", Password);
                    }

                    var userCount = Convert.ToInt32(await command.ExecuteScalarAsync());

                    if (userCount > 0)
                    {
                        // Redirect to the home page upon successful login
                        return RedirectToPage("/Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid Username or Password.");
                        return Page();
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request: " + ex.Message);
                return Page();
            }
        }

        public void OnGet(string username = null, string password = null)
        {
            Username = username;
            Password = password;
        }
    }
}
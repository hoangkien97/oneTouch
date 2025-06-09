using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneTouch.Pages.User
{
    public class HomeModel : PageModel
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUserService _userService;

        public HomeModel(
            IAppointmentService appointmentService,
            IUserService userService)
        {
            _appointmentService = appointmentService;
            _userService = userService;
        }

        public string UserName { get; set; }
        public List<Appointment> UpcomingAppointments { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!int.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            UserName = user.FullName;

            // Get upcoming appointments
            UpcomingAppointments = await _appointmentService.GetUpcomingAppointmentsAsync(userId);

            return Page();
        }
    }

     
} 
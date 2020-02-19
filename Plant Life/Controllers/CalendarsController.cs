using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plant_Life.Data;
using Plant_Life.Models;
using Plant_Life.Models.ViewModel;

namespace Plant_Life.Controllers
{
    public class CalendarsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CalendarsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Calendars
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Calendar.Include(c => c.ApplicationUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Calendars/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calendar = await _context.Calendar
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calendar == null)
            {
                return NotFound();
            }

            return View(calendar);
        }

        // GET: Calendars/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Calendars/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ApplicationUserId,Id,PlantId,UserPlantId,StartDate,EndDate")] Calendar calendar)
        {
            if (ModelState.IsValid)
            {
                _context.Add(calendar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(calendar);
        }

        // GET: Calendars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calendar = await _context.Calendar.FindAsync(id);
            if (calendar == null)
            {
                return NotFound();
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.ApplicationUser, "Id", "Id", calendar.ApplicationUserId);
            return View(calendar);
        }

        // POST: Calendars/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ApplicationUserId,Id,PlantId,UserPlantId,StartDate,EndDate")] Calendar calendar)
        {
            if (id != calendar.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(calendar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CalendarExists(calendar.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(calendar);
        }

        // GET: Calendars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calendar = await _context.Calendar
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (calendar == null)
            {
                return NotFound();
            }

            return View(calendar);
        }

        // POST: Calendars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var calendar = await _context.Calendar.FindAsync(id);
            _context.Calendar.Remove(calendar);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CalendarExists(int id)
        {
            return _context.Calendar.Any(e => e.Id == id);
        }

        //step1:Get start date and days frequency
        //step2: Find out what day to put the first event on    
        //a. find out the difference in days between the today and the startdate   
        //b. find remainder given the days frequency 
        //c. if the remainder is zero- put the event on today; if it is 1 then put the remainder on the next day.
        //step3: create new event happening every x days from the start day and add as many as you want. 


        //looping through a date range using an interval
        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru, int interval)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(interval))
                yield return day;
        }

        public async Task<IActionResult> GetUserEvents()
        {
            /*try catch- when the home page loads and there are no events the calendar will still load and let the 
            user login, view the calendar, my plants, and plant list and add events. */

            var user = await GetCurrentUserAsync();
            var userEvents = new PlantIndexViewModel();
            try
            {
                userEvents.Events = new List<Event>();
                userEvents.Plants = _context.Plant.Where(e => e.ApplicationUserId == user.Id).ToList();
                userEvents.DefaultPlantUsers = _context.DefaultPlantUser.Where(e => e.ApplicationUserId == user.Id)
                    .Include(dp => dp.DefaultPlant).ToList();

                //looping through the plant with the watering event for that plant
                foreach (Plant p in userEvents.Plants)
                {
                    List<DateTime> WaterDates = new List<DateTime>();
                    int TimesperMonth = p.WaterNeeds;
                    int daysInYear = DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365;
                    int dayOfMonth = DateTime.Now.Day;
                    DateTime startDate = p.DateCreated;
                    DateTime lastDayOfCurrentYear = new DateTime(DateTime.Now.Year, 12, 31);
                       

                   
                    int DaysLeftInYear = daysInYear - dayOfMonth;
                    int WaterDayCount = DaysLeftInYear / TimesperMonth;

                    /*how the calendar loops through the date created to the last day of the month 
                    based on water needs(which is times per month)*/

                    foreach (DateTime day in EachDay(startDate, lastDayOfCurrentYear, p.WaterNeeds))
                    {
                        WaterDates.Add(day);
                    }

                    //creating watering events
                    //The forEach method executes a newWaterEvent function once for each WaterDate

                    for (int i = 0; i < WaterDates.Count; i++)
                    {
                        Event newWaterEvent = new Event()
                        {
                            ApplicationUserId = user.Id,
                            EventName = p.PlantName,
                            StartDate = WaterDates[i]
                        };
                        userEvents.Events.Add(newWaterEvent);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                return View("~/Views/Home/Index.cshtml");

            }


            return Json(userEvents.Events);
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

    }
}

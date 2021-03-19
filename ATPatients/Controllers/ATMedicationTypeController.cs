using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ATPatients.Models;
//Created By: Andrew Turner 7558596 Section 2
namespace ATPatients.Controllers
{
    public class ATMedicationTypeController : Controller
    {
        private readonly PatientsContext _context;

        //this contructor initializes the context
        public ATMedicationTypeController(PatientsContext context)
        {
            _context = context;
        }

        // GET: ATMedicationType
        //This Method Renders the view of the index page of the specified controller, will display in table format with crud actions
        //those crud actions will be links to the other actions of this controller
        public async Task<IActionResult> Index()
        {
            return View(await _context.MedicationType.OrderBy(m => m.Name).ToListAsync());
        }

        // GET: ATMedicationType/Details/5
        //This Method Renders the view of the details page to the specific specified id, will display in table format with information from data source
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicationType = await _context.MedicationType
                .FirstOrDefaultAsync(m => m.MedicationTypeId == id);
            if (medicationType == null)
            {
                return NotFound();
            }

            return View(medicationType);
        }

        // GET: ATMedicationType/Create
        //This Method Renders the view of the create page of the specified controller with the fields specific to the corresponding model
        public IActionResult Create()
        {
            return View();
        }

        // POST: ATMedicationType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //This method POSTS to the data source and creates a new record into the source.
        //Once created the new record is displayed similar to detials
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MedicationTypeId,Name")] MedicationType medicationType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(medicationType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(medicationType);
        }

        // GET: ATMedicationType/Edit/5
        //This method renders the edit page to allow the user to edit fields, the fields are listed as per the model
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicationType = await _context.MedicationType.FindAsync(id);
            if (medicationType == null)
            {
                return NotFound();
            }
            return View(medicationType);
        }

        // POST: ATMedicationType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //This method POSTS to the data source and updates an existing record from the source.
        //Once updated the existing record is displayed similar to detials
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MedicationTypeId,Name")] MedicationType medicationType)
        {
            if (id != medicationType.MedicationTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicationType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationTypeExists(medicationType.MedicationTypeId))
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
            return View(medicationType);
        }

        // GET: ATMedicationType/Delete/5
        //This method renders the delete page with the specified specific id with fields from data source
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicationType = await _context.MedicationType
                .FirstOrDefaultAsync(m => m.MedicationTypeId == id);
            if (medicationType == null)
            {
                return NotFound();
            }

            return View(medicationType);
        }

        // POST: ATMedicationType/Delete/5
        //This POST action takes the specified id of the record and deletes it then saves changes, redirects then to the index page
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicationType = await _context.MedicationType.FindAsync(id);
            _context.MedicationType.Remove(medicationType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //This method check is the aforementioned id exists, this is used for the edit POST method to check if it actually exists.
        private bool MedicationTypeExists(int id)
        {
            return _context.MedicationType.Any(e => e.MedicationTypeId == id);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ATPatients.Models;
using Microsoft.AspNetCore.Http;

namespace ATPatients.Controllers
{
    public class ATMedicationController : Controller
    {
        private readonly PatientsContext _context;

        public ATMedicationController(PatientsContext context)
        {
            _context = context;
        }

        // GET: ATMedication
        public async Task<IActionResult> Index(int? id, string name)
        {
            if (id != null)
            {
                HttpContext.Session.SetString("MedicationTypeId", id.ToString());
            }
            else if (HttpContext.Session.GetString("MedicationTypeId") != null)
            {
                var medID = HttpContext.Session.GetString("MedicationTypeId");
                id = Int32.Parse(medID);
            }
            else
            {
                TempData["message"] = "Please Select A Med Type";
                return RedirectToAction("Index", "ATMedicationType");
            }
            if (name != null)
            {
                HttpContext.Session.SetString("MedicationTypeName", name);
            }
            else if (HttpContext.Session.GetString("MedicationTypeName") != null)
            {
                name = HttpContext.Session.GetString("MedicationTypeName"); 
            }
            else
            {
                var getname = _context.MedicationType.Where(m => m.MedicationTypeId == id);
                foreach (var item in getname)
                {
                    name = item.Name;
                    HttpContext.Session.SetString("MedicationTypeName", name);
                }
            }
            ViewBag.Name = name;

            var patientsContext = _context.Medication.Include(m => m.ConcentrationCodeNavigation)
                .Include(m => m.DispensingCodeNavigation)
                .Include(m => m.MedicationType)
                .Where(m => m.MedicationTypeId == id)
                .OrderBy(x => x.Name == name)
                .ThenBy(x => x.ConcentrationCode);
            return View(await patientsContext.ToListAsync());
        }

        // GET: ATMedication/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medication
                .Include(m => m.ConcentrationCodeNavigation)
                .Include(m => m.DispensingCodeNavigation)
                .Include(m => m.MedicationType)
                .FirstOrDefaultAsync(m => m.Din == id);
            if (medication == null)
            {
                return NotFound();
            }
            string name = HttpContext.Session.GetString("MedicationTypeName");
            ViewBag.Name = name;
            return View(medication);
        }

        // GET: ATMedication/Create
        public IActionResult Create()
        {
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit.OrderBy(x => x.ConcentrationCode), "ConcentrationCode", "ConcentrationCode");
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit.OrderBy(x => x.DispensingCode), "DispensingCode", "DispensingCode");
            ViewData["MedicationTypeId"] = new SelectList(_context.MedicationType, "MedicationTypeId", "Name");
            string name = HttpContext.Session.GetString("MedicationTypeName");
            ViewBag.Name = name;
            return View();
        }

        // POST: ATMedication/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Din,Name,Image,MedicationTypeId,DispensingCode,Concentration,ConcentrationCode")] Medication medication)
        {
            
            if (ModelState.IsValid)
            {
                medication.MedicationTypeId = Int32.Parse(HttpContext.Session.GetString("MedicationTypeId"));
                bool recordExists = MedicationExistsExtended(medication.Name, medication.Concentration, medication.ConcentrationCode);
                
                if (recordExists)
                {
                    TempData["message"] = "Duplicate Record for MedTypeID:" + medication.MedicationTypeId
                        + " And Concentration:" + medication.Concentration
                        + " And ConcentrationCode:" + medication.ConcentrationCode;
                    return RedirectToAction("Create", "ATMedication");
                }
                _context.Add(medication);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit.OrderBy(x => x.ConcentrationCode), "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit.OrderBy(x => x.DispensingCode), "DispensingCode", "DispensingCode", medication.DispensingCode);
            ViewData["MedicationTypeId"] = new SelectList(_context.MedicationType, "MedicationTypeId", "Name", medication.MedicationTypeId);
            return View(medication);
        }

        // GET: ATMedication/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var medication = await _context.Medication.FindAsync(id);
            if (medication == null)
            {
                return NotFound();
            }
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit.OrderBy(x => x.ConcentrationCode), "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit.OrderBy(x => x.DispensingCode), "DispensingCode", "DispensingCode", medication.DispensingCode);
            ViewData["MedicationTypeId"] = new SelectList(_context.MedicationType, "MedicationTypeId", "Name", medication.MedicationTypeId);
            string name = HttpContext.Session.GetString("MedicationTypeName");
            ViewBag.Name = name;
            return View(medication);
        }

        // POST: ATMedication/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Din,Name,Image,MedicationTypeId,DispensingCode,Concentration,ConcentrationCode")] Medication medication)
        {
            if (id != medication.Din)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    medication.MedicationTypeId = Int32.Parse(HttpContext.Session.GetString("MedicationTypeId"));
                    
                    _context.Update(medication);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationExists(medication.Din))
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
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit.OrderBy(x => x.ConcentrationCode), "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit.OrderBy(x => x.DispensingCode), "DispensingCode", "DispensingCode", medication.DispensingCode);
            ViewData["MedicationTypeId"] = new SelectList(_context.MedicationType, "MedicationTypeId", "Name", medication.MedicationTypeId);
            return View(medication);
        }

        // GET: ATMedication/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medication
                .Include(m => m.ConcentrationCodeNavigation)
                .Include(m => m.DispensingCodeNavigation)
                .Include(m => m.MedicationType)
                .FirstOrDefaultAsync(m => m.Din == id);
            if (medication == null)
            {
                return NotFound();
            }
            string name = HttpContext.Session.GetString("MedicationTypeName");
            ViewBag.Name = name;
            return View(medication);
        }

        // POST: ATMedication/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var medication = await _context.Medication.FindAsync(id);
            _context.Medication.Remove(medication);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MedicationExists(string id)
        {
            return _context.Medication.Any(e => e.Din == id);
        }

        private bool MedicationExistsExtended(string name, double concentration, string concentrationCode)
        {
            return _context.Medication.Any(e => e.Name == name && e.Concentration == concentration && e.ConcentrationCode == concentrationCode);
        }

    }
}

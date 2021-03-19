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
    public class ATPatientTreatmentController : Controller
    {
        private readonly PatientsContext _context;

        public ATPatientTreatmentController(PatientsContext context)
        {
            _context = context;
        }

        // GET: ATPatientTreatment
        public async Task<IActionResult> Index(string id, string name, string patientid)
        {
            if (id != null)
            {
                HttpContext.Session.SetString("patientDiagnosisId", id);
            }
            else if (HttpContext.Session.GetString("patientDiagnosisId") != null)
            {
                id = HttpContext.Session.GetString("patientDiagnosisId");
            }
            else
            {
                TempData["Message"] = "Please Select A Diagnosis!";
                return RedirectToAction("Index", "ATPatientDiagnosis");
            }

            if (name != null)
            {
                HttpContext.Session.SetString("diagnosisName", name);
            }
            else if (HttpContext.Session.GetString("diagnosisName") != null)
            {
                name = HttpContext.Session.GetString("diagnosisName");
            }

            if (patientid != null)
            {
                HttpContext.Session.SetString("patientId", patientid);
            }
            else if (HttpContext.Session.GetString("patientId") != null)
            {
                patientid = HttpContext.Session.GetString("patientId");
            }
            var diagnosisIdSearch = _context.PatientDiagnosis.Where(d => d.PatientDiagnosisId == Convert.ToInt32(id)).FirstOrDefault();

            HttpContext.Session.SetString("diagnosisId", diagnosisIdSearch.DiagnosisId.ToString());

            var getFullName = _context.Patient.Where(a => a.PatientId == Convert.ToInt32(patientid)).FirstOrDefault();
            string fullName = getFullName.LastName + ", " + getFullName.FirstName; 
            HttpContext.Session.SetString("patientFullName", fullName);

            ViewData["FullName"] = HttpContext.Session.GetString("patientFullName");
            ViewData["DiagnosisTitle"] = HttpContext.Session.GetString("diagnosisName");

            var patientsContext = _context.PatientTreatment.Include(p => p.PatientDiagnosis).Include(p => p.Treatment).Where(p => p.PatientDiagnosis.PatientId == Convert.ToInt32(patientid) && p.PatientDiagnosis.Diagnosis.Name == name).OrderByDescending(p => p.DatePrescribed);
            return View(await patientsContext.ToListAsync());
        }

        // GET: ATPatientTreatment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["FullName"] = HttpContext.Session.GetString("patientFullName");
            ViewData["DiagnosisTitle"] = HttpContext.Session.GetString("diagnosisName");
            var patientTreatment = await _context.PatientTreatment
                .Include(p => p.PatientDiagnosis)
                .Include(p => p.Treatment)
                .FirstOrDefaultAsync(m => m.PatientTreatmentId == id);
            if (patientTreatment == null)
            {
                return NotFound();
            }

            return View(patientTreatment);
        }

        // GET: ATPatientTreatment/Create
        public IActionResult Create()
        {
            string id = HttpContext.Session.GetString("patientDiagnosisId");
            string diagnosisId = HttpContext.Session.GetString("diagnosisId"); 
            ViewData["FullName"] = HttpContext.Session.GetString("patientFullName");
            ViewData["DiagnosisTitle"] = HttpContext.Session.GetString("diagnosisName");

            TempData["Message1"] = DateTime.Now.ToString("dd MMMM yyyy HH:mm").ToString();
            
            ViewData["PatientDiagnosisId"] = new SelectList(_context.PatientDiagnosis, "PatientDiagnosisId", "PatientDiagnosisId");
            ViewData["TreatmentId"] = new SelectList(_context.Treatment.Where(p => p.DiagnosisId == Convert.ToInt32(diagnosisId)), "TreatmentId", "Name");
            return View();
        }

        // POST: ATPatientTreatment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientTreatmentId,TreatmentId,DatePrescribed,Comments,PatientDiagnosisId")] PatientTreatment patientTreatment)
        {
            string diagId = HttpContext.Session.GetString("patientDiagnosisId");
            string diagnosisId = HttpContext.Session.GetString("diagnosisId");
            ViewData["FullName"] = HttpContext.Session.GetString("patientFullName");
            ViewData["DiagnosisTitle"] = HttpContext.Session.GetString("diagnosisName");
            patientTreatment.DatePrescribed = DateTime.Now;
            patientTreatment.PatientDiagnosisId = Convert.ToInt32(diagId);
            if (ModelState.IsValid)
            {
                _context.Add(patientTreatment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PatientDiagnosisId"] = new SelectList(_context.PatientDiagnosis, "PatientDiagnosisId", "PatientDiagnosisId", patientTreatment.PatientDiagnosisId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatment.Where(p => p.DiagnosisId == Convert.ToInt32(diagnosisId)), "TreatmentId", "Name", patientTreatment.TreatmentId);
            return View(patientTreatment);
        }

        // GET: ATPatientTreatment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewData["FullName"] = HttpContext.Session.GetString("patientFullName");
            ViewData["DiagnosisTitle"] = HttpContext.Session.GetString("diagnosisName");
            string diagId = HttpContext.Session.GetString("patientDiagnosisId");
            string diagnosisId = HttpContext.Session.GetString("diagnosisId");
            var patientTreatment = await _context.PatientTreatment.FindAsync(id);
            if (patientTreatment == null)
            {
                return NotFound();
            }
            HttpContext.Session.SetString("DatePrescribed", patientTreatment.DatePrescribed.ToString());
            ViewData["PatientDiagnosisId"] = new SelectList(_context.PatientDiagnosis, "PatientDiagnosisId", "PatientDiagnosisId", patientTreatment.PatientDiagnosisId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatment.Where(p => p.DiagnosisId == Convert.ToInt32(diagnosisId)), "TreatmentId", "Name", patientTreatment.TreatmentId);
            return View(patientTreatment);
        }

        // POST: ATPatientTreatment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PatientTreatmentId,TreatmentId,DatePrescribed,Comments,PatientDiagnosisId")] PatientTreatment patientTreatment)
        {
            if (id != patientTreatment.PatientTreatmentId)
            {
                return NotFound();
            }
            string diagId = HttpContext.Session.GetString("patientDiagnosisId");
            string diagnosisId = HttpContext.Session.GetString("diagnosisId");
            ViewData["FullName"] = HttpContext.Session.GetString("patientFullName");
            ViewData["DiagnosisTitle"] = HttpContext.Session.GetString("diagnosisName");

            patientTreatment.DatePrescribed = Convert.ToDateTime(HttpContext.Session.GetString("DatePrescribed"));
            patientTreatment.PatientDiagnosisId = Convert.ToInt32(diagId);
            if (ModelState.IsValid)
            {
                try
                {
                    
                    _context.Update(patientTreatment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientTreatmentExists(patientTreatment.PatientTreatmentId))
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
            ViewData["PatientDiagnosisId"] = new SelectList(_context.PatientDiagnosis, "PatientDiagnosisId", "PatientDiagnosisId", patientTreatment.PatientDiagnosisId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatment.Where(p => p.DiagnosisId == Convert.ToInt32(diagnosisId)), "TreatmentId", "Name", patientTreatment.TreatmentId);
            return View(patientTreatment);
        }

        // GET: ATPatientTreatment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewData["FullName"] = HttpContext.Session.GetString("patientFullName");
            ViewData["DiagnosisTitle"] = HttpContext.Session.GetString("diagnosisName");

            var patientTreatment = await _context.PatientTreatment
                .Include(p => p.PatientDiagnosis)
                .Include(p => p.Treatment)
                .FirstOrDefaultAsync(m => m.PatientTreatmentId == id);
            if (patientTreatment == null)
            {
                return NotFound();
            }

            return View(patientTreatment);
        }

        // POST: ATPatientTreatment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewData["FullName"] = HttpContext.Session.GetString("patientFullName");
            ViewData["DiagnosisTitle"] = HttpContext.Session.GetString("diagnosisName");

            var patientTreatment = await _context.PatientTreatment.FindAsync(id);
            _context.PatientTreatment.Remove(patientTreatment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientTreatmentExists(int id)
        {
            return _context.PatientTreatment.Any(e => e.PatientTreatmentId == id);
        }
    }
}

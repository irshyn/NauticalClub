/*
 * The ISAnnualFeeStructureController class accesses and maintains records in the
 * AnnualFeeStructure table, and returns appropriate responses to user's requests
 * that allow them to view, create, edit or delete annual fee records in the Sail database
 * Assignment 2
 * Revision History
 *          Iryna Shynkevych 2018-10-01 : created
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ISSail.Models;

namespace ISSail.Controllers
{
    public class ISAnnualFeeStructureController : Controller
    {
        private readonly SailContext _context;
        // class constructor
        public ISAnnualFeeStructureController(SailContext context)
        {
            _context = context;
        }

        // Index action displays all the records in the AnnualFeeStructure table sorted be year
        // (the most recent first)
        public async Task<IActionResult> Index()
        {
            return View(await _context.AnnualFeeStructure.OrderByDescending(a => a.Year).ToListAsync());
        }

        // Details action displays all the fields of a specific record whose id was passed as a parameter
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var annualFeeStructure = await _context.AnnualFeeStructure
                .FirstOrDefaultAsync(m => m.Year == id);
            if (annualFeeStructure == null)
            {
                return NotFound();
            }

            return View(annualFeeStructure);
        }

        // Setup Create action offers a user a form to fill out whose inputs will be saved in the
        // database as a new AnnualFeeStructure record
        public IActionResult Create()
        {
            // to preload data from the most recent year, we retrieve it from the AnnualFeeStructure table
            var recentYearFee = _context.AnnualFeeStructure.OrderByDescending(a => a.Year).FirstOrDefault();
            return View(recentYearFee);
        }

        // Postback Create action saves the input made by user in the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Year,AnnualFee,EarlyDiscountedFee,EarlyDiscountEndDate,RenewDeadlineDate,TaskExemptionFee,SecondBoatFee,ThirdBoatFee,ForthAndSubsequentBoatFee,NonSailFee,NewMember25DiscountDate,NewMember50DiscountDate,NewMember75DiscountDate")] AnnualFeeStructure annualFeeStructure)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // add the record to the database and save the changes
                    _context.Add(annualFeeStructure);
                    await _context.SaveChangesAsync();
                    // if save was successful, display a message
                    TempData["message"] = $"Record added for year {annualFeeStructure.Year}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // in case of error, user will be returned back to the Create View page, his previous input
                    // preloaded in the fields, and a message describing the error will be displayed
                    ModelState.AddModelError("", $"error while saving annual fee structure: {ex.GetBaseException().Message}");
                    return View(annualFeeStructure);
                }
            }
            return View(annualFeeStructure);
        }

        // Setup Edit action displays the input for preloaded with a specific AnnualFeeStructure record
        // details passed in parameters to edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // to prevent edits to previous years
            if (id < DateTime.Now.Year)
            {
                TempData["message"] = "You cannot edit annual fees from previous years!";
                return RedirectToAction(nameof(Index));
            }

            var annualFeeStructure = await _context.AnnualFeeStructure.FindAsync(id);
            if (annualFeeStructure == null)
            {
                return NotFound();
            }
            return View(annualFeeStructure);
        }

        // Post-back Edit action saves the changes to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Year,AnnualFee,EarlyDiscountedFee,EarlyDiscountEndDate,RenewDeadlineDate,TaskExemptionFee,SecondBoatFee,ThirdBoatFee,ForthAndSubsequentBoatFee,NonSailFee,NewMember25DiscountDate,NewMember50DiscountDate,NewMember75DiscountDate")] AnnualFeeStructure annualFeeStructure)
        {
            if (id != annualFeeStructure.Year)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(annualFeeStructure);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnnualFeeStructureExists(annualFeeStructure.Year))
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
            return View(annualFeeStructure);
        }

        // Setup Delete action searches for the record based on its id passed in parameters,
        // and, if found, displays its details and asks the user to confirm the deletion
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var annualFeeStructure = await _context.AnnualFeeStructure
                .FirstOrDefaultAsync(m => m.Year == id);
            if (annualFeeStructure == null)
            {
                return NotFound();
            }

            return View(annualFeeStructure);
        }

        // If user confirms their intention to delete the recors, postback Delete action
        // removes it from the database and saves the database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var annualFeeStructure = await _context.AnnualFeeStructure.FindAsync(id);
            _context.AnnualFeeStructure.Remove(annualFeeStructure);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // AnnualFeeStructureExists function verifies if the specified AnnualFeeStructure record
        // exists in the context
        private bool AnnualFeeStructureExists(int id)
        {
            return _context.AnnualFeeStructure.Any(e => e.Year == id);
        }
    }
}

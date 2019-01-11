/*
 * The ISMemberController class accesses and maintains records in the
 * Member table, and returns appropriate responses to user's requests
 * that allow them to view, create, edit or delete member records 
 * in the Sail database
 * Assignment 4
 * Revision History
 *          Iryna Shynkevych 2018-11-25 : created
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
    public class ISMemberController : Controller
    {
        private readonly SailContext _context;
        // the class constructor
        public ISMemberController(SailContext context)
        {
            _context = context;
        }

        // Index action displays all of the records in the Member table sorted by full name
        public async Task<IActionResult> Index()
        {
            var sailContext = _context.Member.Include(m => m.ProvinceCodeNavigation).OrderBy(m => m.FullName);
            return View(await sailContext.ToListAsync());
        }

        // Details action displays detailed information on a specific record selected based on the id
        // passed in parameters
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .Include(m => m.ProvinceCodeNavigation)
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // Setup Create action displays an empty form for the user to fill out in order to
        // create a new Member record
        public IActionResult Create()
        {
            return View();
        }

        // Postback Create action verifies if the data entered by the user is valid, adds it
        // to the database, and then saves the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MemberId,FullName,FirstName,LastName,SpouseFirstName,SpouseLastName,Street,City,ProvinceCode,PostalCode,HomePhone,Email,YearJoined,Comment,TaskExempt,UseCanadaPost")] Member member)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(member);
                    await _context.SaveChangesAsync();
                    TempData["message"] = $"Record for: {member.FullName} successfully added";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.GetBaseException().Message}");
            }
            return View(member);
        }

        // Setup Edit action searches for the record based on its id, and, if successful, displays a form preloaded
        // wih its data for the user to edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            ViewData["ProvinceCode"] = new SelectList(_context.Province.OrderBy(pc => pc.Name), "ProvinceCode", "Name", member.ProvinceCode);
            return View(member);
        }

        // Postback Edit action saves the changes made by user to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MemberId,FullName,FirstName,LastName,SpouseFirstName,SpouseLastName,Street,City,ProvinceCode,PostalCode,HomePhone,Email,YearJoined,Comment,TaskExempt,UseCanadaPost")] Member member)
        {
            if (id != member.MemberId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                    TempData["message"] = $"Record updated for: {member.FullName}";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!MemberExists(member.MemberId))
                    {
                        ModelState.AddModelError("", $"Member under the name {member.FullName} is not on file");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Concurrency exception: {ex.GetBaseException().Message} ");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"update error: {ex.GetBaseException().Message}");
                }
            }
            ViewData["ProvinceCode"] = new SelectList(_context.Province.OrderBy(pc => pc.Name), "ProvinceCode", "Name", member.ProvinceCode);
            return View(member);
        }

        // Setup Delete action searches for the record selected by user based on its id, and prompts user
        // to confirm their intention to delete it
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .Include(m => m.ProvinceCodeNavigation)
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // Postback Delete action removes the selected record and saves changes to the database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var member = await _context.Member.FindAsync(id);
                _context.Member.Remove(member);
                await _context.SaveChangesAsync();
                TempData["message"] = $"Member '{member.FullName}' deleted from database";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["message"] = "Error on delete: " + ex.GetBaseException().Message;
            }
            return RedirectToAction("Delete", new { ID = id });
        }

        // Function MemberExists checks if a record already exists in the database based on its id
        private bool MemberExists(int id)
        {
            return _context.Member.Any(e => e.MemberId == id);
        }
    }
}
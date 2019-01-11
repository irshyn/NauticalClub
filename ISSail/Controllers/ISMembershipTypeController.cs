/*
 * The TAMembershipTypeController class accesses and maintains records in the membershipType
 * table of the Sail database, and returns appropriate responses to user's requests
 * that allow them to view, create, edit or delete membership type type records in the Sail database
 * Assignment 1
 * Revision History
 *          Iryna Shynkevych 2018-09-14 : created
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
    public class ISMembershipTypeController : Controller
    {
        private readonly SailContext _context;

        // Constructor for the TAMembershipTypeController using context
        //provided by the dependency injection
        public ISMembershipTypeController(SailContext context)
        {
            _context = context;
        }

        // Index is the default action for this controller that displays all
        // available membership types and provides links to the views of other
        // actions (create, edit, details and delete)
        public async Task<IActionResult> Index()
        {
            return View(await _context.MembershipType.OrderBy(a => a.MembershipTypeName).ToListAsync());
        }

        // The Details action returns the view displaying all fields of the selected 
        // membership type
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membershipType = await _context.MembershipType
                .FirstOrDefaultAsync(m => m.MembershipTypeName == id);
            if (membershipType == null)
            {
                return NotFound();
            }

            return View(membershipType);
        }

        // This setup Create action directs the user to the view with a blank
        // input page for the user to fill in
        public IActionResult Create()
        {
            return View();
        }

        // This post-back Create action (requiring Http Post) adds the new 
        // membership type record to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MembershipTypeName,Description,RatioToFull")] MembershipType membershipType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(membershipType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(membershipType);
        }

        // This setup Edit action displays the details of the selected
        // record for update
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membershipType = await _context.MembershipType.FindAsync(id);
            if (membershipType == null)
            {
                return NotFound();
            }
            return View(membershipType);
        }

        // This post-back Edit action (requiring Http Post) saves the updates 
        // to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MembershipTypeName,Description,RatioToFull")] MembershipType membershipType)
        {
            if (id != membershipType.MembershipTypeName)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(membershipType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MembershipTypeExists(membershipType.MembershipTypeName))
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
            return View(membershipType);
        }

        // This setup Delete action directs the user to the view that displays
        // the details of the selected membership type  record and asks them to
        // confirm the intention to delete it
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membershipType = await _context.MembershipType
                .FirstOrDefaultAsync(m => m.MembershipTypeName == id);
            if (membershipType == null)
            {
                return NotFound();
            }

            return View(membershipType);
        }

        // This post-back Delete action (requiring Http Post) deletes
        // the record specified by its id passed as argument from the database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var membershipType = await _context.MembershipType.FindAsync(id);
            _context.MembershipType.Remove(membershipType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Method verifying the existance of a membership type specified by its primary key
        private bool MembershipTypeExists(string id)
        {
            return _context.MembershipType.Any(e => e.MembershipTypeName == id);
        }
    }
}

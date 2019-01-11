/*
 * The ISMembershipController class accesses and maintains records in the
 * Membership table, and returns appropriate responses to user's requests
 * that allow them to view, create, edit or delete membership records 
 * for a specific member in the Sail database
 * Assignment 2
 * Revision History
 *          Iryna Shynkevych 2018-10-01 : created
 */
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using ISSail.Models;

namespace ISSail.Controllers
{
    public class ISMembershipController : Controller
    {
        private readonly SailContext _context;
        // the class constructor
        public ISMembershipController(SailContext context)
        {
            _context = context;
        }

        // Index action displays memberships for a specific member whose
        // id was passed in parameters or retrieved from a session variable
        public async Task<IActionResult> Index()
        {
            string memberId = "";
            string memberName = "";

            // look for the member id in the query string; if found, persist it 
            if (Request.Query["memberId"].Count != 0)
            {
                memberId = Request.Query["memberId"];
                HttpContext.Session.SetString("memberId", memberId);
            }
            //if not, look for it in a session variable
            else
            {
                if (HttpContext.Session.GetString(nameof(memberId)) != null)
                {
                    memberId = HttpContext.Session.GetString(nameof(memberId));
                }
            }
            //if not found there either, return to the ISMember Index view with a message
            // prompting the user to select the member whose memberships they wish to see
            if (memberId == "")
            {
                TempData["message"] = "Please, select a member to see their membership";
                return RedirectToAction("Index", "ISMember");
            }

            // look for member's name in the query string
            if (Request.Query["memberName"].Count != 0)
            {
                memberName = Request.Query["memberName"];
                HttpContext.Session.SetString("memberName", memberName);
            }
            // if not found, look for the member's name in the Members table based on memberId
            else
            {
                memberName = _context.Member.FirstOrDefault(m => m.MemberId == int.Parse(memberId)).FullName;
            }
            // persist member name
            HttpContext.Session.SetString("memberName", memberName);

            // save member name to the ViewBag
            ViewBag.MemberFullName = memberName;

            // display the membership information filtered by memberId and sorted by year
            var sailContext = _context.Membership.Include(m => m.Member)
                .Include(m => m.MembershipTypeNameNavigation)
                .Where(m => m.MemberId == int.Parse(memberId))
                .OrderByDescending(m => m.Year);

            return View(await sailContext.ToListAsync());
        }

        // Details action displays detailed information of a specific record selected based on 
        // its id passed as a parameter
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membership = await _context.Membership
                .Include(m => m.Member)
                .Include(m => m.MembershipTypeNameNavigation)
                .FirstOrDefaultAsync(m => m.MembershipId == id);
            if (membership == null)
            {
                return NotFound();
            }

            return View(membership);
        }

        // Setup Create action displays a form for user to fill out in order to create a new
        // membership
        public IActionResult Create()
        {
            // for a year drop-down (to restrict the choice down to years for which we have annual fees)
            ViewData["MembershipYear"] = new SelectList(_context.AnnualFeeStructure.OrderByDescending(m => m.Year), "Year", "Year");
            // for the membership type name drop-down sorted by name
            ViewData["MembershipTypeName"] = new SelectList(_context.MembershipType.OrderBy(m => m.MembershipTypeName), "MembershipTypeName", "MembershipTypeName");
            return View();
        }

        // Post-back Create action saving the new Membership record to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MembershipId,MemberId,Year,MembershipTypeName,Fee,Comments,Paid")] Membership membership)
        {
            double annualFee = 0;
            double ratioToFull = 0;

            // if there are no errors in the membership data entered by user
            if (ModelState.IsValid)
            {
                // we retrieve the annual fee for the specified year from the AnnualFeeStructure table
                if (_context.AnnualFeeStructure
                    .SingleOrDefault(a => a.Year == membership.Year)
                    .AnnualFee != null)
                {
                    annualFee = (double)_context.AnnualFeeStructure
                        .SingleOrDefault(a => a.Year == membership.Year)
                        .AnnualFee;
                }
                // we retrieve the ratio to full for the specified membership type from the MembershipType table
                ratioToFull = _context.MembershipType.SingleOrDefault(a =>
                a.MembershipTypeName == membership.MembershipTypeName).RatioToFull;

                // we calculate the membership fee based on annual fee and ration to full
                membership.Fee = annualFee * ratioToFull;
                // and save the modified record to the database
                _context.Add(membership);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // for a year drop-down (to restrict the choice down to years for which we have annual fees)
            ViewData["MembershipYear"] = new SelectList(_context.AnnualFeeStructure.OrderByDescending(m => m.Year), "Year", "Year");
            ViewData["MembershipTypeName"] = new SelectList(_context.MembershipType.OrderBy(m => m.MembershipTypeName), "MembershipTypeName", "MembershipTypeName", membership.MembershipTypeName);
            return View(membership);
        }

        // Setup Edit action displays the form for a specific membership record preloaded with its current
        // data for the user to edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // to prevent the user to change records from previous years

            var membership = await _context.Membership.FindAsync(id);
            if (membership == null)
            {
                return NotFound();
            }
            if (membership.Year < DateTime.Now.Year)
            {
                TempData["message"] = "You cannot edit previous year's membership records!";
                return RedirectToAction("Index");
            }
            // for a year drop-down (to restrict the choice down to years for which we have annual fees)
            ViewData["MembershipYear"] = new SelectList(_context.AnnualFeeStructure.OrderByDescending(m => m.Year), "Year", "Year");
            ViewData["MembershipTypeName"] = new SelectList(_context.MembershipType.OrderBy(m => m.MembershipTypeName), "MembershipTypeName", "MembershipTypeName", membership.MembershipTypeName);
            return View(membership);
        }

        //Postback Edit action that saves the changes made by user into the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MembershipId,MemberId,Year,MembershipTypeName,Fee,Comments,Paid")] Membership membership)
        {
            if (id != membership.MembershipId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(membership);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MembershipExists(membership.MembershipId))
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
            // for a year drop-down (to restrict the choice down to years for which we have annual fees)
            ViewData["MembershipYear"] = new SelectList(_context.AnnualFeeStructure.OrderByDescending(m => m.Year), "Year", "Year");
            ViewData["MembershipTypeName"] = new SelectList(_context.MembershipType.OrderBy(m => m.MembershipTypeName), "MembershipTypeName", "MembershipTypeName", membership.MembershipTypeName);
            return View(membership);
        }

        // Setup Delete action displays details of a specific membership record retrieved based
        // on its id and prompts the user to confirm their intention to delete the record
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membership = await _context.Membership
                .Include(m => m.Member)
                .Include(m => m.MembershipTypeNameNavigation)
                .FirstOrDefaultAsync(m => m.MembershipId == id);
            if (membership == null)
            {
                return NotFound();
            }

            return View(membership);
        }

        // The postback Delete action removes the record and saves the changes in the database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var membership = await _context.Membership.FindAsync(id);
            _context.Membership.Remove(membership);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // The MembershipExists function checks if the record already exists in the database based on
        // its id
        private bool MembershipExists(int id)
        {
            return _context.Membership.Any(e => e.MembershipId == id);
        }
    }
}

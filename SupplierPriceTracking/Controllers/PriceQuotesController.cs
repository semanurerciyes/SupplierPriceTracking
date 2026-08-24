using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupplierPriceTracking.Data;
using SupplierPriceTracking.Models;

namespace SupplierPriceTracking.Controllers
{
    [Authorize] // Giriş yapmayan kimse erişemez
    public class PriceQuotesController : Controller
    {
        private readonly AppDbContext _context;

        public PriceQuotesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PriceQuotes (Admin & Viewer)
        public async Task<IActionResult> Index(string sortOrder, string searchString, DateTime? startDate, DateTime? endDate, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["MaterialSortParm"] = String.IsNullOrEmpty(sortOrder) ? "material_desc" : "";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            ViewData["CurrentFilter"] = searchString;
            ViewData["StartDateFilter"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDateFilter"] = endDate?.ToString("yyyy-MM-dd");

            var priceQuotesQuery = _context.PriceQuotes
                .Include(p => p.Material)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                priceQuotesQuery = priceQuotesQuery.Where(pq =>
                    pq.Material.Name.Contains(searchString) ||
                    pq.Supplier.Name.Contains(searchString));
            }

            if (startDate.HasValue)
            {
                priceQuotesQuery = priceQuotesQuery.Where(pq => pq.ValidFrom >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                priceQuotesQuery = priceQuotesQuery.Where(pq => pq.ValidTo <= endDate.Value);
            }

            switch (sortOrder)
            {
                case "material_desc":
                    priceQuotesQuery = priceQuotesQuery.OrderByDescending(pq => pq.Material.Name);
                    break;
                case "Price":
                    priceQuotesQuery = priceQuotesQuery.OrderBy(pq => pq.Price);
                    break;
                case "price_desc":
                    priceQuotesQuery = priceQuotesQuery.OrderByDescending(pq => pq.Price);
                    break;
                case "Date":
                    priceQuotesQuery = priceQuotesQuery.OrderBy(pq => pq.ValidFrom);
                    break;
                case "date_desc":
                    priceQuotesQuery = priceQuotesQuery.OrderByDescending(pq => pq.ValidFrom);
                    break;
                default:
                    priceQuotesQuery = priceQuotesQuery.OrderBy(pq => pq.Material.Name);
                    break;
            }

            int pageSize = 10;
            int pageIndex = pageNumber ?? 1;

            int totalItems = await priceQuotesQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages > 0 ? totalPages : 1;
            ViewBag.HasPreviousPage = pageIndex > 1;
            ViewBag.HasNextPage = pageIndex < totalPages;

            var pagedList = await priceQuotesQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(pagedList);
        }

        // GET: PriceQuotes/ExportToCsv (Admin & Viewer)
        public async Task<IActionResult> ExportToCsv(string searchString, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.PriceQuotes
                .Include(p => p.Material)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                query = query.Where(pq => pq.Material.Name.Contains(searchString) || pq.Supplier.Name.Contains(searchString));
            }
            if (startDate.HasValue)
            {
                query = query.Where(pq => pq.ValidFrom >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(pq => pq.ValidTo <= endDate.Value);
            }

            var quotes = await query.ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Material;Supplier;Price;Currency;Valid From;Valid To;Created Date");

            foreach (var quote in quotes)
            {
                builder.AppendLine($"{quote.Material?.Name};{quote.Supplier?.Name};{quote.Price};{quote.Currency};{quote.ValidFrom:dd.MM.yyyy};{quote.ValidTo:dd.MM.yyyy};{quote.CreatedDate:dd.MM.yyyy HH:mm}");
            }

            var preamble = Encoding.UTF8.GetPreamble();
            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            var fileBytes = preamble.Concat(bytes).ToArray();

            return File(fileBytes, "text/csv; charset=utf-8", $"PriceQuotes_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        // GET: PriceQuotes/Details/5 (Admin & Viewer)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var priceQuote = await _context.PriceQuotes
                .Include(p => p.Material)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (priceQuote == null) return NotFound();

            return View(priceQuote);
        }

        // GET: PriceQuotes/Create (Sadece Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["MaterialId"] = new SelectList(_context.Materials, "Id", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name");
            return View();
        }

        // POST: PriceQuotes/Create (Sadece Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,MaterialId,SupplierId,Price,Currency,ValidFrom,ValidTo")] PriceQuote priceQuote)
        {
            if (priceQuote.ValidTo < priceQuote.ValidFrom)
            {
                ModelState.AddModelError("ValidTo", "Valid To date cannot be earlier than Valid From date.");
            }

            bool hasOverlap = await _context.PriceQuotes.AnyAsync(pq =>
                pq.MaterialId == priceQuote.MaterialId &&
                pq.SupplierId == priceQuote.SupplierId &&
                priceQuote.ValidFrom <= pq.ValidTo &&
                priceQuote.ValidTo >= pq.ValidFrom);

            if (hasOverlap)
            {
                ModelState.AddModelError(string.Empty, "A price quote already exists for this material and supplier within the selected date range.");
            }

            if (ModelState.IsValid)
            {
                priceQuote.CreatedDate = DateTime.Now;
                _context.Add(priceQuote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaterialId"] = new SelectList(_context.Materials, "Id", "Name", priceQuote.MaterialId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", priceQuote.SupplierId);
            return View(priceQuote);
        }

        // GET: PriceQuotes/Edit/5 (Sadece Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var priceQuote = await _context.PriceQuotes.FindAsync(id);
            if (priceQuote == null) return NotFound();

            ViewData["MaterialId"] = new SelectList(_context.Materials, "Id", "Name", priceQuote.MaterialId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", priceQuote.SupplierId);
            return View(priceQuote);
        }

        // POST: PriceQuotes/Edit/5 (Sadece Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaterialId,SupplierId,Price,Currency,ValidFrom,ValidTo,CreatedDate")] PriceQuote priceQuote)
        {
            if (id != priceQuote.Id) return NotFound();

            if (priceQuote.ValidTo < priceQuote.ValidFrom)
            {
                ModelState.AddModelError("ValidTo", "Valid To date cannot be earlier than Valid From date.");
            }

            bool hasOverlap = await _context.PriceQuotes.AnyAsync(pq =>
                pq.Id != priceQuote.Id &&
                pq.MaterialId == priceQuote.MaterialId &&
                pq.SupplierId == priceQuote.SupplierId &&
                priceQuote.ValidFrom <= pq.ValidTo &&
                priceQuote.ValidTo >= pq.ValidFrom);

            if (hasOverlap)
            {
                ModelState.AddModelError(string.Empty, "A price quote already exists for this material and supplier within the selected date range.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(priceQuote);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PriceQuoteExists(priceQuote.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaterialId"] = new SelectList(_context.Materials, "Id", "Name", priceQuote.MaterialId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", priceQuote.SupplierId);
            return View(priceQuote);
        }

        // GET: PriceQuotes/Delete/5 (Sadece Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var priceQuote = await _context.PriceQuotes
                .Include(p => p.Material)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (priceQuote == null) return NotFound();

            return View(priceQuote);
        }

        // POST: PriceQuotes/Delete/5 (Sadece Admin)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var priceQuote = await _context.PriceQuotes.FindAsync(id);
            if (priceQuote != null)
            {
                _context.PriceQuotes.Remove(priceQuote);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PriceQuoteExists(int id)
        {
            return _context.PriceQuotes.Any(e => e.Id == id);
        }
    }
}
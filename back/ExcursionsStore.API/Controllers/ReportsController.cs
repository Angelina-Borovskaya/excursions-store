using ExcursionsStore.API.Data;
using ExcursionsStore.API.DTOs.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ExcursionsStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("sales")]
        public async Task<IActionResult> GenerateSalesReport([FromBody] SalesReportRequestDto dto)
        {
            var startUtc = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
            var endUtc   = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc);
            // === ПРОДАЖИ ===
           var sales = await _context.Orders
                .Where(o =>
                    o.Status == "Завершён" &&
                    o.OrderDate >= startUtc &&
                    o.OrderDate <= endUtc)
                .Include(o => o.Tour)
                    .ThenInclude(t => t!.City)
                        .ThenInclude(c => c!.Country)
                .ToListAsync();


            // Группировка по турам
            var salesByTour = sales
                .GroupBy(o => o.Tour!)
                .Select(g => new
                {
                    TourTitle = g.Key.Title,
                    Count = g.Count(),
                    Total = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Группировка по направлениям
            var salesByDestination = sales
                .GroupBy(o => new
                {
                    Country = o.Tour!.City!.Country!.Name,
                    City = o.Tour.City.Name
                })
                .Select(g => new
                {
                    g.Key.Country,
                    g.Key.City,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // === PDF ===
            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text("Отчёт о продажах")
                        .FontSize(20)
                        .SemiBold()
                        .AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Spacing(20);

                        col.Item().Text(
                            $"Период: {startUtc:dd.MM.yyyy} – {endUtc:dd.MM.yyyy}");

                        // ===== Таблица продаж =====
                        col.Item().Text("Продажи по экскурсиям").FontSize(16).SemiBold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Экскурсия").SemiBold();
                                header.Cell().Text("Кол-во").SemiBold();
                                header.Cell().Text("Сумма").SemiBold();
                            });

                            foreach (var item in salesByTour)
                            {
                                table.Cell().Text(item.TourTitle);
                                table.Cell().Text(item.Count.ToString());
                                table.Cell().Text(item.Total.ToString("N0"));
                            }
                        });

                        // ===== Таблица направлений =====
                        col.Item().Text("Популярные направления").FontSize(16).SemiBold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Страна").SemiBold();
                                header.Cell().Text("Город").SemiBold();
                                header.Cell().Text("Заказов").SemiBold();
                            });

                            foreach (var item in salesByDestination)
                            {
                                table.Cell().Text(item.Country);
                                table.Cell().Text(item.City);
                                table.Cell().Text(item.Count.ToString());
                            }
                        });
                    });

                    page.Footer().AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Сформировано: ");
                            x.Span(DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm"));
                        });
                });
            });

            var pdfBytes = pdf.GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"sales-report-{startUtc:yyyyMMdd}-{endUtc:yyyyMMdd}.pdf"
            );
        }
    }
}

using AdminApps.DAL;
using AdminApps.DAL.Services;
using AdminApps.Helpers;
using AdminApps.Models;
using AdminApps.ViewModels;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.fonts;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AdminApps.Controllers
{
    [Authorize(Roles = "BillsClerc, BillsDept")]
    public class BillReportsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BillReports
        public ActionResult Index()
        {
            return View();
        }

        // GET: BillReports/DocumentsViewsList
        public ActionResult ALSR()
        {
            var reportService = new BillsAlsrReportsService();
            var reports = reportService.GetAll();

            return View(reports);
        }

        // GET: BillReports/ALSR
        public async Task<ActionResult> GenerateALSR()
        {
            var user = await ReturnCurrentUserAsync();
            var reviews = await db.BillReviews
                                    .Where(r => 
                                        r.Bill.LegStatusID == 1 && //select only Pending Bills
                                        r.DeptID == user.DeptID && 
                                        r.Approvals.Where(a => a.ApprovalLevel == 1).Any() &&
                                        r.BillVersionID == db.BillVersions.Where(v => v.BillID == r.BillID).OrderByDescending(v => v.VersionNum).FirstOrDefault().ID)
                                    .Include(r => r.Bill)
                                    .Include(r => r.CreatedByUser)
                                    .Include(r => r.Recommendation)
                                    .OrderBy(r => r.Bill.BillPrefixID).ThenBy(r => r.Bill.Suffix)
                                    .ToListAsync();
            var viewModel = new AlsrReportSelectBillReviewsViewModel();
            foreach (BillReview r in reviews)
            {
                var x = new AlsrReportIndividualBillReviewViewModel();
                x.ID = r.ID;
                x.CompositeBillNumber = r.Bill.CompositeBillNumber;
                x.VersionNum = r.BillVersion.VersionNum;
                x.CreatedByUserFullName = r.CreatedByUser.FullName;
                x.Recommendation = r.Recommendation.Description;
                x.Selected = true;
                viewModel.BillReviews.Add(x);
            }
            return View(viewModel);
        }

        // POST: BillReports/ALSR
        [HttpPost]
        public async Task<ActionResult> GenerateALSR(AlsrReportSelectBillReviewsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var selectedIds = model.getSelectedIds();

                if (selectedIds.Count() == 0)
                {
                    Danger("No Bills were selected for inclusion in the report.");
                    return View();
                }

                var selectedBillReviews = from x in db.BillReviews
                                                        .Include(r => r.Bill)
                                                        .Include(r => r.CreatedByUser)
                                                        .Include(r => r.Recommendation)
                                                    where selectedIds.Contains(x.ID)
                                                    select x;

                var report = new BillsAlsrReport();
                report.BudgetPeriodID = (await db.AppGlobalSettings.FirstOrDefaultAsync()).BudgetPeriodID;
                report.BudgetSessionID = (await db.AppGlobalSettings.FirstOrDefaultAsync()).BudgetSessionID;
                report.CreatedAt = DateTime.Now;
                report.GovOfficeDeliveryDate = model.GovOfficeDeliveryDate;

                var user = await ReturnCurrentUserAsync();
                report.DeptID = user.DeptID;
                report.DivID = user.DivID;

                //var snapshots = new List<AlsrBillReviewSnapshot>();
                foreach (BillReview r in selectedBillReviews.ToList())
                {
                    var s = new AlsrBillReviewSnapshot()
                        {
                            ActivelyTracking = r.ActivelyTracking,
                            ApplicationUserID = r.ApplicationUserID,
                            BillID = r.BillID,
                            BillReviewRecommendationID = r.BillReviewRecommendationID,
                            BillVersionID = r.BillVersionID,
                            CapturedFromBillReviewID = r.ID,
                            CapturedFromRowVersion = r.RowVersion,
                            Comments = r.Comments,
                            CreatedAt = r.CreatedAt,
                            CreatedAtApprovalLevel = r.CreatedAtApprovalLevel,
                            DeptID = r.DeptID,
                            DivID = r.DivID,
                            FiscalImpactFuture = r.FiscalImpactFuture,
                            FiscalImpactYr1 = r.FiscalImpactYr1,
                            FiscalImpactYr2 = r.FiscalImpactYr2,
                            FiscalNoteSubmitted = r.FiscalNoteSubmitted,
                            InformationToBeProvided = r.InformationToBeProvided,
                            Notes = r.Notes,
                            PolicyImpact = r.PolicyImpact,
                            RequiresTestimony = r.RequiresTestimony,
                            Timestamp = DateTime.Now
                            //SupercedesPreviousSnapshotID = ()
                        };
                    report.AlsrBillReviewSnapshots.Add(s);
                }

                db.BillsAlsrReports.Add(report);
                await db.SaveChangesAsync();

                var reportService = new BillsAlsrReportsService();
                var readReport = reportService.GetById(report.ID);
                readReport.Pdf = await CreateAlsrPdf(report.ID);
                readReport.Filename = string.Format("ALSR_{0}_{1}.pdf", readReport.Dept.Description, readReport.CreatedAt);
                reportService.Update(readReport);
                //await db.SaveChangesAsync();

                Success("ALSR Report created successfully");
                return RedirectToAction("ALSR");
            }
            Danger("Invalid model state");
            return View();
        }

        // GET: BillReports/DownloadAlsr/5
        public ActionResult DownloadAlsr(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var reportService = new BillsAlsrReportsService();
            var report = reportService.GetById(id.Value);
            return File(report.Pdf, "application/pdf", report.Filename);
        }

        private async Task<byte[]> CreateAlsrPdf(int id)
        {
            var reportModel = await db.BillsAlsrReports
                                .Where(r => r.ID == id)
                                .Include(r => r.AlsrBillReviewSnapshots.Select(s => s.Bill))
                                .Include(r => r.AlsrBillReviewSnapshots.Select(s => s.BillVersion))
                                .Include(r => r.AlsrBillReviewSnapshots.Select(s => s.CreatedByUser))
                                .Include(r => r.AlsrBillReviewSnapshots.Select(s => s.CreatedByUserInDept))
                                .Include(r => r.AlsrBillReviewSnapshots.Select(s => s.CreatedByUserInDiv))
                                .Include(r => r.AlsrBillReviewSnapshots.Select(s => s.Recommendation))
                                .FirstOrDefaultAsync();

            // initialize iText document
            using (MemoryStream ms = new MemoryStream())
            {
                string agencyTitle = reportModel.AlsrBillReviewSnapshots.FirstOrDefault().CreatedByUserInDept.Description;

                //initialize PDF Document
                var doc = new Document(PageSize.LETTER, 36, 36, 36, 36);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                 // add page # footer
                PageEventHelper pageEventHelper = new PageEventHelper();
                writer.PageEvent = pageEventHelper;

                doc.AddAuthor(agencyTitle);
                doc.AddCreationDate();
                doc.Open();

                // formatting variables
                float tableFullPageWidth = 540f;
                float tableCellBottomPadding = 6f;
                float paragraphSpacingBefore = 18f;

                // header (used on each page)
                Chunk alsrAgencyHeader = new Chunk(agencyTitle);
                Chunk alsrHeaderLine2 = new Chunk("\nAgency Legislative Status Report");
                Chunk alsrHeaderLine3 = new Chunk("\n78th Legislative Session (2015)");
                Paragraph header = new Paragraph();
                header.Alignment = Element.ALIGN_CENTER;
                header.Add(alsrAgencyHeader);
                header.Add(alsrHeaderLine2);
                header.Add(alsrHeaderLine3);

                // cover sheet
                Chapter coverSheet = new Chapter(header, 1);
                coverSheet.NumberDepth = 0;
                coverSheet.BookmarkTitle = "Cover Sheet";

                doc.Add(coverSheet);
                
                // individual BillReviews
                for (int i = 0; i < reportModel.AlsrBillReviewSnapshots.Count; i++)
                {
                    Chapter reviewPage = new Chapter(header, 1);
                    reviewPage.NumberDepth = 0;
                    reviewPage.BookmarkTitle = reportModel.AlsrBillReviewSnapshots[i].Bill.CompositeBillNumber;

                    // Agency contact section
                    PdfPTable contactTable = new PdfPTable(2);
                    contactTable.DefaultCell.Border = Rectangle.NO_BORDER;
                    contactTable.DefaultCell.PaddingBottom = tableCellBottomPadding;
                    contactTable.TotalWidth = tableFullPageWidth;
                    contactTable.LockedWidth = true;
                    contactTable.SpacingBefore = paragraphSpacingBefore;
                    contactTable.AddCell(string.Format("Agency: {0}", agencyTitle));
                    contactTable.AddCell(string.Format("Delivery Date: {0}", reportModel.GovOfficeDeliveryDate.ToShortDateString()));
                    PdfPCell contactNameCell = new PdfPCell(new Phrase(string.Format("Contact Name: Ann Wilkinson (Deputy Administrator)")));
                        contactNameCell.Colspan = 2;
                        contactNameCell.Border = Rectangle.NO_BORDER;
                        contactNameCell.PaddingBottom = tableCellBottomPadding;
                        contactTable.AddCell(contactNameCell);
                    contactTable.AddCell(string.Format("Phone: 775-684-0222"));
                    contactTable.AddCell(string.Format("Fax: 775-684-0260"));
                    reviewPage.Add(contactTable);

                    // Bill info section
                    PdfPTable billInfoTable = new PdfPTable(3);
                    billInfoTable.DefaultCell.Border = Rectangle.NO_BORDER;
                    billInfoTable.DefaultCell.PaddingBottom = tableCellBottomPadding;
                    billInfoTable.TotalWidth = tableFullPageWidth;
                    billInfoTable.LockedWidth = true;
                    billInfoTable.SpacingBefore = paragraphSpacingBefore;
                    float[] billInfoTableWidths = new float[] { 1, 1, 2 };
                    billInfoTable.SetWidths(billInfoTableWidths);
                    billInfoTable.AddCell(string.Format("Bill No: {0}", reportModel.AlsrBillReviewSnapshots[i].Bill.CompositeBillNumber));
                    billInfoTable.AddCell(string.Format("BDR No: {0}", reportModel.AlsrBillReviewSnapshots[i].Bill.CompositeNelisBdrNumber));
                    billInfoTable.AddCell(string.Format("Sponsor: {0}", reportModel.AlsrBillReviewSnapshots[i].BillVersion.Sponsor));
                    PdfPCell reprintNoCell = new PdfPCell(new Phrase(string.Format("Reprint No: {0}", reportModel.AlsrBillReviewSnapshots[i].BillVersion.VersionDescription)));
                        reprintNoCell.Colspan = 4;
                        reprintNoCell.Border = Rectangle.NO_BORDER;
                        reprintNoCell.PaddingBottom = tableCellBottomPadding;
                        billInfoTable.AddCell(reprintNoCell);
                    reviewPage.Add(billInfoTable);

                    // BillReview section
                    Paragraph billReviewHeaderParagraph = new Paragraph(string.Format("This Bill Review is being provided on behalf of Agency: {1}", agencyTitle, reportModel.AlsrBillReviewSnapshots[i].CreatedByUserInDiv.Description));
                    billReviewHeaderParagraph.SpacingBefore = paragraphSpacingBefore;
                    reviewPage.Add(billReviewHeaderParagraph);

                    PdfPTable billReviewTable = new PdfPTable(2);
                    billReviewTable.DefaultCell.Border = Rectangle.NO_BORDER;
                    billReviewTable.DefaultCell.PaddingBottom = tableCellBottomPadding;
                    billReviewTable.DefaultCell.PaddingRight = tableCellBottomPadding;
                    billReviewTable.TotalWidth = tableFullPageWidth;
                    billReviewTable.LockedWidth = true;
                    billReviewTable.SpacingBefore = paragraphSpacingBefore;
                    float[] billReviewTableWidths = new float[] { 1, 2 };
                    billReviewTable.SetWidths(billReviewTableWidths);
                    billReviewTable.AddCell("Position:");
                    billReviewTable.AddCell(reportModel.AlsrBillReviewSnapshots[i].Recommendation.Description);
                    billReviewTable.AddCell("Agency is tracking this Bill:");
                    billReviewTable.AddCell((reportModel.AlsrBillReviewSnapshots[i].ActivelyTracking ? "Yes" : "No"));
                    billReviewTable.AddCell("Information/testimony or data to be provided by agency:");
                    billReviewTable.AddCell((reportModel.AlsrBillReviewSnapshots[i].InformationToBeProvided ? "Yes" : "No"));
                    billReviewTable.AddCell("Policy impact on agency:");
                    string policyImpact = "";
                    switch (reportModel.AlsrBillReviewSnapshots[i].PolicyImpact)
                    {
                        case null:
                            policyImpact = "Unknown";
                            break;
                        case false:
                            policyImpact = "No";
                            break;
                        case true:
                            policyImpact = "Yes";
                            break;
                        default:
                            break;
                    }
                    billReviewTable.AddCell(policyImpact);
                    billReviewTable.AddCell("Fiscal impact on agency:");
                    billReviewTable.AddCell((reportModel.AlsrBillReviewSnapshots[i].FiscalNoteSubmitted ? "Yes" : "No"));
                    billReviewTable.AddCell("Estimated fiscal impact for biennium:");
                    billReviewTable.AddCell(string.Format("${0}", reportModel.AlsrBillReviewSnapshots[i].FiscalImpactBiennium));
                    reviewPage.Add(billReviewTable);

                    Paragraph billReviewCommentsHeaderParagraph = new Paragraph("Comments: Would passage of this bill constitute good public policy?\n");
                    billReviewCommentsHeaderParagraph.SpacingBefore = paragraphSpacingBefore;
                    reviewPage.Add(billReviewCommentsHeaderParagraph);

                    Paragraph billReviewCommentsParagraph = new Paragraph(reportModel.AlsrBillReviewSnapshots[i].Comments);
                    //billReviewCommentsParagraph.SpacingBefore = paragraphSpacingBefore;
                    reviewPage.Add(billReviewCommentsParagraph);

                    doc.Add(reviewPage);
                }


                doc.Close();
                writer.Close();
                return (ms.ToArray());
            }
        }

        public class PageEventHelper : PdfPageEventHelper
        {
            PdfContentByte cb;
            PdfTemplate template;
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);


            public override void OnOpenDocument(PdfWriter writer, Document document)
            {
                cb = writer.DirectContent;
                template = cb.CreateTemplate(50, 50);
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                base.OnEndPage(writer, document);

                int pageN = writer.PageNumber;
                String text = "Page " + pageN.ToString() + " of ";
                float len = bf.GetWidthPoint(text, 8f);

                iTextSharp.text.Rectangle pageSize = document.PageSize;

                cb.SetRGBColorFill(100, 100, 100);

                cb.BeginText();
                cb.SetFontAndSize(bf, 8f);
                cb.SetTextMatrix(document.LeftMargin, pageSize.GetBottom(document.BottomMargin));
                cb.ShowText(text);

                cb.EndText();

                cb.AddTemplate(template, document.LeftMargin + len, pageSize.GetBottom(document.BottomMargin));
            }

            public override void OnCloseDocument(PdfWriter writer, Document document)
            {
                base.OnCloseDocument(writer, document);

                template.BeginText();
                template.SetFontAndSize(bf, 8);
                template.SetTextMatrix(0, 0);
                template.ShowText("" + (writer.PageNumber - 1));
                template.EndText();
            }
        }

        //public void GenerateAlsrReport(AlsrReportModel model)
        //{
        //    // initialize iText document
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        var doc = new Document(PageSize.LETTER, 36, 36, 36, 36);
        //        PdfWriter writer = PdfWriter.GetInstance(doc, ms);

        //        doc.AddAuthor("Nevada Department of Administration");
        //        doc.AddCreationDate();
        //        doc.Open();

        //        foreach (BillReview r in model.BillReviews.ToList())
        //        {
        //            doc.Add(new Paragraph(r.Bill.CompositeBillNumber));
        //        }

        //        doc.Close();

        //        writer.Close();
        //        Response.ContentType = "pdf/application;";
        //        Response.AddHeader("content-disposition", "attachment;filename=Dept of Administration - ALSR - " + DateTime.Now + ".pdf");
        //        Response.OutputStream.Write(ms.GetBuffer(), 0, ms.GetBuffer().Length);
        //    }
        //}

        //private static byte[] ReadPdfData(string fileName)
        //{
        //    using (var source = System.IO.File.OpenRead(fileName))
        //    {
        //        var buffer = new byte[16 * 1024];
        //        using (var ms = new MemoryStream())
        //        {
        //            int bytesRead;
        //            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
        //            {
        //                ms.Write(buffer, 0, bytesRead);
        //            }
        //            return ms.ToArray();
        //        }
        //    }
        //}

        // GET: BillReports/AddFile
        
        //public ActionResult AddFile()
        //{
        //    var reportService = new BillsAlsrReportsService();
        //    var report = new BillsAlsrReport();
        //    report.Pdf = ReadPdfData(@"C:\Temp\test.pdf");
        //    report.CreatedAt = DateTime.Now;
        //    reportService.Insert(report);
        //    return RedirectToAction("DocumentsViewsList");
        //}

        // GET: BillReports/ALSR

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
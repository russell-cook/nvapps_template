using AdminApps.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace AdminApps.DAL.Services
{
    public class BillsAlsrReportsService : IRepository<BillsAlsrReport>
    {
        private const string RowDataStatement = @"SELECT Pdf.PathName() AS 'Path', GET_FILESTREAM_TRANSACTION_CONTEXT() AS 'Transaction' FROM {0} WHERE ID = @id";

        public  IList<BillsAlsrReport> GetAll()
        {
            using (var context = new ApplicationDbContext())
            {
                return context.BillsAlsrReports.ToList();
            }
        }

        public BillsAlsrReport GetById(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                var report = context.BillsAlsrReports.FirstOrDefault(r => r.ID == id);

                if (report == null)
                {
                    return null;
                }
                
                using (var tx = new TransactionScope())
                {

                    var selectStatement = String.Format(RowDataStatement, context.GetTableName<BillsAlsrReport>());

                    var rowData =
                        context.Database.SqlQuery<FileStreamRowData>(selectStatement, new SqlParameter("id", id))
                            .First();

                    using (var source = new SqlFileStream(rowData.Path, rowData.Transaction, FileAccess.Read))
                    {
                        var buffer = new byte[16 * 1024];
                        using (var ms = new MemoryStream())
                        {
                            int bytesRead;
                            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, bytesRead);
                            }
                            report.Pdf = ms.ToArray();
                        }
                    }

                    tx.Complete();
                }

                return report;
            }
        }

        public void Update(BillsAlsrReport entity)
        {
            using (var context = new ApplicationDbContext())
            {
                using (var tx = new TransactionScope())
                {
                    context.Entry(entity).State = EntityState.Modified;
                    context.SaveChanges();

                    SaveBillsAlsrReportData(context, entity);

                    tx.Complete();
                }
            }
        }

        public void Insert(BillsAlsrReport entity)
        {
            using (var context = new ApplicationDbContext())
            {
                using (var tx = new TransactionScope())
                {
                    context.BillsAlsrReports.Add(entity);
                    context.SaveChanges();

                    SaveBillsAlsrReportData(context, entity);

                    tx.Complete();
                }
            }
        }

        public void Delete(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                context.Entry(new BillsAlsrReport { ID = id }).State = EntityState.Deleted;
                context.SaveChanges();
            }
        }

        private static void SaveBillsAlsrReportData(ApplicationDbContext context, BillsAlsrReport entity)
        {
            var selectStatement = String.Format(RowDataStatement, context.GetTableName<BillsAlsrReport>());

            var rowData =
                context.Database.SqlQuery<FileStreamRowData>(selectStatement, new SqlParameter("id", entity.ID))
                    .First();

            using (var destination = new SqlFileStream(rowData.Path, rowData.Transaction, FileAccess.Write))
            {
                var buffer = new byte[16 * 1024];
                using (var ms = new MemoryStream(entity.Pdf))
                {
                    int bytesRead;
                    while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destination.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

    }
}

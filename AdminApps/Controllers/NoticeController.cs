using AdminApps.Models;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminApps.Controllers
{
    public class NoticeController : BaseController
    {
        // GET: Notice
        public ActionResult Index()
        {
            Notice viewModel = new Notice();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Notice model)
        {
            Notice viewModel = new Notice();
            if (ModelState.IsValid)
            {
                DateTime postingDate = DateTime.Now;
                if (meetingDateIsValid(postingDate, model.MeetingDate.Value))
                {
                    viewModel.InjectFrom(model);
                    viewModel.MeetingDateTime = viewModel.MeetingDate.Value + viewModel.MeetingTime.Value.TimeOfDay;
                    Success("Meeting date/time is valid.");
                }
                else
                {
                    Danger("The meeting date you have provided is not valid. NRS 241.020 requires that notices be posted not later than 9 a.m. of the third working day before the meeting is to be held, unless the public body is unable to do so because of technical problems relating to the operation or maintenance of this website.");
                }
            }
            return View(viewModel);
        }

        public bool meetingDateIsValid (DateTime postingDate, DateTime meetingDate)
        {
            DateTime postingDate9AM = new DateTime(postingDate.Year, postingDate.Month, postingDate.Day, 9, 0, 0);
            DateTime minimumRequiredDate = new DateTime();
            switch (postingDate.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    minimumRequiredDate = postingDate.AddDays(4);
                    break;
                case DayOfWeek.Monday:
                    minimumRequiredDate = (postingDate < postingDate9AM) ? postingDate.AddDays(3) : postingDate.AddDays(4);
                    break;
                case DayOfWeek.Tuesday:
                    minimumRequiredDate = (postingDate < postingDate9AM) ? postingDate.AddDays(3) : postingDate.AddDays(6);
                    break;
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                    minimumRequiredDate = (postingDate < postingDate9AM) ? postingDate.AddDays(5) : postingDate.AddDays(6);
                    break;
                case DayOfWeek.Saturday:
                    minimumRequiredDate = postingDate.AddDays(5);
                    break;
            }
            return ((meetingDate.Date >= minimumRequiredDate.Date) ? true : false);
        }
    }
}
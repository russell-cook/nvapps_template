using NVApps.DAL;
using NVApps.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NVApps.Controllers
{
    [Authorize(Roles = "GlobalAdmin, RolesAdmin, UsersAdmin")]
    public class UsersAdminController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Users/
        public async Task<ActionResult> Index()
        {
            return View(await UserManager.Users.OrderBy(u => u.LastName).ToListAsync());
        }

        //
        // GET: /Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);

            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

            return View(user);
        }

        //
        // GET: /Users/Create
        public async Task<ActionResult> Create()
        {
            var globalBudgetPeriod = (await db.AppGlobalSettings.FirstOrDefaultAsync()).BudgetPeriodID;
            RegisterViewModel viewModel = new RegisterViewModel();

            //Get the list of Roles
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");

            // populate Dept/Div dropdowns
            viewModel.DeptsList = new SelectList(await db.Depts.Where(b => b.BudgetPeriodID == globalBudgetPeriod).OrderBy(c => c.Code).ToListAsync(), "ID", "CompositeDeptName");
            viewModel.DivsList = new SelectList(await db.Divs.Where(b => b.BudgetPeriodID == globalBudgetPeriod).OrderBy(c => c.Code).ToListAsync(), "ID", "CompositeDivName");
            return View(viewModel);
        }

        //
        // POST: /Users/Create
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, params string[] selectedRoles)
        {
            var globalBudgetPeriod = (await db.AppGlobalSettings.FirstOrDefaultAsync()).BudgetPeriodID;
            RegisterViewModel viewModel = new RegisterViewModel();

            if (ModelState.IsValid)
            {
                var newUser = new ApplicationUser
                {
                    FirstName = userViewModel.FirstName,
                    LastName = userViewModel.LastName,
                    Title = userViewModel.Title,
                    UserName = userViewModel.Email,
                    Email = userViewModel.Email,
                    AppModuleID = 1,
                    DeptID = userViewModel.DeptID,
                    DivID = userViewModel.DivID,
                    IsActive = true
                };
                // if password was not manually entered, generate random password and initialize account verification process
                string password;
                if (userViewModel.Password == null)
                {
                    password = await UserManager.GenerateRandomPasswordAsync();
                    newUser.AutoPwdReplaced = false;
                }
                else
                {
                    password = userViewModel.Password;
                    newUser.AutoPwdReplaced = true;
                }

                var adminresult = await UserManager.CreateAsync(newUser, password);
                string requestMessage = "";

                if (adminresult.Succeeded)
                {
                    // if password was not manually entered, send confirmation email with randomly generated password
                    if (userViewModel.Password == null)
                    {
                        string body;
                        using (var sr = new StreamReader(Server.MapPath("\\Templates\\") + "RegisterAccountEmailTemplate.html"))
                        {
                            body = await sr.ReadToEndAsync();
                        }
                        var code = await UserManager.GenerateEmailConfirmationTokenAsync(newUser.Id);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = newUser.Id, code = code }, protocol: Request.Url.Scheme);
                        string messageBody = string.Format(body, callbackUrl, newUser.UserName, password);
                        await UserManager.SendEmailAsync(newUser.Id, "Your AdminApps user account has been created. Please confirm your email address.", messageBody);
                    }

                    //Add User to the selected Roles 
                    if (selectedRoles != null)
                    {
                        var result = await UserManager.AddToRolesAsync(newUser.Id, selectedRoles);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", result.Errors.First());
                            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                            // populate Dept/Div dropdowns
                            viewModel.DeptsList = new SelectList(await db.Depts.Where(b => b.BudgetPeriodID == globalBudgetPeriod).OrderBy(c => c.Code).ToListAsync(), "ID", "CompositeDeptName");
                            viewModel.DivsList = new SelectList(await db.Divs.Where(b => b.BudgetPeriodID == globalBudgetPeriod).OrderBy(c => c.Code).ToListAsync(), "ID", "CompositeDivName");

                            return View(viewModel);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", adminresult.Errors.First());
                    ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                    // populate Dept/Div dropdowns
                    viewModel.DeptsList = new SelectList(await db.Depts.Where(b => b.BudgetPeriodID == globalBudgetPeriod).OrderBy(c => c.Code).ToListAsync(), "ID", "CompositeDeptName");
                    viewModel.DivsList = new SelectList(await db.Divs.Where(b => b.BudgetPeriodID == globalBudgetPeriod).OrderBy(c => c.Code).ToListAsync(), "ID", "CompositeDivName");
                    return View(viewModel);

                }
                Success("User was created successfully. " + requestMessage, true);
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            // populate Dept/Div dropdowns
            viewModel.DeptsList = new SelectList(await db.Depts.Where(b => b.BudgetPeriodID == globalBudgetPeriod).OrderBy(c => c.Code).ToListAsync(), "ID", "CompositeDeptName");
            viewModel.DivsList = new SelectList(await db.Divs.Where(b => b.BudgetPeriodID == globalBudgetPeriod).OrderBy(c => c.Code).ToListAsync(), "ID", "CompositeDivName");
            return View(viewModel);
        }

        //
        // GET: /Users/Edit/1
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);
            var globalBudgetPeriod = (await db.AppGlobalSettings.FirstOrDefaultAsync()).BudgetPeriodID;
            EditUserViewModel viewModel = new EditUserViewModel();

            viewModel.InjectFrom(user);
            viewModel.DeptsList = new SelectList(await db.Depts.Where(b => b.BudgetPeriodID == globalBudgetPeriod).OrderBy(c => c.Code).ToListAsync(), "ID", "CompositeDeptName");
            viewModel.DivsList = new SelectList(await db.Divs.Where(b => b.BudgetPeriodID == globalBudgetPeriod).OrderBy(c => c.Code).ToListAsync(), "ID", "CompositeDivName");
            viewModel.RolesList = RoleManager.Roles.OrderBy(i => i.Id).ToList().Select(x => new SelectListItem()
            {
                Selected = userRoles.Contains(x.Name),
                Text = x.Name,
                Value = x.Name
            });

            return View(viewModel);
        }

        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditUserViewModel editUser, params string[] selectedRole)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.InjectFrom(editUser);

                var userRoles = await UserManager.GetRolesAsync(user.Id);

                selectedRole = selectedRole ?? new string[] { };

                var result = await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Something failed.");
            return View();
        }

        //
        // GET: /Users/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}

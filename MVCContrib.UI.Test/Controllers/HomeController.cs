using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcContrib.EmailSender;
using MVCContrib.UI.ViewModels;
using System.Net.Mail;

namespace MVCContrib.EmailSenderTest.Controllers
{
	[HandleError]
	public class HomeController : Controller
	{
		private readonly EmailTemplateService _emailService = new EmailTemplateService();
		private static readonly string _viewPath = "~/Views/Mail/{0}.aspx";

		public ActionResult Index()
		{
			ViewData["Message"] = "Welcome to ASP.NET MVC!";

			return View();
		}

		public ActionResult SendMail()
		{
			var email = _emailService.RenderMessage(string.Format(_viewPath, "Welcome"), new EmailMetadata("sender@email.com", "foo@email.com") , ControllerContext);
			new SmtpClient().Send(email);
			return View();
		}

		public ActionResult SendEmbedMail()
		{
			var email = _emailService.RenderMessage(string.Format(_viewPath, "EmbedHeader"), ControllerContext, new EmailModel { From ="Sender @ Email dot com", To = "foo @ email dot com" });
			new SmtpClient().Send(email);

			return RedirectToAction("Index");
		}

	}

}

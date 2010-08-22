using System;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Web;

namespace MvcContrib.EmailSender
{
	/// <remarks>
	/// Inspired by Castle's EmailTemplateService.
	/// </remarks>
	public class EmailTemplateService : IEmailTemplateService
	{

		#region From MVCContrib.Services
		
		private readonly IViewStreamReader _viewReader;

		public EmailTemplateService()
		{
			_viewReader = new ViewStreamReader();
		}

		public EmailTemplateService(IViewStreamReader viewReader)
		{
			_viewReader = viewReader;
		}

		public MailMessage RenderMessage(string viewName, EmailMetadata metadata, ControllerContext context)
		{
			var details = GetEmailDetails(viewName, metadata, context);

			var result = new MailMessage { From = metadata.From, Subject = details.Subject, Body = details.Body, IsBodyHtml = metadata.IsHtmlEmail };
			metadata.To.ForEach(x => result.To.Add(x));
			metadata.Cc.ForEach(x => result.CC.Add(x));
			metadata.Bcc.ForEach(x => result.Bcc.Add(x));

			return result;
		}

		private EmailDetails GetEmailDetails(string viewName, EmailMetadata metadata, ControllerContext context)
		{
			using (var stream = _viewReader.GetViewStream(viewName, metadata, context))
			{
				string subject = "";
				string body = "";

				using (var reader = new StreamReader(stream))
				{
					bool subjectProcessed = false;
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						if (!subjectProcessed)
						{
							if (string.IsNullOrEmpty(line))
							{
								continue;
							}

							subject = line;
							subjectProcessed = true;
							continue;
						}
						body += line;
					}
				}

				return new EmailDetails { Body = body, Subject = subject };
			}
		}

		/// <summary>
		/// The only information that comes from the email template is subject and body. 
		/// 
		/// Everything else ("to", "from", etc) is known when we call the service. 
		/// But subject/body are localizable and can contain placeholders - so they need 
		/// to get fetched from the email template view. 
		/// </summary>
		private class EmailDetails
		{
			public string Subject { get; set; }
			public string Body { get; set; }
		}

		private class ViewStreamReader : IViewStreamReader
		{
			public Stream GetViewStream(string viewName, object model, ControllerContext controllerContext)
			{
				var view = ViewEngines.Engines.FindPartialView(controllerContext, viewName).View;
				if (view == null)
				{
					throw new InvalidOperationException(string.Format("Could not find a view named '{0}'", viewName));
				}

				var sb = new StringBuilder();
				using (var writer = new StringWriter(sb))
				{
					var viewContext = new ViewContext(controllerContext, view, new ViewDataDictionary(model), new TempDataDictionary(), writer);
					view.Render(viewContext, writer);

					writer.Flush();
				}
				return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
			}
		}
		
		#endregion

		#region Taking from MonoRail

		private static readonly String HeaderPattern = @"[ \t]*(?<header>(to|from|cc|bcc|subject|reply-to|X-\w+)):[ \t]*(?<value>(.)+)(\r*\n*)?";
		private static readonly Regex HeaderRegEx = new Regex(HeaderPattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public MailMessage RenderMessage(string viewName, ControllerContext controllerContext, object model)
		{
			MailMessage message ;
			using (var stream = _viewReader.GetViewStream(viewName, model, controllerContext))
			{
				message = CreateMessage(new StreamReader(stream));
			}
			
			if (message.To.Count == 0) 
				throw new InvalidOperationException("There is no receipient in embeded in the view (eg: to: <someone@somewhere.com>)");

			if(message.From == null)
				throw new InvalidOperationException("A from address must be specified. (eg: from: <someone@somewhere.com>)");

			return message;
		}

		private static bool IsLineAHeader(string line, out string header, out string value)
		{
			Match match = HeaderRegEx.Match(line);

			if (match.Success)
			{
				header = match.Groups["header"].ToString().ToLower();
				value = match.Groups["value"].ToString();
				return true;
			}

			header = value = null;
			return false;
		}

		private MailMessage CreateMessage(StreamReader reader)
		{
			// create a message object
			MailMessage message = new MailMessage();

			bool isInBody = false;
			StringBuilder body = new StringBuilder();
			string line;

			while ((line = reader.ReadLine()) != null)
			{
				string header, value;
				
				if (!isInBody & line.Trim().Length == 0) continue;

				if (!isInBody && IsLineAHeader(line, out header, out value))
				{
					switch (header.ToLowerInvariant())
					{
						case "to":
							message.To.Add(value);
							break;
						case "cc":
							message.CC.Add(value);
							break;
						case "bcc":
							message.Bcc.Add(value);
							break;
						case "subject":
							message.Subject = value;
							break;
						case "from":
							message.From = new MailAddress(value);
							break;
						case "reply-to":
							message.ReplyTo = new MailAddress(value);
							break;
						default:
							message.Headers[header] = value;
							break;
					}
				}
				else
				{
					isInBody = true;
					body.AppendLine(line);
				}
			}

			message.Body = body.ToString();

			if (message.Body.IndexOf("<html", StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				message.IsBodyHtml = true;
			}

			return message;
		}
		
		#endregion

	}
}
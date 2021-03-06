using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using MvcViewEngines = System.Web.Mvc.ViewEngines;
using System.Net.Mail;

namespace MvcContrib.EmailSender.Test
{
	[TestFixture]
	public class EmailTemplateServiceTest
	{
		private ControllerContext _controllerContext;
		private EmailTemplateService _service = new EmailTemplateService();

		[SetUp]
		public void Setup()
		{
			_controllerContext = new ControllerContext { HttpContext = MvcMockHelpers.DynamicHttpContextBase() };
			_service = new EmailTemplateService();
		}

		[Test]
		public void CanRenderMessage()
		{
			string subject = "this line is subject";
			string body = "Here is message body...";
			var viewStream = new MemoryStream(Encoding.UTF8.GetBytes(subject + Environment.NewLine + body));
			var viewReader = MockRepository.GenerateMock<IViewStreamReader>();
			viewReader.Expect(x => x.GetViewStream(null, null, null)).IgnoreArguments().Return(viewStream);

			_service = new EmailTemplateService(viewReader);
			var message = _service.RenderMessage("foo", new EmailMetadata("from@amail.com", "to@email.com"), _controllerContext);

			Assert.That(message, Is.Not.Null);
			Assert.That(message.Subject, Is.EqualTo(subject));
			Assert.That(message.Body, Is.EqualTo(body));
			Assert.That(message.IsBodyHtml, Is.True, "Emails are HTML by default - plain text should be explicitly set in metadata.");
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ShouldThrowIfViewNotFound()
		{
			var engine = MockRepository.GenerateMock<IViewEngine>();
			engine.Stub(x => x.FindPartialView(null, null, true)).IgnoreArguments().Return(new ViewEngineResult(new[] { "foo", "bar" }));
			MvcViewEngines.Engines.Clear();
			MvcViewEngines.Engines.Add(engine);

			_service.RenderMessage("foo", new EmailMetadata("from@amail.com", "to@email.com"), _controllerContext);
		}

		[Test]
		public void CanRenderMessageWithEmbededHeader()
		{
			string to = "sokun@ncdd.gov.kh";
			string body = "<body>whatever ...</body>";
			var viewStream = new MemoryStream(Encoding.UTF8.GetBytes("to:" + to + Environment.NewLine + body));
			var viewReader = MockRepository.GenerateMock<IViewStreamReader>();
			viewReader.Expect(x => x.GetViewStream(null, null, null)).IgnoreArguments().Return(viewStream);

			_service = new EmailTemplateService(viewReader);
			var message = _service.RenderMessage("foo", _controllerContext, null);

			Assert.IsNotNull(message.To);
			Assert.IsNotNull(message.Body);

			Assert.IsTrue(message.To.ToString().Contains(to));
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ShouldThrowIfHeaderNotDetected()
		{
			var viewStream = new MemoryStream(Encoding.UTF8.GetBytes("nothing"));
			var viewReader = MockRepository.GenerateMock<IViewStreamReader>();
			viewReader.Expect(x => x.GetViewStream(null, null, null)).IgnoreArguments().Return(viewStream);

			_service = new EmailTemplateService(viewReader);
			var message = _service.RenderMessage("foo", _controllerContext, null);
		}

		[Test]
		public void CanRenderMessageFirstLineEmpty()
		{
			var viewStream = new MemoryStream(Encoding.UTF8.GetBytes(@"
to: sokun@ncdd.gov.kh
subject: Hello !
<html>
<head>
		<title>EmbedHeader</title>
</head>
<body>
what are you doing?
</body>
</html>
"));
			var viewReader = MockRepository.GenerateMock<IViewStreamReader>();
			viewReader.Expect(x => x.GetViewStream(null, null, null)).IgnoreArguments().Return(viewStream);

			_service = new EmailTemplateService(viewReader);
			var message = _service.RenderMessage("foo", _controllerContext, null);

			Assert.IsNotNull(message.To);
		}
	}
}
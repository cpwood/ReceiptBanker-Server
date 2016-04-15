using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web.Http;

namespace Controllers
{
    public class DataRequest
    {
        public string Email { get; set; }
        public string Html { get; set; }
        public string Attachment { get; set; }
        // Not actually used.
        public string Filename { get; set; }
    }

    public class DataController : ApiController
    {
        public IHttpActionResult Post(DataRequest request)
        {
            var msg = new MailMessage();
            msg.From = new MailAddress(ConfigurationManager.AppSettings["sender"]);
            msg.To.Add(new MailAddress(request.Email));
            msg.Subject = "Invoice from Receipt Banker Chrome Extension";

            msg.SubjectEncoding = Encoding.UTF8;
            msg.BodyEncoding = Encoding.UTF8;
            msg.IsBodyHtml = false;
            // ReSharper disable once AssignNullToNotNullAttribute
            msg.Body = null;

            var htmlView = AlternateView.CreateAlternateViewFromString(request.Html);
            htmlView.ContentType = new ContentType("text/html");
            htmlView.ContentType.CharSet = Encoding.UTF8.WebName;
            htmlView.TransferEncoding = TransferEncoding.Base64;
            msg.AlternateViews.Add(htmlView);

            MemoryStream ms = null;

            try
            {
                if (request.Attachment != null)
                {
                    ms = new MemoryStream(Convert.FromBase64String(request.Attachment));
                    var attachment = new Attachment(ms, Guid.NewGuid() + ".pdf", "application/pdf");
                    msg.Attachments.Add(attachment);  
                }

                using (var client = new SmtpClient())
                {
                    client.Send(msg);
                }

                return Ok("Received");
            }
            finally
            {
                if (ms != null)
                {
                    ms.Close();
                    ms.Dispose();
                }
            }

        
        }
    }
}
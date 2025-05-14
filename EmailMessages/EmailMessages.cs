using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using System.Text;
using Domain;

namespace Messages
{
    public class EmailMessages
    {

        public IConfiguration Configuration { get; }
        public object Assert { get; private set; }
        string SenderEmail;
        string SenderPassword;
        string BccEmail;
        string BusinessNumber;
        string BusinessEmail;
        List<string> BccEmailList;

        public EmailMessages(IConfiguration configuration)
        {

            this.Configuration = configuration;
            this.SenderEmail = Configuration.GetSection("EmailVar")["senderEmail"];
            this.SenderPassword = Configuration.GetSection("EmailVar")["senderPassword"];
            this.BusinessNumber = Configuration.GetSection("BusinessInfo")["number"];
            this.BusinessEmail = Configuration.GetSection("BusinessInfo")["email"];
            this.BccEmail = Configuration.GetSection("EmailVar")["bccEmail"];
            this.BccEmailList = (BccEmail?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0]).ToList();

        }

        /// <summary>
        /// Sends an email with a newly generated password to the user.
        /// </summary>
        /// <param name="user">The user object containing email and password details.</param>
        public async Task NewGeneratedPassword(User user)
        {
            // Ensure sender email credentials are securely stored
            string senderEmail = SenderEmail;
            string senderPassword = SenderPassword;
            string bccEmail = BccEmail;
            const string subject = "Ultimate Hoopers - New Password Generated";

            // Receiver's email address
            string receiverEmail = user.Email;

            // Current date
            string ticketDate = DateTime.Now.ToString("MMMM dd, yyyy");

            // Construct the email's HTML body
            string htmlBody = $@"
        <html>
        <head>
            <style>
                .button {{
                    background-color: #4CAF50;
                    border: none;
                    color: white;
                    padding: 15px 32px;
                    text-align: center;
                    text-decoration: none;
                    display: inline-block;
                    font-size: 16px;
                    margin: 4px 2px;
                    cursor: pointer;
                }}
            </style>
        </head>
        <body>
            <h1>New Password Generated</h1>
            <p>Password: <strong>{System.Web.HttpUtility.HtmlEncode(user.Password)}</strong></p>
            <p>Created Date: {System.Web.HttpUtility.HtmlEncode(ticketDate)}</p>
        </body>
        </html>";

            try
            {
                using (var smtpClient = new SmtpClient("smtp.office365.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

                    using (var message = new MailMessage(senderEmail, receiverEmail))
                    {
                        message.Subject = subject;
                        message.Body = htmlBody;
                        message.IsBodyHtml = true;

                        // Add BCC recipients
                        if (BccEmailList != null)
                        {
                            foreach (var item in BccEmailList)
                            {
                                message.Bcc.Add(new MailAddress(item));
                            }
                        }

                        // Send the email asynchronously
                        await smtpClient.SendMailAsync(message);
                        Console.WriteLine("Email sent successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error with stack trace for debugging
                Console.WriteLine($"Failed to send email. Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }



        /// <summary>
        /// Sends a sign-up confirmation email to the user.
        /// </summary>
        /// <param name="user">The user object containing email, username, and other details.</param>
        public async Task SignUpEmail(User user)
        {
            // Ensure sensitive data like sender credentials is securely stored
            string senderEmail = SenderEmail;
            string senderPassword = SenderPassword;

            // Email subject and receiver's address
            const string subject = "Ultimate Hoopers - Thanks for Signing Up";
            string receiverEmail = user.Email;

            // Current date for the email
            string ticketDate = DateTime.Now.ToString("MMMM dd, yyyy");

            // Construct the email's HTML body
            string htmlBody = $@"
        <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    line-height: 1.6;
                    color: #333;
                }}
                h1 {{
                    color: #4CAF50;
                }}
                .button {{
                    background-color: #4CAF50;
                    border: none;
                    color: white;
                    padding: 15px 32px;
                    text-align: center;
                    text-decoration: none;
                    display: inline-block;
                    font-size: 16px;
                    margin: 4px 2px;
                    cursor: pointer;
                }}
            </style>
        </head>
        <body>
            <h1>Player Number: #{System.Web.HttpUtility.HtmlEncode(user.Profile.PlayerNumber)}</h1>
            <h1>{System.Web.HttpUtility.HtmlEncode(user.Email)}, Welcome!</h1>
            <p>Welcome to Ultimate Hooper – the home for passionate hoopers like you! We're excited to have you join our community of basketball enthusiasts who love the thrill of pickup games and connecting with other players.</p>
            <p>If you have any questions or need assistance, feel free to reach out to our team at <a href='mailto:support@ultimatehooper.com'>support@ultimatehooper.com</a>. We’re here to help you ball out. See you on the court!</p>
            <p>Stay legendary,<br/>The Ultimate Hooper Team</p>
            <p>Created Date: {System.Web.HttpUtility.HtmlEncode(ticketDate)}</p>
        </body>
        </html>";

            try
            {
                using (var smtpClient = new SmtpClient("smtp.office365.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

                    using (var message = new MailMessage(senderEmail, receiverEmail))
                    {
                        message.Subject = subject;
                        message.Body = htmlBody;
                        message.IsBodyHtml = true;

                        // Add BCC recipients if available
                        if (BccEmailList != null)
                        {
                            foreach (var item in BccEmailList)
                            {
                                message.Bcc.Add(new MailAddress(item));
                            }
                        }

                        // Send the email asynchronously
                        await smtpClient.SendMailAsync(message);
                        Console.WriteLine("Sign-up confirmation email sent successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error with detailed information for debugging
                Console.WriteLine($"Failed to send email. Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }


        /// <summary>
        /// Sends a sign-up confirmation email to the user.
        /// </summary>
        /// <param name="user">The user object containing email, username, and other details.</param>
        //public async Task PostCommentNotification(Post post)
        //{

        //    foreach (var item in post.ProfileMentions)
        //    {
        //        if (item.Setting.AllowEmailNotification)
        //        {
        //            // Ensure sensitive data like sender credentials is securely stored
        //            string senderEmail = SenderEmail;
        //            string senderPassword = SenderPassword;

        //            // Email subject and receiver's address
        //            const string subject = "A post that you was mentioned in was commented";
        //            string receiverEmail = item.Email;
        //            string postTitle = post.Title;

        //            // Current date for the email
        //            string ticketDate = DateTime.Now.ToString("MMMM dd, yyyy");

        //            // Construct the email's HTML body
        //            string htmlBody = $@"
        //<html>
        //<head>
        //    <style>
        //        body {{
        //            font-family: Arial, sans-serif;
        //            line-height: 1.6;
        //            color: #333;
        //        }}
        //        h1 {{
        //            color: #4CAF50;
        //        }}
        //        .button {{
        //            background-color: #4CAF50;
        //            border: none;
        //            color: white;
        //            padding: 15px 32px;
        //            text-align: center;
        //            text-decoration: none;
        //            display: inline-block;
        //            font-size: 16px;
        //            margin: 4px 2px;
        //            cursor: pointer;
        //        }}
        //    </style>
        //</head>
        //<body>
        //    <h1>{System.Web.HttpUtility.HtmlEncode(subject)}</h1>
        //   <img src='https://uhblobstorageaccount.blob.core.windows.net/defaultimage/notificationlogo.png' alt='Notification Image' style='max-width: 100%; height: auto;' />
        //    <p>Login and view <a href='https://ultimatehooperapp.azurewebsites.net/'>ultimatehooper.com</a>. We’re here to help you ball out. See you on the court!</p>
        //    <p>Stay legendary,<br/>The Ultimate Hooper Team</p>
        //    <p>Created Date: {System.Web.HttpUtility.HtmlEncode(ticketDate)}</p>
        //</body>
        //</html>";

        //            try
        //            {
        //                using (var smtpClient = new SmtpClient("smtp.office365.com"))
        //                {
        //                    smtpClient.Port = 587;
        //                    smtpClient.EnableSsl = true;
        //                    smtpClient.UseDefaultCredentials = false;
        //                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

        //                    using (var message = new MailMessage(senderEmail, receiverEmail))
        //                    {
        //                        message.Subject = subject;
        //                        message.Body = htmlBody;
        //                        message.IsBodyHtml = true;

        //                        //// Add BCC recipients if available
        //                        //if (BccEmailList != null)
        //                        //{
        //                        //    foreach (var item in BccEmailList)
        //                        //    {
        //                        //        message.Bcc.Add(new MailAddress(item));
        //                        //    }
        //                        //}

        //                        // Send the email asynchronously
        //                        await smtpClient.SendMailAsync(message);
        //                        Console.WriteLine("Sign-up confirmation email sent successfully.");
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                // Log the error with detailed information for debugging
        //                Console.WriteLine($"Failed to send email. Error: {ex.Message}");
        //                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        //            }
        //        }
        //        ;
        //    }



        //}


        /// <summary>
        /// Send Temp Forgotten Password
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task SendTempForgottenPassword(User user)
        {
            // Sender's email address and password

            string senderEmail = SenderEmail;
            string senderPassword = SenderPassword;


            // Receiver's email address
            string receiverEmail = user.Email;

            // BCC recipient's email address
            //string bccEmail = "esoundsonline@gmail.com";

            // Create a new MailMessage object
            MailMessage mail = new MailMessage(senderEmail, receiverEmail);

            // Add BCC recipient
            // mail.Bcc.Add(bccEmail);

            // Set the subject and body of the email
            mail.Subject = "Your New Temp Password";


            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Temp Password");
            stringBuilder.Append("\n");
            stringBuilder.Append(user.Password);





            mail.Body = stringBuilder.ToString();

            //mail.Body = "This is a test email from Outlook using C#\n"
            //+ "This email has multiple lines in the body.\n"
            //+ "You can add as many lines as you want.";

            // Create an SMTP client
            SmtpClient smtpClient = new SmtpClient("smtp.office365.com");

            // Set the port and enable SSL
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;

            // Set the credentials (sender's email address and password)
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

            try
            {
                // Send the email
                smtpClient.Send(mail);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send email. Error: " + ex.Message);
            }
        }




    }

}

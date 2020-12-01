using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Saber.Vendor;

namespace Saber
{
    public class TestEmailClient : IVendorEmailClient
    {
        public string Id { get; set; } = "test-client";
        public string Name { get; set; } = "Test Client";
        public Dictionary<string, EmailClientParameter> Parameters { get; set; } = new Dictionary<string, EmailClientParameter>()
        {
            {"service",
                new EmailClientParameter()
                {
                    DataType = EmailClientDataType.List,
                    Name = "Service",
                    Description = "",
                    ListOptions = new string[]
                    {
                        "Gmail", "Hotmail", "Yahoo", "ProtonMail"
                    }
                }
            },
            {"host",
                new EmailClientParameter()
                {
                    DataType = EmailClientDataType.Text,
                    Name = "Host",
                    Description = ""
                }
            },
            {"port",
                new EmailClientParameter()
                {
                    DataType = EmailClientDataType.Number,
                    Name = "Port",
                    Description = ""
                }
            },
            {"use-ssl",
                new EmailClientParameter()
                {
                    DataType = EmailClientDataType.Boolean,
                    Name = "Use SSL",
                    Description = ""
                }
            },
            {"username",
                new EmailClientParameter()
                {
                    DataType = EmailClientDataType.UserOrEmail,
                    Name = "Username",
                    Description = ""
                }
            },
            {"password",
                new EmailClientParameter()
                {
                    DataType = EmailClientDataType.Password,
                    Name = "Password",
                    Description = ""
                }
            },
        };

        public Dictionary<string, string> GetConfig()
        {
            return new Dictionary<string, string>();
        }

        public void Init() { }

        public void SaveConfig(Dictionary<string, string> parameters)
        {
            
        }

        public void Send(MailMessage message, string RFC2822_formatted)
        {
            throw new NotImplementedException();
        }
    }
}

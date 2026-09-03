using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace ApiTransporteLweb.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            var host = _configuration["Smtp:Host"];
            var puerto = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var usuario = _configuration["Smtp:User"];
            var password = _configuration["Smtp:Password"];
            var remitente = _configuration["Smtp:From"] ?? usuario;

            using var mensaje = new MailMessage
            {
                From = new MailAddress(remitente!, "Transportes Luchito"),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };
            mensaje.To.Add(destinatario);

            using var cliente = new SmtpClient(host, puerto)
            {
                Credentials = new NetworkCredential(usuario, password),
                EnableSsl = true
            };

            await cliente.SendMailAsync(mensaje);
        }
    }
}
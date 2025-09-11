using System.Text;

namespace MedicareApi.Services
{
    public interface IEmailTemplateService
    {
        Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> variables);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<EmailTemplateService> _logger;
        private readonly string _templatePath;

        public EmailTemplateService(IWebHostEnvironment environment, ILogger<EmailTemplateService> logger)
        {
            _environment = environment;
            _logger = logger;
            _templatePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates");
        }

        public async Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> variables)
        {
            try
            {
                // Load base template
                var baseTemplatePath = Path.Combine(_templatePath, "BaseTemplate.html");
                var baseTemplate = await File.ReadAllTextAsync(baseTemplatePath, Encoding.UTF8);

                // Load content template
                var contentTemplatePath = Path.Combine(_templatePath, $"{templateName}.html");
                var contentTemplate = await File.ReadAllTextAsync(contentTemplatePath, Encoding.UTF8);

                // Add current year to variables if not present
                if (!variables.ContainsKey("year"))
                {
                    variables["year"] = DateTime.Now.Year.ToString();
                }

                // Add title if not present
                if (!variables.ContainsKey("title"))
                {
                    variables["title"] = GetDefaultTitle(templateName);
                }

                // Replace variables in content template
                var processedContent = ReplaceVariables(contentTemplate, variables);

                // Insert content into base template
                variables["content"] = processedContent;
                var finalHtml = ReplaceVariables(baseTemplate, variables);

                return finalHtml;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render email template: {TemplateName}", templateName);
                throw new InvalidOperationException($"Failed to render email template: {templateName}", ex);
            }
        }

        private string ReplaceVariables(string template, Dictionary<string, string> variables)
        {
            var result = template;

            foreach (var variable in variables)
            {
                var placeholder = $"{{{{{variable.Key}}}}}";
                result = result.Replace(placeholder, variable.Value ?? string.Empty);
            }

            // Handle conditional blocks (simple Mustache-like syntax)
            result = ProcessConditionalBlocks(result, variables);

            return result;
        }

        private string ProcessConditionalBlocks(string template, Dictionary<string, string> variables)
        {
            // Simple implementation for conditional blocks like {{#variable}}content{{/variable}}
            var result = template;

            foreach (var variable in variables)
            {
                var startTag = $"{{{{#{variable.Key}}}}}";
                var endTag = $"{{{{/{variable.Key}}}}}";

                var startIndex = result.IndexOf(startTag);
                while (startIndex != -1)
                {
                    var endIndex = result.IndexOf(endTag, startIndex);
                    if (endIndex == -1) break;

                    var blockContent = result.Substring(startIndex + startTag.Length, 
                                                       endIndex - startIndex - startTag.Length);

                    // If variable has value, keep the content; otherwise remove it
                    var replacement = !string.IsNullOrWhiteSpace(variable.Value) ? blockContent : string.Empty;
                    
                    result = result.Substring(0, startIndex) + replacement + 
                            result.Substring(endIndex + endTag.Length);

                    startIndex = result.IndexOf(startTag);
                }
            }

            return result;
        }

        private string GetDefaultTitle(string templateName)
        {
            return templateName switch
            {
                "EmailConfirmation" => "تأكيد البريد الإلكتروني - صحتك",
                "PasswordReset" => "إعادة تعيين كلمة المرور - صحتك",
                "Welcome" => "مرحباً بك في صحتك",
                "AppointmentReminder" => "تذكير بالموعد الطبي - صحتك",
                _ => "صحتك - منصة الرعاية الصحية"
            };
        }
    }
}
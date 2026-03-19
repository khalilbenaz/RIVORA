namespace RVR.Framework.Email.Templates;

public interface IEmailTemplateRenderer
{
    string Render(string templateName, Dictionary<string, string> variables);
}

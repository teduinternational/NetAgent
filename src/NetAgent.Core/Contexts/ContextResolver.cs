namespace NetAgent.Core.Contexts
{
    public class ContextResolver
    {
        public string Resolve(ContextTemplate template, Dictionary<string, object> data)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Ensure all required keys are present in the data dictionary
            foreach (var key in template.RequiredKeys)
            {
                if (!data.ContainsKey(key))
                    throw new ArgumentException($"Missing required key: {key}");
            }

            // Replace placeholders in the template with corresponding values from the data dictionary
            var resolvedTemplate = template.Template;
            foreach (var kvp in data)
            {
                var placeholder = $"{{{{{kvp.Key}}}}}"; // Placeholder format: {{key}}
                resolvedTemplate = resolvedTemplate.Replace(placeholder, kvp.Value?.ToString() ?? string.Empty);
            }

            return resolvedTemplate;
        }
    }
}

using Microsoft.AspNetCore.Mvc.Formatters;
using System.IO;
using System.Threading.Tasks;

namespace MteDemoTest.Helpers
{
    /// <summary>
    /// Helper used in controller when incoming is text/plain
    /// </summary>
    public class TextPlainInputFormatter : InputFormatter
    {
        private const string ContentType = "text/plain";

        public TextPlainInputFormatter()
        {
            SupportedMediaTypes.Add(ContentType);
        }

        /// <summary>
        /// Read Request to check if content type is text plain
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            using (var reader = new StreamReader(request.Body))
            {
                var content = await reader.ReadToEndAsync();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        /// <summary>
        /// Determine if we can read it or not
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool CanRead(InputFormatterContext context)
        {
            var contentType = context.HttpContext.Request.ContentType;
            return contentType.StartsWith(ContentType);

        }
    }
}

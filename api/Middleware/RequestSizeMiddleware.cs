namespace api.Middleware
{
    public class RequestSizeMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestSizeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.ContentLength > 1024 * 1024) // 1 MB
            {
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                await context.Response.WriteAsync("Payload quá lớn!");
                return;
            }

            var queryString = context.Request.QueryString.Value;
            if (!string.IsNullOrEmpty(queryString) && queryString.Length > 1024) //1 KB
            {
                context.Response.StatusCode = StatusCodes.Status414RequestUriTooLong;
                await context.Response.WriteAsync("Query parameters quá lớn!");
                return;
            }

            await _next(context);
        }
    }

}

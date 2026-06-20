using System.Web;

namespace WebApplication1
{
    public sealed class OwinPassThroughHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
        }
    }
}

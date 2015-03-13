using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ReportHandler
/// </summary>
public class ReportHandler : System.Web.Routing.IRouteHandler
{
    public IHttpHandler GetHttpHandler(System.Web.Routing.RequestContext requestContext)
    {
        HttpHanler httpHandler = new HttpHanler();
        return httpHandler;
    }
    public class HttpHanler : IHttpHandler
    {

        public bool IsReusable{get{return false;}}

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            try
            {
                string _APIToken = null;
                if (context.Request.Headers["API_TOKEN"] != null)
                {
                    _APIToken = context.Request.Headers["API_TOKEN"].ToString();
                }
                else if (context.Request.QueryString["api_token"] != null)
                {
                    _APIToken = context.Request.QueryString["api_token"];
                }

                if (!string.IsNullOrEmpty(_APIToken))
                {
                    APIRequest _ptrackerAPICall = new APIRequest();
                    string _activities = _ptrackerAPICall.GetPivotalTrackerJson(_APIToken);
                    _ptrackerAPICall.GetActivitiesComments(_ptrackerAPICall.GetMyActivities(_activities));
                }
                else
                {
                    context.Response.Write("API KEY FAILED");
                }
            }
            catch (Exception Ex)
            {
                context.Response.Write(Ex.StackTrace);
            }
            

        }
    }
}

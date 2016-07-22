using System;
using System.Linq;
using System.Net;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using tvMaster.Dialogs;
using System.Diagnostics;
using System.Web;
using System.Web.UI.WebControls;
using System.IO;

namespace tvMaster
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        /// 
        /// 
        public static String x = "";
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            StreamWriter file = new StreamWriter("C:\\Logs\\log.txt",false);
            file.WriteLine(activity.Text.ToString());
            file.Close();
            if (activity != null)
            {
                // one of these will have an interface and process it
                x = activity.Text;
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        await Conversation.SendAsync(activity, () => new TraktLuisDialog());
                        break;

                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    default:
                        Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                        break;
                }
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }
    }
}
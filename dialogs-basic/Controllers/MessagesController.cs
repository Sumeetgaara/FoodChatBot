using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace dialogs_basic
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new MathsDialog());
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }
    }

    [Serializable]
    public class MathsDialog : IDialog<object>
    {
        // Bot Framework manages automatically persists per conversation data
        public string place { get; set; }
        public string price { get; set; }

        
        public async Task StartAsync(IDialogContext context)
        {
            
            context.Wait(welcome);
        }
        public async Task welcome(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            await context.PostAsync("Hi Beautiful!I am NMFJBOT .I will recommend you food.Please tell me your city!");
            

            context.Wait(location);
        }

        public async Task location(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var loc = await argument;
            place = Convert.ToString(loc.Text);
            await context.PostAsync("Your budget?500 or 1000?");
            context.Wait(budget);
            
        }

        public async Task budget(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var cost = await argument;
            price = Convert.ToString(cost.Text);

            Models.botEntities DB = new Models.botEntities();
            var Name = (from botTable in DB.botTables
                                    where botTable.Place == place
                                    where botTable.Price == price
                                      select botTable.Name );
            string Name1 = Name.Single();
            var purl = (from botTable in DB.botTables
                        where botTable.Place == place
                        where botTable.Price == price
                        select botTable.PhotoURL);
            string Purl = purl.Single();

            var curl = (from botTable in DB.botTables
                        where botTable.Place == place
                        where botTable.Price == price
                        select botTable.CardURL);
            string Curl = curl.Single();


            var replyMessage = context.MakeMessage();
            replyMessage.Attachments = new List<Attachment>();
            

            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url:Purl));
            List<CardAction> cardButtons = new List<CardAction>();
            CardAction plButton = new CardAction()
            {
                Value = Curl,
                Type = "openUrl",
                Title = "Read"
            };
            cardButtons.Add(plButton);
            HeroCard plCard = new HeroCard()
            {
                Title = Name1,
                Images = cardImages,
                Buttons = cardButtons
            };
            Attachment plAttachment = plCard.ToAttachment();
            replyMessage.Attachments.Add(plAttachment);
            await context.PostAsync(replyMessage);     
            context.Wait(welcome);

        }
    }
}

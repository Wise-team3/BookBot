/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Goodreads;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
namespace BotApplication5.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
         const string ApiKey = "MdkOmbDoxa2vczCWSRJIw";
         const string ApiSecret = "s8JXg023iZaezn9oIpkPWWl5ThQux1HznNOSuX9OLU";
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);

            var activity = await result as Activity;
            /*
                        // Calculate something for us to return
                     // int length = (activity.Text ?? string.Empty).Length;
                        // Goodreads.Models.Response.Book book = await client.Books.GetByBookId(bookId: 15979976);
                        Goodreads.Models.Response.Book book=  await client.Books.GetByTitle(activity.Text);
                        // Get a list of groups by search keyword.
                       // var groups = await client.Groups.GetGroups(search: "Arts");
                        Attachment attachment = new Attachment();
                        attachment.ContentType = "image/jpg";
                        attachment.ContentUrl = book.ImageUrl;
                        var message = context.MakeMessage();
                        message.Attachments.Add(attachment);
                        // Return our reply to the user
                        await context.PostAsync($"{book.Title}");
                        await context.PostAsync(message);
                        await context.PostAsync($"Rating:{book.AverageRating}");
                        // await context.PostAsync($"You sent {activity.Text} which was {length} characters and {book.Title}");*/
// await context.PostAsync($"You sent {activity.Text}");
/*  if (activity.Text.Equals("Hi"))
  {
      var reply = activity.CreateReply("I have few genres in mind,which one would you like .");

      reply.Type = ActivityTypes.Message;
      reply.TextFormat = TextFormatTypes.Plain;
      reply.SuggestedActions = new SuggestedActions()
      {
          Actions = new List<CardAction>()
{
  new CardAction(){ Title = "Romance", Type=ActionTypes.ImBack, Value="Romance" },
new CardAction(){ Title = "Paranormal", Type=ActionTypes.ImBack, Value="Paranormal" },
new CardAction(){ Title = "Sci-fi", Type=ActionTypes.ImBack, Value="Sci-fi" },
new CardAction(){ Title = "Fantasy", Type=ActionTypes.ImBack, Value="Fantasy" },
new CardAction(){ Title = "Suspence", Type=ActionTypes.ImBack, Value="Suspence" }
}
      };
      await context.PostAsync(reply);
      context.Wait(MessageReceivedAsync);
  }
  else
  {
      Goodreads.Models.Response.PaginatedList<Goodreads.Models.Response.Work> sugg = await client.Books.Search(activity.Text,1,Goodreads.Models.Request.BookSearchField.Title);
      int i = 0;
      Attachment attachment = new Attachment();
      attachment.ContentType = "image/jpg";
      Goodreads.Models.Response.Book book;
      while (sugg.List.Count() != i)
      {
          //  await context.PostAsync($"{sugg.List.ElementAt(i).Id}");
          book = await client.Books.GetByBookId(sugg.List.ElementAt(i).Id);
          if (book != null)
          {
              attachment.ContentUrl = book.ImageUrl;
              var message = context.MakeMessage();
              message.Attachments.Add(attachment);
              //   System.Threading.Thread.Sleep(1050);
              await context.PostAsync($"{book.Title}");
              await context.PostAsync(message);
              await context.PostAsync($"Rating:{book.AverageRating}");
          }
          i = i + 1;
          book = null;
      }
      //  await context.PostAsync(reply);
      context.Wait(MessageReceivedAsync);
  }
}
}
}*/
using System;
using System.Threading.Tasks;
//using Goodreads;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;

namespace BotApplication5.Dialogs
{

    [LuisModel("2f0a49fb-a9a3-4203-9185-c3e7eeeee50f", "c272eab449364677a01c67a6d038190a")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        const string ApiKey = "MdkOmbDoxa2vczCWSRJIw";
        const string ApiSecret = "s8JXg023iZaezn9oIpkPWWl5ThQux1HznNOSuX9OLU";
        static string genre = "";
        static float ratings = 0;
        static string author = "";
        int find = 0;
        [LuisIntent("Greetings")]
        public async Task Greet(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {
            await context.PostAsync($"Hi! I'm Nerdy,the Book Bot.");
            
            await context.PostAsync($"I can help you find novels,compare them and can suggest a few too.");

            await context.PostAsync($"Try asking me about a book.");
            context.Wait(this.MessageReceived);

        }
        [LuisIntent("BookSearch")]
        public async Task Search(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            EntityRecommendation booktitle;

           if(res.TryFindEntity("Book",out booktitle))
            {
                booktitle.Type = "Book";
            }
            await context.PostAsync(booktitle.Entity);
            Goodreads.Models.Response.Book book = await client.Books.GetByTitle(booktitle.Entity);
           // await context.PostAsync($"{activity.Text}");

            //   var groups = await client.Groups.GetGroups(search: "Arts");
            Attachment attachment = new Attachment();
            attachment.ContentType = "image/jpg";
            attachment.ContentUrl = book.ImageUrl;
            var mes = context.MakeMessage();
            mes.Attachments.Add(attachment);

            // Return our reply to the user
            await context.PostAsync($"{book.Title}");
            await context.PostAsync(mes);
            await context.PostAsync($"Rating:{book.AverageRating}");
            context.Wait(this.MessageReceived);

        }
        [LuisIntent("Genre")]
        public async Task Genre(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {

            //string message = $"Hello...Try asking me about the books";
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            Goodreads.Models.Response.PaginatedList<Goodreads.Models.Response.Work> sugg = await client.Books.Search(activity.Text, 1, Goodreads.Models.Request.BookSearchField.Title);

            if (find == 0)
            {
                genre = activity.Text;
                string mes = $"Found '{sugg.List.Count()}' books ....Do you want to search by following?";
                find = 1;
                var reply = activity.CreateReply(mes);
                reply.Type = ActivityTypes.Message;
                reply.TextFormat = TextFormatTypes.Plain;
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "Ratings", Type=ActionTypes.ImBack, Value="Ratings" },
                        new CardAction(){ Title = "Author", Type=ActionTypes.ImBack, Value="Author" },
                        new CardAction(){ Title = "All", Type=ActionTypes.ImBack, Value="All" },
                        //new CardAction(){ Title = "Fantasy", Type=ActionTypes.ImBack, Value="Fantasy" },*/
                    }
                };
                await context.PostAsync(reply);
                //context.Wait(this.MessageReceived);
            }
            else if (activity.Text == "All" && find == 1)
            {

                int i = 0;
                Attachment attachment = new Attachment();
                attachment.ContentType = "image/jpg";
                Goodreads.Models.Response.Book book;
                find = 2;
                while (sugg.List.Count() != i)
                {
                    //  await context.PostAsync($"{sugg.List.ElementAt(i).Id}");
                    book = await client.Books.GetByBookId(sugg.List.ElementAt(i).Id);
                    if (book != null)
                    {
                        attachment.ContentUrl = book.ImageUrl;
                        var message = context.MakeMessage();
                        message.Attachments.Add(attachment);
                        //   System.Threading.Thread.Sleep(1050);
                        await context.PostAsync($"{book.Title}");
                        // await context.PostAsync(message);
                        await context.PostAsync($"Rating:{book.AverageRating}");
                    }
                    i = i + 1;
                    book = null;
                }
                //context.Wait(this.MessageReceived);
            }
            else if (activity.Text == "Ratings" && find == 1)
            {
                int i = 0;
                Attachment attachment = new Attachment();
                attachment.ContentType = "image/jpg";
                Goodreads.Models.Response.Book book;
                find = 2;
                var reply = activity.CreateReply("choose the following ratings");
                reply.Type = ActivityTypes.Message;
                reply.TextFormat = TextFormatTypes.Plain;
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = ">1", Type=ActionTypes.ImBack, Value="1" },
                        new CardAction(){ Title = ">2", Type=ActionTypes.ImBack, Value="2" },
                        new CardAction(){ Title = ">3", Type=ActionTypes.ImBack, Value="3" },
                        new CardAction(){ Title = ">4", Type=ActionTypes.ImBack, Value="4" },
                    }
                };
                await context.PostAsync(reply);

                // context.Wait(this.MessageReceived);

            }
            context.Wait(this.MessageReceived);
        }
        [LuisIntent("Ratings")]
        public async Task Ratings(IDialogContext context, IAwaitable<object> result, LuisResult resu)
        {

            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            Goodreads.Models.Response.PaginatedList<Goodreads.Models.Response.Work> sugg = await client.Books.Search(genre, 1, Goodreads.Models.Request.BookSearchField.Title);

            string mes = $"Hello...Try asking me about the books or type 'help' to explore specific genre";
            if (find < 2)
                await context.PostAsync(mes);
            else if (find == 2)
            {
                Attachment attachment = new Attachment();
                attachment.ContentType = "image/jpg";
                Goodreads.Models.Response.Book book;
                int i = 0;
                while (sugg.List.Count() != i)
                {

                    //  await context.PostAsync($"{sugg.List.ElementAt(i).Id}");
                    book = await client.Books.GetByBookId(sugg.List.ElementAt(i).Id);
                    if (book != null && book.AverageRating > Convert.ToDecimal(activity.Text))
                    {
                        attachment.ContentUrl = book.ImageUrl;
                        var message = context.MakeMessage();
                        message.Attachments.Add(attachment);
                        //   System.Threading.Thread.Sleep(1050);
                        await context.PostAsync($"{book.Title}");
                        // await context.PostAsync(message);
                        await context.PostAsync($"Rating:{book.AverageRating}");
                    }
                    i = i + 1;
                    book = null;
                }
                context.Wait(this.MessageReceived);
            }
        }
      
        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {
            string message = $"Sorry,Nothing is found...Try another book";
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            var reply = activity.CreateReply("Hello!...I have few genres in mind,choose the one you like .");
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "Romance", Type=ActionTypes.ImBack, Value="Romance" },
                        new CardAction(){ Title = "Paranormal", Type=ActionTypes.ImBack, Value="Paranormal" },
                        new CardAction(){ Title = "Sci-fi", Type=ActionTypes.ImBack, Value="Sci-fi" },
                        new CardAction(){ Title = "Fantasy", Type=ActionTypes.ImBack, Value="Fantasy" },
                        new CardAction(){ Title = "Suspence", Type=ActionTypes.ImBack, Value="Suspence" }
                    }
            };
            await context.PostAsync(reply);
            context.Wait(this.MessageReceived);

        }
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Hello...Try asking me about the books or type 'help' to explore specifically";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

    }

}
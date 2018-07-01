/*using System;
using System.Threading.Tasks;
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
        static string author = "";
        static int find = 0;
        static Goodreads.Models.Response.PaginatedList<Goodreads.Models.Response.Work> sugg;
        [LuisIntent("Greetings")]
        public async Task Greet(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {
            await context.PostAsync($"Hi! I'm Nerdy,the Book Bot.");

            await context.PostAsync($"I can help you find novels,compare them and can suggest a few too.");

            await context.PostAsync($"Try asking me about a books or ask for 'help' to explore me completely...");
            context.Wait(this.MessageReceived);

        }
        [LuisIntent("BookSearch")]
        public async Task Search(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {
            //*******"search for a book named percy jackson"***********
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;

            //Get the entity of the matched intents 
            EntityRecommendation booktitle;
            if (res.TryFindEntity("Book", out booktitle))
            {
                booktitle.Type = "Book";      // set the type of entity used in LUIS
            }
            string a = activity.Text.Substring((int)booktitle.StartIndex);
            Goodreads.Models.Response.Book book = await client.Books.GetByTitle(a);
            var message = context.MakeMessage();
            message.Attachments = new List<Attachment>
            {
                new ThumbnailCard
                {
                    Title = book.Title,
                    Subtitle = $"Rating:{book.AverageRating}",
                    Text = $"Summary:{book.Description}",
                    Images = new List<CardImage> { new CardImage(book.ImageUrl) },
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "more..", value: "https://www.goodreads.com/search?q="+book.Title) }
                }.ToAttachment()
            };
            await context.PostAsync(message);

            context.Wait(this.MessageReceived);

        }
        public async Task displayAsync(IDialogContext context, IAwaitable<object> result, LuisResult res,Goodreads.IGoodreadsClient client)
        {
            var activity = await result as Activity;
            if (find == 0) { 
            string mes = $"Found {sugg.List.Count()} books ....Do you want to search by following?";
            find = 1;
            var reply = activity.CreateReply(mes);
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "Ratings", Type=ActionTypes.ImBack, Value="Ratings" },
                        new CardAction(){ Title = "All", Type=ActionTypes.ImBack, Value="All" },
                    }
            };
            await context.PostAsync(reply); }
            else if (activity.Text == "All" && find == 1)
            {

                int i = 0;
     
        Goodreads.Models.Response.Book book;
        find = 0;
                List<Attachment> l = new List<Attachment>();
                while (sugg.List.Count() != i)
                {

                    book = await client.Books.GetByBookId(sugg.List.ElementAt(i).BestBook.Id);
                    if (book != null)
                    {

                        l.Add(new ThumbnailCard
                        {
                            Title = book.Title,
                            Subtitle = $"Rating:{book.AverageRating}",
                            Text = $"Summary:{book.Description}",
                            Images = new List<CardImage> { new CardImage(book.ImageUrl) },
                            Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "more..", value: "https://www.goodreads.com/search?q=" + book.Title) }
                        }.ToAttachment());

                    }
                    i = i + 1;
                    book = null;
                }
                var message = context.MakeMessage();
message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                message.Attachments = l;
                await context.PostAsync(message);
sugg = null;

            }
            else if (activity.Text == "Ratings" && find == 1)
            {
                int i = 0;
find = 2;
                var reply = activity.CreateReply("choose from the following ratings");
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
            }
        }
        [LuisIntent("Genre")]
        public async Task Genre(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {

            //*******show me books belonging to paranormal genre************
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            if (find == 0)
            {
                EntityRecommendation gen;

                if (res.TryFindEntity("Genre Type", out gen))
                {
                    gen.Type = "Genre Type";
                }

                sugg = await client.Books.Search(gen.Entity, 1, Goodreads.Models.Request.BookSearchField.Genre);
                genre = activity.Text;

            }
            await displayAsync(context, result, res, client);
            context.Wait(this.MessageReceived);
        }
        
        [LuisIntent("Author")]
        public async Task Author(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {

            //*******show me books belonging to paranormal genre************
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            if (find == 0)
            {
                EntityRecommendation gen;

                if (res.TryFindEntity("author", out gen))
                {
                    gen.Type = "author";
                }
               
                sugg = await client.Books.Search(gen.Entity, 1, Goodreads.Models.Request.BookSearchField.Author);
            }
            await displayAsync(context, result, res, client);
            context.Wait(this.MessageReceived);
        }
        [LuisIntent("Ratings")]
        public async Task Ratings(IDialogContext context, IAwaitable<object> result, LuisResult resu)
        {

            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            string mes = $"Sorry...Try asking me about the books or type 'help' to explore specific genre";
            if (find < 2)
                await context.PostAsync(mes);
            else if (find == 2)
            {
                List<Attachment> l = new List<Attachment>();

                int g = Convert.ToInt16(activity.Text);
               Goodreads.Models.Response.Book book;
                int i = 0;
                while (sugg.List.Count() != i)
                {
                    book = await client.Books.GetByBookId(sugg.List.ElementAt(i).BestBook.Id);
                    if (book != null && book.AverageRating > g)
                    {
                        l.Add(new ThumbnailCard
                        {
                            Title = book.Title,
                            Subtitle = $"Rating:{book.AverageRating}",
                            Text = $"Summary:{book.Description}",
                            Images = new List<CardImage> { new CardImage(book.ImageUrl) },
                            Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "more..", value: "https://www.goodreads.com/search?q=" + book.Title) }
                        }.ToAttachment());
                    }
                    i = i + 1;
                    book = null;
                }
                var message = context.MakeMessage();
                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                message.Attachments = l;
                await context.PostAsync(message);
                find = 0;

            }
            sugg = null;
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {
            string message = $"Sorry,Nothing is found...Try another book";
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            var reply = activity.CreateReply("umm...I have few genres in mind,choose the one you like .");
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "Romance", Type=ActionTypes.ImBack, Value="Search for books belonging to Romance genre" },
                        new CardAction(){ Title = "Paranormal", Type=ActionTypes.ImBack, Value="Search for books belonging to Paranormal genre" },
                        new CardAction(){ Title = "Thriller", Type=ActionTypes.ImBack, Value="Search for books belonging to thriller genre" },
                        new CardAction(){ Title = "Fantasy", Type=ActionTypes.ImBack, Value="Search for books belonging to Fantasy genre" },
                        new CardAction(){ Title = "Crime", Type=ActionTypes.ImBack, Value="Search for books belonging to Crime genre" },
                        new CardAction(){ Title = "Fiction", Type=ActionTypes.ImBack, Value="Search for books belonging to fiction genre" }
                        //new CardAction(){ Title = "Young Adult", Type=ActionTypes.ImBack, Value="Search for books belonging to young-adult genre" }
                    }
            };
            await context.PostAsync(reply);
            context.Wait(this.MessageReceived);

        }
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Hello...Try asking me about the books or type 'help' to explore more specifically";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

    }

}*/


using System;
using System.Threading.Tasks;
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
        static string author = "";
        static int find = 0;
        static Goodreads.Models.Response.PaginatedList<Goodreads.Models.Response.Work> sugg;
        [LuisIntent("Greetings")]
        public async Task Greet(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {
            await context.PostAsync($"Hi! I'm Nerdy,the Book Bot.");

            await context.PostAsync($"I can help you find novels,compare them and can suggest a few too.");

            await context.PostAsync($"Try asking me about a books or ask for 'help' to explore me completely...");
            context.Wait(this.MessageReceived);

        }
        [LuisIntent("BookSearch")]
        public async Task Search(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {
            //*******"search for a book named percy jackson"***********
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;

            //Get the entity of the matched intents 
            EntityRecommendation booktitle;
            if (res.TryFindEntity("Book", out booktitle))
            {
                booktitle.Type = "Book";      // set the type of entity used in LUIS
            }
            string a = activity.Text.Substring((int)booktitle.StartIndex);
            Goodreads.Models.Response.Book book = await client.Books.GetByTitle(a);
            var message = context.MakeMessage();
            message.Attachments = new List<Attachment>
            {
                new ThumbnailCard
                {
                    Title = book.Title,
                    Subtitle = $"Rating:{book.AverageRating}",
                    Text = $"Summary:{book.Description}",
                    Images = new List<CardImage> { new CardImage(book.ImageUrl) },
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "more..", value: "https://www.goodreads.com/search?q="+book.Title) }
                }.ToAttachment()
            };
            await context.PostAsync(message);

            context.Wait(this.MessageReceived);

        }
        public async Task displayAsync(IDialogContext context, IAwaitable<object> result, LuisResult res, Goodreads.IGoodreadsClient client)
        {
            var activity = await result as Activity;
            if (activity.Text == "All" && find == 1)
            {

                int i = 0;

                Goodreads.Models.Response.Book book;
                find = 0;
                List<Attachment> l = new List<Attachment>();
                while (sugg.List.Count() != i)
                {

                    book = await client.Books.GetByBookId(sugg.List.ElementAt(i).BestBook.Id);
                    if (book != null)
                    {

                        l.Add(new ThumbnailCard
                        {
                            Title = book.Title,
                            Subtitle = $"Rating:{book.AverageRating}",
                            Text = $"Summary:{book.Description}",
                            Images = new List<CardImage> { new CardImage(book.ImageUrl) },
                            Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "more..", value: "https://www.goodreads.com/search?q=" + book.Title) }
                        }.ToAttachment());

                    }
                    i = i + 1;
                    book = null;
                }
                var message = context.MakeMessage();
                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                message.Attachments = l;
                await context.PostAsync(message);
                sugg = null;

            }
            else if (activity.Text == "Ratings" && find == 1)
            {
                int i = 0;
                find = 2;
                var reply = activity.CreateReply("choose from the following ratings");
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
            }
            else 
            {
                string mes = $"Found {sugg.List.Count()} books ....Do you want to search by following?";
                find = 1;
                var reply = activity.CreateReply(mes);
                reply.Type = ActivityTypes.Message;
                reply.TextFormat = TextFormatTypes.Plain;
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "Ratings", Type=ActionTypes.ImBack, Value="Ratings" },
                        new CardAction(){ Title = "All", Type=ActionTypes.ImBack, Value="All" },
                    }
                };
                await context.PostAsync(reply);
            }
           
        }
        [LuisIntent("Genre")]
        public async Task Genre(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {

            //*******show me books belonging to paranormal genre************
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            if (find == 0)
            {
                EntityRecommendation gen;

                if (res.TryFindEntity("Genre Type", out gen))
                {
                    gen.Type = "Genre Type";
                }

                sugg = await client.Books.Search(gen.Entity, 1, Goodreads.Models.Request.BookSearchField.Genre);
                genre = activity.Text;

            }
            await displayAsync(context, result, res, client);
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Author")]
        public async Task Author(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {

            //*******show me books belonging to paranormal genre************
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            
          
            EntityRecommendation gen;

                if (res.TryFindEntity("author", out gen))
                {
                    gen.Type = "author";
                }

                sugg = await client.Books.Search(gen.Entity, 1, Goodreads.Models.Request.BookSearchField.Author);
               
            await displayAsync(context, result, res, client);
            context.Wait(this.MessageReceived);
        }
        [LuisIntent("Ratings")]
        public async Task Ratings(IDialogContext context, IAwaitable<object> result, LuisResult resu)
        {

            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            string mes = $"Sorry...Try asking me about the books or type 'help' to explore specific genre";
            if (find < 2)
                await context.PostAsync(mes);
            else if (find == 2)
            {
                List<Attachment> l = new List<Attachment>();

                int g = Convert.ToInt16(activity.Text);
                Goodreads.Models.Response.Book book;
                int i = 0;
                while (sugg.List.Count() != i)
                {
                    book = await client.Books.GetByBookId(sugg.List.ElementAt(i).BestBook.Id);
                    if (book != null && book.AverageRating > g)
                    {
                        l.Add(new ThumbnailCard
                        {
                            Title = book.Title,
                            Subtitle = $"Rating:{book.AverageRating}",
                            Text = $"Summary:{book.Description}",
                            Images = new List<CardImage> { new CardImage(book.ImageUrl) },
                            Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "more..", value: "https://www.goodreads.com/search?q=" + book.Title) }
                        }.ToAttachment());
                    }
                    i = i + 1;
                    book = null;
                }
                var message = context.MakeMessage();
                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                message.Attachments = l;
                await context.PostAsync(message);
                find = 0;

            }
            sugg = null;
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {
            string message = $"Sorry,Nothing is found...Try another book";
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            var reply = activity.CreateReply("umm...I have few genres in mind,choose the one you like .");
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "Romance", Type=ActionTypes.ImBack, Value="Search for books belonging to Romance genre" },
                        new CardAction(){ Title = "Paranormal", Type=ActionTypes.ImBack, Value="Search for books belonging to Paranormal genre" },
                        new CardAction(){ Title = "Thriller", Type=ActionTypes.ImBack, Value="Search for books belonging to thriller genre" },
                        new CardAction(){ Title = "Fantasy", Type=ActionTypes.ImBack, Value="Search for books belonging to Fantasy genre" },
                        new CardAction(){ Title = "Crime", Type=ActionTypes.ImBack, Value="Search for books belonging to Crime genre" },
                        new CardAction(){ Title = "Fiction", Type=ActionTypes.ImBack, Value="Search for books belonging to fiction genre" }
                        //new CardAction(){ Title = "Young Adult", Type=ActionTypes.ImBack, Value="Search for books belonging to young-adult genre" }
                    }
            };
            await context.PostAsync(reply);
            context.Wait(this.MessageReceived);

        }
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Hello...Try asking me about the books or type 'help' to explore more specifically";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

    }

}

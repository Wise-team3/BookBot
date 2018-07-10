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

    [LuisModel("ee3548f8-5472-4bce-9b9e-7c5a5b0beac3", "1bfe00ae7dac44579916f1fea6891ae3")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        const string ApiKey = "MdkOmbDoxa2vczCWSRJIw";
        const string ApiSecret = "s8JXg023iZaezn9oIpkPWWl5ThQux1HznNOSuX9OLU";
        static string genre = "";
        static string author = "";
        int gen_working = 0;

        int author_working = 0;
        static int gen_page = 1;
        static int author_page = 1;
        static int ratings = 0;
        int more_genre = 0;
        int more_author = 0;
        static int find = 0;
        static Goodreads.Models.Response.PaginatedList<Goodreads.Models.Response.Work> sugg;
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Hello...Try asking me about the books or type 'help' to explore more specifically";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }
        [LuisIntent("Greetings")]
        public async Task Greet(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {
            await context.PostAsync($"Hi! I'm Nerdy,the Book Bot.");

            await context.PostAsync($"I can help you find novels based on their names,author and can suggest a few based on genre.");

            await context.PostAsync($"Try asking me about a books or ask for 'help' to get started ...");
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
        public async Task displayMore(IDialogContext context, IAwaitable<object> result, LuisResult res, Goodreads.IGoodreadsClient client)
        {
            if (more_genre == 1 || more_author == 1)
            {
                var activity = await result as Activity;
                Goodreads.Models.Response.Book book;
                int i = 0;
                List<Attachment> l = new List<Attachment>();
                while (i < 10 && sugg.List.Count() > i)
                {

                    book = await client.Books.GetByBookId(sugg.List.ElementAt(i).BestBook.Id);
                    if (book != null && book.AverageRating > ratings)
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
                var reply = activity.CreateReply("Want more..?!");
                reply.Type = ActivityTypes.Message;
                reply.TextFormat = TextFormatTypes.Plain;
                if (more_genre == 1)
                {

                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "show more genre books", Type=ActionTypes.ImBack, Value="more genre books" },

                    }
                    };
                }
                else if (more_author == 1)
                {
                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "show more author books", Type=ActionTypes.ImBack, Value="more author books" },

                    }
                    };
                }
                await context.PostAsync(message);
                await context.PostAsync(reply);
                sugg = null;
            }

        }
        public async Task displayAsync(IDialogContext context, IAwaitable<object> result, LuisResult res, Goodreads.IGoodreadsClient client)
        {
            var activity = await result as Activity;
            // await context.PostAsync($"display gen ={gen_working} auth={author_working}");
            if (activity.Text == "All" && find == 1)
            {

                int i = 0;
                ratings = 0;
                Goodreads.Models.Response.Book book;
                find = 0;
                List<Attachment> l = new List<Attachment>();
                while (i < 10 && sugg.List.Count() > i)
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
                var reply = activity.CreateReply("Want more...?!");
                reply.Type = ActivityTypes.Message;
                reply.TextFormat = TextFormatTypes.Plain;
                if (gen_working == 1)
                {


                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "show more genre books", Type=ActionTypes.ImBack, Value="more genre books" },

                    }
                    };
                }
                else if (author_working == 1)
                {
                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "show more author books", Type=ActionTypes.ImBack, Value="more author books" },

                    }
                    };
                }
                await context.PostAsync(message);
                await context.PostAsync(reply);
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
                string mes = $"Found few interesting books from good reads ....Do you want to search by following?";
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
            if (activity.Text != "All" && activity.Text != "Ratings")
            {
                author_working = 0;
                gen_working = 1;
            }
            if (activity.Text == "more genre books")
            {
                sugg = await client.Books.Search(genre, ++gen_page, Goodreads.Models.Request.BookSearchField.Genre);
                more_genre = 1;
                //await context.PostAsync($"inside more{genre} and page= {gen_page} ratings={ratings}");
                await displayMore(context, result, res, client);
                more_genre = 0;
            }
            else
            {
                if (find == 0)
                {

                    EntityRecommendation gen;

                    if (res.TryFindEntity("Genre Type", out gen))
                    {
                        gen.Type = "Genre Type";
                    }

                    sugg = await client.Books.Search(gen.Entity, 1, Goodreads.Models.Request.BookSearchField.Genre);
                    genre = gen.Entity;
                }
                await displayAsync(context, result, res, client);

            }
            //await displayAsync(context, result, res, client);
            context.Wait(this.MessageReceived);
        }
      
        [LuisIntent("Author")]
        public async Task Author(IDialogContext context, IAwaitable<object> result, LuisResult res)
        {
            //auth_find = 1;
            //*******show me books belonging to paranormal genre************
            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            gen_working = 0;
            author_working = 1;
            if (activity.Text == "more author books")
            {
                more_author = 1;
                sugg = await client.Books.Search(author, ++author_page, Goodreads.Models.Request.BookSearchField.Author);
                await displayMore(context, result, res, client);
                more_author = 0;
            }
            else
            {
                EntityRecommendation gen;

                if (res.TryFindEntity("author", out gen))
                {
                    gen.Type = "author";
                }

                sugg = await client.Books.Search(gen.Entity, 1, Goodreads.Models.Request.BookSearchField.Author);
                author = gen.Entity;
                await displayAsync(context, result, res, client);
                //   await context.PostAsync($"inside author gen={gen_working},auth={author_working}");
            }
            context.Wait(this.MessageReceived);
        }
        [LuisIntent("Ratings")]
        public async Task Ratings(IDialogContext context, IAwaitable<object> result, LuisResult resu)
        {

            var client = Goodreads.GoodreadsClient.Create(ApiKey, ApiSecret);
            var activity = await result as Activity;
            // await context.PostAsync($"inside ratings author={author_working}, genre={gen_working}");
            string mes = $"Sorry...Try asking me about the books or type 'help' to explore specific genre";
            if (find < 2)
                await context.PostAsync(mes);
            else if (find == 2)
            {
                List<Attachment> l = new List<Attachment>();

                int g = Convert.ToInt16(activity.Text);
                Goodreads.Models.Response.Book book;
                ratings = g;
                int i = 0;
                while (i < 10 && sugg.List.Count() > i)
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
                var reply = activity.CreateReply("Want more...?! ");
                reply.Type = ActivityTypes.Message;
                reply.TextFormat = TextFormatTypes.Plain;
                if (gen_working == 1)
                {
                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "show more genre books", Type=ActionTypes.ImBack, Value="more genre books" },

                    }
                    };
                }
                else if (author_working == 1)
                {
                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                    {
                        new CardAction(){ Title = "show more author books", Type=ActionTypes.ImBack, Value="more author books" },

                    }
                    };
                }

                await context.PostAsync(message);
                await context.PostAsync(reply);
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
        
    }

}

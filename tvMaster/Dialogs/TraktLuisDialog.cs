using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Threading.Tasks;
using System.Net.Http;
using tvMaster.Models;
using System.IO;

namespace tvMaster.Dialogs
{
    [LuisModel("45445747-b7e1-4087-ba7d-5c10a08620c1", "e686ad6ca03b4cf29d26288783935568")]
    [Serializable]
    public class TraktLuisDialog : LuisDialog<object>
    {
        public static String category = "";
        public TraktLuisDialog()
        {
        }

        public TraktLuisDialog(ILuisService service) : base(service)
        {
        }
        public String pathGenerater(String slug)
        {
            String path = "movies/" + slug + "/comments";
            return path;
        }
        public String getMovieComments(String slug)
        {
            String movieComment = "";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");
                string k = pathGenerater(slug);
                var response = client.GetAsync(pathGenerater(slug)).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<comments>>(responseString);

                for (int i = 0; i < responseJSON.Count; i++)
                {
                    movieComment += $"{Environment.NewLine}{Environment.NewLine} - " + responseJSON[i].user.username + $"{Environment.NewLine}{Environment.NewLine} > - " + responseJSON[i].updated_at + $"{Environment.NewLine}{Environment.NewLine} > - " + responseJSON[i].comment;
                }
            }
            if (movieComment.Equals(""))
            {
                movieComment = "No comments";
            }
            return movieComment;
        }


        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            int count = 0;
            String output = "Search results";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");
                if (result.Query.ToLower().ToString().Contains("movie") || (!category.Equals("")))
                {
                    if (result.Query.ToLower().ToString().Contains("trending") || category.Equals("trending"))
                    {
                        var response = await client.GetAsync("movies/trending");
                        var responseString = await response.Content.ReadAsStringAsync();
                        var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TrendingMovie>>(responseString);
                        if (category.Equals("trending"))
                        {
                            for (int i = 0; i < responseJSON.Count; i++)
                            {
                                count++;
                                if ((responseJSON[i].movie.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].movie.title.ToLower().ToString())) || (count + "").Equals(result.Query.ToString()))
                                {
                                    output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].movie.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].movie.year + $"{Environment.NewLine}{Environment.NewLine} > - watchers " + responseJSON[i].watchers + $"{Environment.NewLine}{Environment.NewLine}" + getMovieComments(responseJSON[i].movie.ids.slug.ToString());
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < responseJSON.Count; i++)
                            {
                                if (responseJSON[i].movie.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].movie.title.ToLower().ToString()))
                                {
                                    count++;
                                    output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].movie.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].movie.year + $"{Environment.NewLine}{Environment.NewLine} > - watchers " + responseJSON[i].watchers;
                                }
                            }
                        }
                    }
                    else if (result.Query.ToLower().ToString().Contains("popular") || category.Equals("popular"))
                    {
                        using (var clients = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
                        {
                            clients.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                            var response = await clients.GetAsync("movies/popular");
                            var responseString = await response.Content.ReadAsStringAsync();
                            var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Movie>>(responseString);

                            if (category.Equals("popular"))
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {
                                    count++;
                                    if ((responseJSON[i].title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].title.ToLower().ToString())) || (count + "").Equals(result.Query.ToString()))
                                    {
                                        output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].year + $"{Environment.NewLine}{Environment.NewLine}" + getMovieComments(responseJSON[i].ids.slug.ToString());
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {
                                    if (responseJSON[i].title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].title.ToLower().ToString()))
                                    {
                                        count++;
                                        output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].year;
                                    }
                                }
                            }
                        }
                    }
                    else if (result.Query.ToLower().ToString().Contains("anticipated") || category.Equals("anticipated"))
                    {
                        using (var clients = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
                        {
                            clients.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                            var response = await clients.GetAsync("movies/anticipated");
                            var responseString = await response.Content.ReadAsStringAsync();
                            var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnticipatedMovies>>(responseString);
                            if (category.Equals("anticipated"))
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {
                                    count++;
                                    if ((responseJSON[i].movie.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].movie.title.ToLower().ToString())) || (count + "").Equals(result.Query.ToString()))
                                    {
                                        output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].movie.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].movie.year + $"{Environment.NewLine}{Environment.NewLine}" + getMovieComments(responseJSON[i].movie.ids.slug.ToString());
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {
                                    if (responseJSON[i].movie.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].movie.title.ToLower().ToString()))
                                    {
                                        count++;
                                        output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].movie.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].movie.year;
                                    }
                                }
                            }
                        }
                    }
                    else if (result.Query.ToLower().ToString().Contains("most played") || category.Equals("most played"))
                    {
                        using (var clients = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
                        {
                            clients.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                            var response = await client.GetAsync("movies/played");
                            var responseString = await response.Content.ReadAsStringAsync();
                            var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MostPlayedMovies>>(responseString);
                            if (category.Equals("most played"))
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {
                                    count++;
                                    if ((responseJSON[i].movie.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].movie.title.ToLower().ToString())) || (count + "").Equals(result.Query.ToString()))
                                    {
                                        output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].movie.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].movie.year + $"{Environment.NewLine}{Environment.NewLine} > - watcher: " + responseJSON[i].watcher_count + $"{Environment.NewLine}{Environment.NewLine} > - plays: " + responseJSON[i].play_count + $"{Environment.NewLine}{Environment.NewLine} > - collected: " + responseJSON[i].collected_count+ $"{Environment.NewLine}{Environment.NewLine}"+getMovieComments(responseJSON[i].movie.ids.slug);
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {
                                    if (responseJSON[i].movie.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].movie.title.ToLower().ToString()))
                                    {
                                        count++;
                                        output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].movie.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].movie.year + $"{Environment.NewLine}{Environment.NewLine} > - watcher: " + responseJSON[i].watcher_count + $"{Environment.NewLine}{Environment.NewLine} > - plays: " + responseJSON[i].play_count + $"{Environment.NewLine}{Environment.NewLine} > - collected: " + responseJSON[i].collected_count;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var response = await client.GetAsync("movies/trending");
                        var responseString = await response.Content.ReadAsStringAsync();
                        var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TrendingMovie>>(responseString);
                        int trendCount = 0;
                        for (int i = 0; i < responseJSON.Count; i++)
                        {
                            if (responseJSON[i].movie.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].movie.title.ToLower().ToString()))
                            {
                                if (trendCount == 0)
                                {
                                    output += $"{Environment.NewLine}{Environment.NewLine} Trending movies:";
                                    trendCount++;
                                }
                                count++;
                                output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].movie.title;
                            }

                        }

                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        var responses = await client.GetAsync("movies/popular");
                        var responseStrings = await responses.Content.ReadAsStringAsync();
                        var responseJSONs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Movie>>(responseStrings);
                        int popCount = 0;
                        for (int i = 0; i < responseJSON.Count; i++)
                        {
                            if (responseJSONs[i].title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONs[i].title.ToLower().ToString()))
                            {
                                if (popCount == 0)
                                {
                                    output += $"{Environment.NewLine}{Environment.NewLine} Populal movies:";
                                    popCount++;
                                }
                                count++;
                                output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSONs[i].title;
                            }

                        }


                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        var responseAnticipated = await client.GetAsync("movies/anticipated");
                        var responseStringAnticipated = await responseAnticipated.Content.ReadAsStringAsync();
                        var responseJSONAnticipated = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnticipatedMovies>>(responseStringAnticipated);
                        int antCount = 0;
                        for (int i = 0; i < responseJSON.Count; i++)
                        {
                            if (responseJSONAnticipated[i].movie.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONAnticipated[i].movie.title.ToLower().ToString()))
                            {
                                if (antCount == 0)
                                {
                                    output += $"{Environment.NewLine}{Environment.NewLine} Anticipated movies:";
                                    antCount++;
                                }
                                count++;
                                output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSONAnticipated[i].movie.title;
                            }

                        }

                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        var responsePlayed = await client.GetAsync("movies/played");
                        var responseStringPlayed = await responsePlayed.Content.ReadAsStringAsync();
                        var responseJSONPlayed = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnticipatedMovies>>(responseStringPlayed);
                        int playedCount = 0;
                        for (int i = 0; i < responseJSON.Count; i++)
                        {
                            if (responseJSONPlayed[i].movie.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONPlayed[i].movie.title.ToLower().ToString()))
                            {
                                if (playedCount == 0)
                                {
                                    output += $"{Environment.NewLine}{Environment.NewLine} Anticipated movies:";
                                    playedCount++;
                                }
                                count++;
                                output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSONPlayed[i].movie.title;
                            }

                        }
                    }
                }
                else //This needs to be visited. the code bellow is a dupplication of the code above
                {
                    var response = await client.GetAsync("movies/trending");
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TrendingMovie>>(responseString);
                    int trendCount = 0;
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        if (responseJSON[i].movie.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].movie.title.ToLower().ToString()))
                        {
                            if (trendCount == 0)
                            {
                                output += $"{Environment.NewLine}{Environment.NewLine} Trending movies:";
                                trendCount++;
                            }
                            count++;
                            output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].movie.title;
                        }

                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    var responses = await client.GetAsync("movies/popular");
                    var responseStrings = await responses.Content.ReadAsStringAsync();
                    var responseJSONs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Movie>>(responseStrings);
                    int popCount = 0;
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        if (responseJSONs[i].title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONs[i].title.ToLower().ToString()))
                        {
                            if (popCount == 0)
                            {
                                output += $"{Environment.NewLine}{Environment.NewLine} Populal movies:";
                                popCount++;
                            }
                            count++;
                            output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSONs[i].title;
                        }

                    }


                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    var responseAnticipated = await client.GetAsync("movies/anticipated");
                    var responseStringAnticipated = await responseAnticipated.Content.ReadAsStringAsync();
                    var responseJSONAnticipated = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnticipatedMovies>>(responseStringAnticipated);
                    int antCount = 0;
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        if (responseJSONAnticipated[i].movie.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONAnticipated[i].movie.title.ToLower().ToString()))
                        {
                            if (antCount == 0)
                            {
                                output += $"{Environment.NewLine}{Environment.NewLine} Anticipated movies:";
                                antCount++;
                            }
                            count++;
                            output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSONAnticipated[i].movie.title;
                        }

                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    var responsePlayed = await client.GetAsync("movies/played");
                    var responseStringPlayed = await responsePlayed.Content.ReadAsStringAsync();
                    var responseJSONPlayed = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnticipatedMovies>>(responseStringPlayed);
                    int playedCount = 0;
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        if (responseJSONPlayed[i].movie.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONPlayed[i].movie.title.ToLower().ToString()))
                        {
                            if (playedCount == 0)
                            {
                                output += $"{Environment.NewLine}{Environment.NewLine} Anticipated movies:";
                                playedCount++;
                            }
                            count++;
                            output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSONPlayed[i].movie.title;
                        }

                    }

                }
            }
            if (output.Equals("Search results"))
            {
                output = "Sorry I did not understand your query. Please try to be more obvious...";
            }
            string message = output;
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("intent.tvMaster.movies.trending")]
        public async Task Trending(IDialogContext context, LuisResult result)
        {
            int count = 0;
            Boolean yes = false;
            String output = "Trending movies";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                var response = await client.GetAsync("movies/trending");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TrendingMovie>>(responseString);
                if ((result.Query.ToLower().ToString().Contains("search") || result.Query.ToLower().ToString().Contains("find")) && (!(result.Query.ToLower().ToString().Contains("list trending") || result.Query.ToLower().ToString().Contains("find trending") || result.Query.ToLower().ToString().Contains("find movies that are trending") || result.Query.ToLower().ToString().Contains("list movies that are trending") || result.Query.ToLower().ToString().Contains("list of movies that are trending") || result.Query.ToLower().ToString().Contains("get trending") || result.Query.ToLower().ToString().Contains("get movies that are trending"))))
                {
                    await None(context, result);
                }
                else
                {
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        count++;
                        output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].movie.title + $"{Environment.NewLine}{Environment.NewLine} > - year " + responseJSON[i].movie.year;
                        yes = true;
                    }
                    if (yes)
                    {
                        string message = $"" + output;
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        category = "trending";
                    }
                    else
                    {
                        string message = $"No result found";
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        category = "";
                    }
                }
                for (int i = 0; i < responseJSON.Count; i++)
                {
                    count++;
                    output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].movie.title;
                }
            }
        }
        [LuisIntent("intent.tvMaster.movie.popular")]
        public async Task Popular(IDialogContext context, LuisResult result)
        {
            int count = 0;
            Boolean yes = false;
            String output = "Popular movies";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                var response = await client.GetAsync("movies/popular");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Movie>>(responseString);
                if ((result.Query.ToLower().ToString().Contains("search") || result.Query.ToLower().ToString().Contains("find")) && (!(result.Query.ToLower().ToString().Contains("list popular") || result.Query.ToLower().ToString().Contains("find popular") || result.Query.ToLower().ToString().Contains("find movies that are popular") || result.Query.ToLower().ToString().Contains("list movies that are popular") || result.Query.ToLower().ToString().Contains("list of movies that are popular") || result.Query.ToLower().ToString().Contains("get popular") || result.Query.ToLower().ToString().Contains("get movies that are popular"))))
                {
                    await None(context, result);
                }
                else
                {
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        count++;
                        output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].title + $"{Environment.NewLine}{Environment.NewLine} > - year " + responseJSON[i].year;
                        yes = true;

                    }
                    if (yes)
                    {
                        string message = $"" + output;
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        category = "popular";
                    }
                    else
                    {
                        string message = $"No result found";
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        category = "";
                    }

                }

            }


        }

        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [LuisIntent("intent.tvMaster.movies.mostplayed")]
        public async Task MostPlayed(IDialogContext context, LuisResult result)
        {
            Boolean yes = false;
            int count = 0;
            String output = "Most Played movies";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                var response = await client.GetAsync("movies/played");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MostPlayedMovies>>(responseString);
                if ((result.Query.ToLower().ToString().Contains("search") || result.Query.ToLower().ToString().Contains("find")) && (!(result.Query.ToLower().ToString().Contains("list most played") || result.Query.ToLower().ToString().Contains("find most played") || result.Query.ToLower().ToString().Contains("find movies that are played most") || result.Query.ToLower().ToString().Contains("list movies that are most played") || result.Query.ToLower().ToString().Contains("list of movies that are most played") || result.Query.ToLower().ToString().Contains("get most played") || result.Query.ToLower().ToString().Contains("get movies that are played most"))))
                {
                    await None(context, result);
                }
                else
                {
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        count++;
                        output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].movie.title;
                        yes = true;
                    }
                    if (yes)
                    {
                        string message = $"" + output;
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        category = "most played";
                    }
                    else
                    {
                        string message = $"No result found";
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        category = "";
                    }
                }
            }
        }

        [LuisIntent("intent.tvMaster.movies.anticipated")]
        public async Task Anticipated(IDialogContext context, LuisResult result)
        {
            Boolean yes = false;
            int count = 0;
            String output = "Most Anticipated movies";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                var response = await client.GetAsync("movies/anticipated");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnticipatedMovies>>(responseString);
                if ((result.Query.ToLower().ToString().Contains("search") || result.Query.ToLower().ToString().Contains("find")) && (!(result.Query.ToLower().ToString().Contains("list anticipated") || result.Query.ToLower().ToString().Contains("find anticipated") || result.Query.ToLower().ToString().Contains("find movies that are anticipated") || result.Query.ToLower().ToString().Contains("list movies that are most anticipated") || result.Query.ToLower().ToString().Contains("list of movies that are anticipated most") || result.Query.ToLower().ToString().Contains("get anticipated") || result.Query.ToLower().ToString().Contains("get movies that are most anticipated"))))
                {
                    await None(context, result);
                }
                else
                {
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        count++;
                        output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].movie.title;
                        yes = true;
                    }
                    if (yes)
                    {
                        string message = $"" + output;
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        category = "anticipated";
                    }
                    else
                    {
                        string message = $"No result found";
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        category = "";
                    }
                }

            }
        }






        //Everything form here going down is for shows and maybe search...

        [LuisIntent("intent.tvMaster.shows.trending")]
        public async Task TrendingShow(IDialogContext context, LuisResult result)
        {
            int count = 0;
            String output = "Trending Shows";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                var response = await client.GetAsync("shows/trending");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TrendingShows>>(responseString);
                for (int i = 0; i < responseJSON.Count; i++)
                {
                    count++;
                    output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].show.title;
                }
            }
            string message = $"" + output;
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("intent.tvMaster.shows.popular")]
        public async Task PopularShow(IDialogContext context, LuisResult result)
        {
            int count = 0;
            String output = "Popular Shows";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                var response = await client.GetAsync("shows/popular");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Show>>(responseString);
                for (int i = 0; i < responseJSON.Count; i++)
                {
                    count++;
                    output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].title;
                }
            }
            string message = $"" + output;
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("intent.tvMaster.shows.mostplayed")]
        public async Task MostPlayedShow(IDialogContext context, LuisResult result)
        {
            int count = 0;
            String output = "Popular Shows";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                var response = await client.GetAsync("shows/played");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PlayedShows>>(responseString);
                for (int i = 0; i < responseJSON.Count; i++)
                {
                    count++;
                    output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].show.title;
                }
            }
            string message = $"" + output;
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("intent.tvMaster.shows.anticipated")]
        public async Task AnticipatedShow(IDialogContext context, LuisResult result)
        {
            int count = 0;
            String output = "Anticipated Shows";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                var response = await client.GetAsync("shows/anticipated");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnticipatedShows>>(responseString);
                for (int i = 0; i < responseJSON.Count; i++)
                {
                    count++;
                    output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].show.title;
                }
            }
            string message = $"" + output;
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        //[LuisIntent("intent.tvMaster.movies.search")]
        //public async Task SearchMovie(IDialogContext context, LuisResult result)
        //{
        //    StreamReader file = new StreamReader("C:\\Logs\\log.txt", true);
        //    String activity_message = file.ReadLine();

        //    String movieName = activity_message.Substring(activity_message.IndexOf('^') + 2);
        //    int count = 0;
        //    String output = "Results";
        //    using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
        //    {
        //        client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

        //        var response = await client.GetAsync("movies/anticipated");
        //        var responseString = await response.Content.ReadAsStringAsync();
        //        var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnticipatedMovies>>(responseString);
        //        for (int i = 0; i < responseJSON.Count; i++)
        //        {
        //            if (movieName.ToLower().ToString().Contains(responseJSON[i].movie.title) || responseJSON[i].movie.title.ToLower().ToString().Contains(movieName.ToLower().ToString()))
        //            {
        //                count++;
        //                output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].movie.title;
        //            }

        //        }
        //    }


        //    String message = output;
        //    await context.PostAsync(message);
        //    context.Wait(MessageReceived);
        //    file.Close();
        //}
        //[LuisIntent("intent.tvMaster.shows.search")]
        //public async Task SearchShow(IDialogContext context, LuisResult result)
        //{
        //    StreamReader file = new StreamReader("C:\\Logs\\log.txt", true);
        //    String activity_message = file.ReadLine();

        //    String movieName = activity_message.Substring(activity_message.IndexOf("find a show clalled") + 2);
        //    if (activity_message.IndexOf("find a show clalled") < 0)
        //    {
        //        movieName = activity_message.Substring(activity_message.IndexOf("find a show titled") + 2);
        //    }
        //    int count = 0;
        //    String output = "Resultss";
        //    using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
        //    {
        //        client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

        //        var response = await client.GetAsync("shows/popular");
        //        var responseString = await response.Content.ReadAsStringAsync();
        //        var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Show>>(responseString);
        //        for (int i = 0; i < responseJSON.Count; i++)
        //        {
        //            if (movieName.ToLower().ToString().Contains(responseJSON[i].title) || responseJSON[i].title.ToLower().ToString().Contains(movieName.ToLower().ToString()))
        //            {
        //                count++;
        //                output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].title;
        //            }

        //        }
        //    }


        //    String message = output;
        //    await context.PostAsync(message);
        //    context.Wait(MessageReceived);
        //    file.Close();
        //}



    }
}
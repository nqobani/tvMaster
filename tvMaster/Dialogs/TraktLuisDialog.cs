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
        //for movies
        public String pathGenerater(String slug)
        {
            String path = "movies/" + slug + "/comments";
            return path;
        }
        //for shows
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
        //path generater for shows
        public String pathGeneraterShows(String slug)
        {
            String path = "shows/" + slug + "/comments";
            return path;
        }
        //for shows
        public String getShowsComments(String slug)
        {
            String movieComment = "";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");
                string k = pathGeneraterShows(slug);
                var response = client.GetAsync(pathGeneraterShows(slug)).Result;
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
                if (result.Query.ToLower().ToString().Contains("movie") || (category.Equals("popular") || category.Equals("trending") || category.Equals("most played") || category.Equals("anticipated")))
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
                    else if (result.Query.ToLower().ToString().Contains("most played") || result.Query.ToLower().ToString().Contains("played most") || category.Equals("most played"))
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
                                        output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].movie.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].movie.year + $"{Environment.NewLine}{Environment.NewLine} > - watcher: " + responseJSON[i].watcher_count + $"{Environment.NewLine}{Environment.NewLine} > - plays: " + responseJSON[i].play_count + $"{Environment.NewLine}{Environment.NewLine} > - collected: " + responseJSON[i].collected_count + $"{Environment.NewLine}{Environment.NewLine}" + getMovieComments(responseJSON[i].movie.ids.slug);
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
                else if (result.Query.ToLower().ToString().Contains("shows") || (!(category.Equals("popular") || category.Equals("trending") || category.Equals("most played") || category.Equals("anticipated") || category.Equals(""))))
                {
                    if (result.Query.ToLower().ToString().Contains("trending") || category.Equals("trending shows"))
                    {
                        using (var clients = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
                        {
                            clients.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                            var response = await clients.GetAsync("shows/trending");
                            var responseString = await response.Content.ReadAsStringAsync();
                            var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TrendingShows>>(responseString);
                            if (category.Equals("trending shows"))
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {
                                    count++;
                                    if ((responseJSON[i].show.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].show.title.ToLower().ToString())) || (count + "").Equals(result.Query.ToString()))
                                    {
                                        output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].show.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].show.year + $"{Environment.NewLine}{Environment.NewLine} > - watchers " + responseJSON[i].watchers + $"{Environment.NewLine}{Environment.NewLine}" + getShowsComments(responseJSON[i].show.ids.slug.ToString());
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {

                                    if ((responseJSON[i].show.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].show.title.ToLower().ToString())))
                                    {
                                        count++;
                                        output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].show.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].show.year + $"{Environment.NewLine}{Environment.NewLine} > - watchers " + responseJSON[i].watchers;
                                    }
                                }
                            }

                        }
                    }
                    else if (result.Query.ToLower().ToString().Contains("popular") || category.Equals("popular shows"))
                    {
                        using (var clients = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
                        {
                            clients.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                            var response = await clients.GetAsync("shows/popular");
                            var responseString = await response.Content.ReadAsStringAsync();
                            var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Show>>(responseString);
                            if (category.Equals("popular shows"))
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {
                                    count++;
                                    if ((responseJSON[i].title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].title.ToLower().ToString())) || (count + "").Equals(result.Query.ToString()))
                                    {
                                        output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].year + getShowsComments(responseJSON[i].ids.slug.ToString());
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {
                                    if ((responseJSON[i].title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].title.ToLower().ToString())))
                                    {
                                        count++;
                                        output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].year + getShowsComments(responseJSON[i].ids.slug.ToString());
                                    }
                                }
                            }

                        }
                    }
                    else if (result.Query.ToLower().ToString().Contains("anticipated") || category.Equals("anticipated shows"))
                    {
                        using (var clients = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
                        {
                            clients.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                            var response = await clients.GetAsync("shows/anticipated");
                            var responseString = await response.Content.ReadAsStringAsync();
                            var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnticipatedShows>>(responseString);
                            if (category.Equals("anticipated shows"))
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {
                                    count++;
                                    if ((responseJSON[i].show.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].show.title.ToLower().ToString())) || (count + "").Equals(result.Query.ToString()))
                                    {
                                        output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].show.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].show.year + $"{Environment.NewLine}{Environment.NewLine} > - List count: " + responseJSON[i].list_count + getShowsComments(responseJSON[i].show.ids.slug.ToString()) + $"{Environment.NewLine}{Environment.NewLine} > - List count: ";
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {

                                    if ((responseJSON[i].show.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].show.title.ToLower().ToString())))
                                    {
                                        count++;
                                        output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].show.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].show.year + getShowsComments(responseJSON[i].show.ids.slug.ToString());
                                    }
                                }
                            }
                        }
                    }
                    else if (result.Query.ToLower().ToString().Contains("most played") || result.Query.ToLower().ToString().Contains("played most") || category.Equals("most played shows"))
                    {
                        using (var clients = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
                        {
                            clients.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                            var response = await clients.GetAsync("shows/played");
                            var responseString = await response.Content.ReadAsStringAsync();
                            var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PlayedShows>>(responseString);
                            if (category.Equals("most played shows"))
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {
                                    count++;
                                    if ((responseJSON[i].show.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].show.title.ToLower().ToString())) || (count + "").Equals(result.Query.ToString()))
                                    {
                                        output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].show.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].show.year + $"{Environment.NewLine}{Environment.NewLine} > - Play count: " + responseJSON[i].play_count + getShowsComments(responseJSON[i].show.ids.slug.ToString()) + $"{Environment.NewLine}{Environment.NewLine} > - List count: ";
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < responseJSON.Count; i++)
                                {

                                    if ((responseJSON[i].show.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].show.title.ToLower().ToString())))
                                    {
                                        count++;
                                        output += $"{Environment.NewLine}{Environment.NewLine} " + count + ")" + responseJSON[i].show.title + $"{Environment.NewLine}{Environment.NewLine} > - year: " + responseJSON[i].show.year + $"{Environment.NewLine}{Environment.NewLine} > - Play count: " + responseJSON[i].play_count + getShowsComments(responseJSON[i].show.ids.slug.ToString()) + $"{Environment.NewLine}{Environment.NewLine} > - List count: ";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var response = await client.GetAsync("shows/trending");
                        var responseString = await response.Content.ReadAsStringAsync();
                        var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TrendingShows>>(responseString);
                        int countTrendingShows = 0;
                        for (int i = 0; i < responseJSON.Count; i++)
                        {
                            if (responseJSON[i].show.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSON[i].show.title.ToLower().ToString()))
                            {
                                if (countTrendingShows == 0)
                                {
                                    output += $"{Environment.NewLine}{Environment.NewLine} Trending Shows:";
                                    countTrendingShows++;
                                }
                                count++;
                                output += $"{Environment.NewLine}{Environment.NewLine} > " + responseJSON[i].show.title + $"{Environment.NewLine}{Environment.NewLine} > year: " + responseJSON[i].show.year;
                            }
                        }

                        var responsePopularShows = await client.GetAsync("shows/popular");
                        var responseStringPopularShows = await responsePopularShows.Content.ReadAsStringAsync();
                        var responseJSONPopularShows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Show>>(responseStringPopularShows);
                        int countPopularShows = 0;
                        for (int i = 0; i < responseJSON.Count; i++)
                        {
                            if (responseJSONPopularShows[i].title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONPopularShows[i].title.ToLower().ToString()))
                            {
                                if (countPopularShows == 0)
                                {
                                    output += $"{Environment.NewLine}{Environment.NewLine} Popular Shows:";
                                    countPopularShows++;
                                }
                                count++;
                                output += $"{Environment.NewLine}{Environment.NewLine} > " + responseJSONPopularShows[i].title + $"{Environment.NewLine}{Environment.NewLine} > year: " + responseJSONPopularShows[i].year;
                            }
                        }

                        var responseAnticipatedShows = await client.GetAsync("shows/anticipated");
                        var responseStringAnticipatedShows = await responseAnticipatedShows.Content.ReadAsStringAsync();
                        var responseJSONAnticipatedShows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnticipatedShows>>(responseStringAnticipatedShows);
                        int countAnticipatedShows = 0;

                        for (int i = 0; i < responseJSON.Count; i++)
                        {
                            if (responseJSONAnticipatedShows[i].show.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONAnticipatedShows[i].show.title.ToLower().ToString()))
                            {
                                if (countAnticipatedShows == 0)
                                {
                                    output += $"{Environment.NewLine}{Environment.NewLine} Anticipated Shows:";
                                    countAnticipatedShows++;
                                }
                                count++;
                                output += $"{Environment.NewLine}{Environment.NewLine} > " + responseJSONAnticipatedShows[i].show.title + $"{Environment.NewLine}{Environment.NewLine} > year: " + responseJSONAnticipatedShows[i].show.year;
                            }
                        }

                        var responsePlayedShows = await client.GetAsync("shows/played");
                        var responseStringPlayedShows = await responsePlayedShows.Content.ReadAsStringAsync();
                        var responseJSONPlayedShows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PlayedShows>>(responseStringPlayedShows);
                        int countPlayedShows = 0;

                        for (int i = 0; i < responseJSON.Count; i++)
                        {
                            if (responseJSONPlayedShows[i].show.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONPlayedShows[i].show.title.ToLower().ToString()))
                            {
                                if (countPlayedShows == 0)
                                {
                                    output += $"{Environment.NewLine}{Environment.NewLine} Anticipated Shows:";
                                    countPlayedShows++;
                                }
                                count++;
                                output += $"{Environment.NewLine}{Environment.NewLine} > " + responseJSONPlayedShows[i].show.title + $"{Environment.NewLine}{Environment.NewLine} > year: " + responseJSONPlayedShows[i].show.year;
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


                    //SHOWS////SHOWS//SHOWS////SHOWS//SHOWS////SHOWS//SHOWS////SHOWS//SHOWS////SHOWS//SHOWS////SHOWS//SHOWS////SHOWS//SHOWS////SHOWS
                    var responseTrendinShows = await client.GetAsync("shows/trending");
                    var responseStringTrendinShows = await responseTrendinShows.Content.ReadAsStringAsync();
                    var responseJSONTrendinShows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TrendingShows>>(responseStringTrendinShows);
                    int countTrendingShows = 0;
                    for (int i = 0; i < responseJSONTrendinShows.Count; i++)
                    {
                        if (responseJSONTrendinShows[i].show.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONTrendinShows[i].show.title.ToLower().ToString()))
                        {
                            if (countTrendingShows == 0)
                            {
                                output += $"{Environment.NewLine}{Environment.NewLine} Trending Shows:";
                                countTrendingShows++;
                            }
                            count++;
                            output += $"{Environment.NewLine}{Environment.NewLine}  " + responseJSONTrendinShows[i].show.title + $"{Environment.NewLine}{Environment.NewLine} > year: " + responseJSONTrendinShows[i].show.year;
                        }
                    }

                    var responsePopularShows = await client.GetAsync("shows/popular");
                    var responseStringPopularShows = await responsePopularShows.Content.ReadAsStringAsync();
                    var responseJSONPopularShows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Show>>(responseStringPopularShows);
                    int countPopularShows = 0;
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        if (responseJSONPopularShows[i].title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONPopularShows[i].title.ToLower().ToString()))
                        {
                            if (countPopularShows == 0)
                            {
                                output += $"{Environment.NewLine}{Environment.NewLine} Popular Shows:";
                                countPopularShows++;
                            }
                            count++;
                            output += $"{Environment.NewLine}{Environment.NewLine}  " + responseJSONPopularShows[i].title + $"{Environment.NewLine}{Environment.NewLine} > year: " + responseJSONPopularShows[i].year;
                        }
                    }

                    var responseAnticipatedShows = await client.GetAsync("shows/anticipated");
                    var responseStringAnticipatedShows = await responseAnticipatedShows.Content.ReadAsStringAsync();
                    var responseJSONAnticipatedShows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnticipatedShows>>(responseStringAnticipatedShows);
                    int countAnticipatedShows = 0;

                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        if (responseJSONAnticipatedShows[i].show.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONAnticipatedShows[i].show.title.ToLower().ToString()))
                        {
                            if (countAnticipatedShows == 0)
                            {
                                output += $"{Environment.NewLine}{Environment.NewLine} Anticipated Shows:";
                                countAnticipatedShows++;
                            }
                            count++;
                            output += $"{Environment.NewLine}{Environment.NewLine}  " + responseJSONAnticipatedShows[i].show.title + $"{Environment.NewLine}{Environment.NewLine} > year: " + responseJSONAnticipatedShows[i].show.year;
                        }
                    }

                    var responsePlayedShows = await client.GetAsync("shows/played");
                    var responseStringPlayedShows = await responsePlayedShows.Content.ReadAsStringAsync();
                    var responseJSONPlayedShows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PlayedShows>>(responseStringPlayedShows);
                    int countPlayedShows = 0;

                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        if (responseJSONPlayedShows[i].show.title.ToLower().ToString().Contains(result.Query.ToLower().ToString()) || result.Query.ToLower().ToString().Contains(responseJSONPlayedShows[i].show.title.ToLower().ToString()))
                        {
                            if (countPlayedShows == 0)
                            {
                                output += $"{Environment.NewLine}{Environment.NewLine} Anticipated Shows:";
                                countPlayedShows++;
                            }
                            count++;
                            output += $"{Environment.NewLine}{Environment.NewLine}  " + responseJSONPlayedShows[i].show.title + $"{Environment.NewLine}{Environment.NewLine} > year: " + responseJSONPlayedShows[i].show.year;
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
            Boolean yes = false;
            String output = "Trending Shows";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                var response = await client.GetAsync("shows/trending");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TrendingShows>>(responseString);
                if ((result.Query.ToLower().ToString().Contains("search") || result.Query.ToLower().ToString().Contains("find")) && (!(result.Query.ToLower().ToString().Contains("list trending") || result.Query.ToLower().ToString().Contains("find trending") || result.Query.ToLower().ToString().Contains("find shows that are trending") || result.Query.ToLower().ToString().Contains("list shows that are trending") || result.Query.ToLower().ToString().Contains("list of shows that are trending") || result.Query.ToLower().ToString().Contains("get trending") || result.Query.ToLower().ToString().Contains("get shows that are trending"))))
                {
                    await None(context, result);
                }
                else
                {
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        count++;
                        output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].show.title + $"{Environment.NewLine} > year:" + responseJSON[i].show.year + $"{Environment.NewLine} > watchers:" + responseJSON[i].watchers;
                        yes = true;
                    }
                    if (yes)
                    {
                        string message = $"" + output;
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        category = "trending shows";
                    }
                    else
                    {
                        string message = $"No results found";
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        category = "";
                    }
                }

            }

        }

        [LuisIntent("intent.tvMaster.shows.popular")]
        public async Task PopularShow(IDialogContext context, LuisResult result)
        {
            int count = 0;
            Boolean yes = false;
            String output = "Popular Shows";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                var response = await client.GetAsync("shows/popular");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Show>>(responseString);
                if ((result.Query.ToLower().ToString().Contains("search") || result.Query.ToLower().ToString().Contains("find")) && (!(result.Query.ToLower().ToString().Contains("list popular") || result.Query.ToLower().ToString().Contains("find popular") || result.Query.ToLower().ToString().Contains("find shows that are popular") || result.Query.ToLower().ToString().Contains("list shows that are popular") || result.Query.ToLower().ToString().Contains("list of shows that are popular") || result.Query.ToLower().ToString().Contains("get popular") || result.Query.ToLower().ToString().Contains("get shows that are popular"))))
                {
                    await None(context, result);
                }
                else
                {
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        count++;
                        output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].title + $"{Environment.NewLine} > year: " + responseJSON[i].year;
                        yes = true;
                    }
                }
                if (yes)
                {
                    string message = $"" + output;
                    await context.PostAsync(message);
                    context.Wait(MessageReceived);
                    category = "popular shows";
                }
                else
                {
                    string message = "No retults";
                    await context.PostAsync(message);
                    context.Wait(MessageReceived);
                    category = "";
                }
            }
        }

        [LuisIntent("intent.tvMaster.shows.mostplayed")]
        public async Task MostPlayedShow(IDialogContext context, LuisResult result)
        {
            int count = 0;
            Boolean yes = false;
            String output = "Most played shows";
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                var response = await client.GetAsync("shows/played");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PlayedShows>>(responseString);
                if ((result.Query.ToLower().ToString().Contains("search") || result.Query.ToLower().ToString().Contains("find")) && (!(result.Query.ToLower().ToString().Contains("list most played") || result.Query.ToLower().ToString().Contains("find most played") || result.Query.ToLower().ToString().Contains("find shows that are played most") || result.Query.ToLower().ToString().Contains("list shows that are most played") || result.Query.ToLower().ToString().Contains("list of shows that are most played") || result.Query.ToLower().ToString().Contains("list of shows that are played most") || result.Query.ToLower().ToString().Contains("get most played") || result.Query.ToLower().ToString().Contains("get shows that are played most"))))
                {
                    await None(context, result);
                }
                else
                {
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        count++;
                        output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].show.title + $"{Environment.NewLine} > year: " + responseJSON[i].show.year + $"{Environment.NewLine} > Play count: " + responseJSON[i].play_count;
                        yes = true;
                    }
                }
                if (yes)
                {
                    string message = $"" + output;
                    await context.PostAsync(message);
                    context.Wait(MessageReceived);
                    category = "most played shows";
                }
                else
                {
                    string message = $"No results";
                    await context.PostAsync(message);
                    context.Wait(MessageReceived);
                    category = "";
                }
            }

        }
        [LuisIntent("intent.tvMaster.shows.anticipated")]
        public async Task AnticipatedShow(IDialogContext context, LuisResult result)
        {
            int count = 0;
            String output = "Anticipated Shows";
            Boolean yes = false;
            using (var client = new HttpClient { BaseAddress = new Uri("https://api.trakt.tv") })
            {
                client.DefaultRequestHeaders.Add("trakt-api-key", "468a92c26d3411be7886881b7f40afea47288963a91d9c5a0f43257521ceab74");

                var response = await client.GetAsync("shows/anticipated");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnticipatedShows>>(responseString);
                if ((result.Query.ToLower().ToString().Contains("search") || result.Query.ToLower().ToString().Contains("find")) && (!(result.Query.ToLower().ToString().Contains("list anticipated") || result.Query.ToLower().ToString().Contains("find anticipated") || result.Query.ToLower().ToString().Contains("find shows that are anticipated") || result.Query.ToLower().ToString().Contains("list shows that are most anticipated") || result.Query.ToLower().ToString().Contains("list of shows that are anticipated most") || result.Query.ToLower().ToString().Contains("get anticipated") || result.Query.ToLower().ToString().Contains("get shows that are most anticipated"))))
                {
                    await None(context, result);
                }
                else
                {
                    for (int i = 0; i < responseJSON.Count; i++)
                    {
                        count++;
                        output += $"{Environment.NewLine}{Environment.NewLine} > " + count + ")" + responseJSON[i].show.title + $"{Environment.NewLine} > Year: " + responseJSON[i].show.year + $"{Environment.NewLine} > List count: " + responseJSON[i].list_count;
                        yes = true;
                    }
                    if (yes)
                    {
                        string message = $"" + output;
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        category = "anticipated shows";
                    }
                    else
                    {
                        string message = $"No Results";
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        category = "";
                    }
                }


            }

        }
        [LuisIntent("intent.tvMaster.greetings")]
        public async Task Greetings(IDialogContext context, LuisResult result)
        {
            String[] greet = { "Hey! Its good to have you on board. ",
                "Hey! Welcome to my world. ","The master welcome you with warm hands.",
                "Hey! Its good to have you on board my friend.",
                "Hey! ","Hey! I have been noticing you noticing me noticing you...>"};
            Random rand = new Random();
            int chooseGreetingMessage = rand.Next(0, 5);
            String greetingMessage = greet[chooseGreetingMessage];
            String menu = $"{Environment.NewLine}{Environment.NewLine}This is the menu I have for you.{Environment.NewLine}{Environment.NewLine} > 1) Anticipated Movies {Environment.NewLine}{Environment.NewLine} > 2) Most Played Movies {Environment.NewLine}{Environment.NewLine} > 3) Popular Movies {Environment.NewLine}{Environment.NewLine} > 4) Trending Movies {Environment.NewLine}{Environment.NewLine} > 5) Anticipated Shows {Environment.NewLine}{Environment.NewLine} > 6) Most played Shows {Environment.NewLine}{Environment.NewLine} >7) Popular Shows {Environment.NewLine}{Environment.NewLine} >8) Trending Shows";
            String options = $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine} You can also just search a movie or a show by it litle. The results will also show you all the categories that a movie was found no. You can also use the following options {Environment.NewLine}{Environment.NewLine} > 9) about me {Environment.NewLine}{Environment.NewLine} > 10) reset";
            string message = $"" + greetingMessage + menu + options;
            await context.PostAsync(message);
            context.Wait(MessageReceived);
            category = "";
        }
    }
}
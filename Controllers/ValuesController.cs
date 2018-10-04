using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MovieApi.Controllers
{
    public class MyMovieList
    {
        public List<MyMovieData> Movies { get; set; }
    }

    public class MyMovieData
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string ID { get; set; }
        public string Type { get; set; }
        public string Poster { get; set; }
        public string Price { get; set; }
        public decimal PriceDecimal { get; set; }
        public string DataSource { get; set; }
        public int BestValue { get; set; }
    }
    public class MyMovieDetailData
    {
        public string Price { get; set; }
    }
    public class APISourceData
    {
        public string SourceCompany { get; set; }
    }
    // static class to hold global variables, etc.
    // global function


    static class MovieConfigList
    {
        // global  
        public static string host = "http://webjetapitest.azurewebsites.net";
        public static string filmWorld = "filmWorld";
        public static string cinemaWorld = "cinemaWorld";

    }


    class Program
    {

        public static List<APISourceData> GetSourceCompany()
        {
            List<APISourceData> APISourceDataList = new List<APISourceData>();
            APISourceData APILink = new APISourceData();
            APILink.SourceCompany = "filmWorld";
            APISourceDataList.Add(APILink);
            APILink = new APISourceData();
            APILink.SourceCompany = "cinemaWorld";
            APISourceDataList.Add(APILink);
            return APISourceDataList;
        }
        public static string GetUrlString(string souceCompany, bool ifDetail)
        {

            string APIUrl = null;
            if (ifDetail) //true is detail API Url
                APIUrl = MovieConfigList.host + "/api/" + souceCompany + "/movie/";
            else
                APIUrl = MovieConfigList.host + "/api/" + souceCompany + "/movies";
            return APIUrl;
        }

        //public static async Task<MyMovieList> GetDataFromAPI(string souceCompany)
        public static async Task<List<MyMovieData>> GetDataFromAPI(string souceCompany)
        {

            string apiURL = GetUrlString(souceCompany, false);

            MyMovieList movieWorldData = await GetRequest(apiURL, souceCompany);
            MyMovieData moviveDetailData = new MyMovieData();
            MyMovieData detailData = new MyMovieData();
            List<MyMovieData> movieFinalList = new List<MyMovieData>();

            if (movieWorldData != null)
            {
                List<MyMovieData> movieList = movieWorldData.Movies;
 
                Parallel.ForEach(movieList, (item, loopState) =>
        
                {
                    moviveDetailData = item;
                     
                    //Console.WriteLine("after to get movie detail");
                     detailData = Program.getDetailDataFromAPI(item, souceCompany).GetAwaiter().GetResult();
                     movieFinalList.Add(detailData);
 
                });
                
            }
         
         
            return movieFinalList;
        }
        public static List<MyMovieData> SortListFunc(List<MyMovieData> movieList)
        {

            List<MyMovieData> sortList = new List<MyMovieData>();
            List<MyMovieData> compareList = movieList;
            //List<MyMovieData> movieNameList = movieList;
            List<IGrouping<string, MyMovieData>> movieNameList = movieList.GroupBy(u => u.Title).ToList();
            var lowestValue = from r in movieList
                              group r by new { r.Title } into rGroup
                              select rGroup.OrderBy(rg => rg.PriceDecimal).FirstOrDefault();
            //sortList = movieList.OrderBy(c => c.Title).ThenBy(a => a.Year).ThenBy(b => b.PriceDecimal).ToList();
            foreach (var item in lowestValue)
            {
                compareList.Where(w => w.ID == item.ID && w.PriceDecimal > 0).ToList().ForEach(s => s.BestValue = 1);
            }
            List<MyMovieData> objListOrder = compareList.OrderByDescending(a => a.Year).ThenBy(a => a.Title).ThenBy(b => b.PriceDecimal).ToList();
            return objListOrder;
        }



        public static async Task<MyMovieData> getDetailDataFromAPI(MyMovieData item, string souceCompany)
        {
            string apiDetailURL = GetUrlString(souceCompany, true);
            try
            {
                var moviceIDURL = apiDetailURL + item.ID;
                HttpResponseMessage responseData = new HttpResponseMessage();
                int numberOfRetry = 0;
                MyMovieData movieFinalData = new MyMovieData();
                using (HttpClient client = new HttpClient())
                {
                    do
                    {
                        using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, moviceIDURL))
                        {
                            requestMessage.Headers.Add("x-access-token", "sjd1HfkjU83ksdsm3802k");
                            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                            responseData = await client.SendAsync(requestMessage);
                            numberOfRetry++;

                            if (responseData.IsSuccessStatusCode)
                            {
                                using (HttpContent content = responseData.Content)
                                {
                                    MyMovieDetailData myMovieDetailData = new MyMovieDetailData();
                                    myMovieDetailData = await content.ReadAsAsync<MyMovieDetailData>();
                                
                                    movieFinalData = item;
                                    movieFinalData.Price = myMovieDetailData.Price;
                                    movieFinalData.DataSource = souceCompany;
                                    if (movieFinalData.Price != "")
                                        movieFinalData.PriceDecimal = Convert.ToDecimal(movieFinalData.Price);
                                    return movieFinalData;

                                }
                            }

                        }
                    } while (responseData.IsSuccessStatusCode == false & numberOfRetry < 5);



                }
                return null;
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
                return null;
            }


        }
        
        static async Task<MyMovieList> GetRequest(string url, string dataSource)
        {
            int numberOfRetry = 0;
            HttpResponseMessage responseData = new HttpResponseMessage();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    do
                    {
                        using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
                        {
                            requestMessage.Headers.Add("x-access-token", "sjd1HfkjU83ksdsm3802k");
                            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = await client.SendAsync(requestMessage);
                            numberOfRetry++;

                            if (responseData.IsSuccessStatusCode)
                            {
                                using (HttpContent content = response.Content)
                                {
                                    MyMovieList myMovieData = new MyMovieList();
                                    myMovieData = await content.ReadAsAsync<MyMovieList>();
                                    //myMovieData.DataSource = dataSource;
                                    //Console.WriteLine(myMovieData.Movies.Count);

                                    return myMovieData;

                                }
                            }
                            
                        }
                    } while (responseData.IsSuccessStatusCode == false & numberOfRetry < 5);
                }
                return null;
            }
     

            catch (Exception e)
            {

                Console.WriteLine(e);
                return null;
            }
        }
        //get movie data for each ID
        static async Task<String> GetIDRequest(string url, string movieID)
        {
            var moviceIDURL = url + movieID;

            using (HttpClient client = new HttpClient())
            {
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, moviceIDURL))
                {
                    requestMessage.Headers.Add("x-access-token", "sjd1HfkjU83ksdsm3802k");
                    requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.SendAsync(requestMessage);

                    using (HttpContent content = response.Content)
                    {
                        MyMovieDetailData myMovieDetailData = new MyMovieDetailData();
                        myMovieDetailData = await content.ReadAsAsync<MyMovieDetailData>();
                        //Console.WriteLine(myMovieDetailData.Price);
                        return myMovieDetailData.Price;

                    }

                }
            }
        }
    }//end of program


    [Route("api/[controller]")]
    
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]

        public IActionResult  Get()
        {
            //read data from API
            List<APISourceData> allSource = Program.GetSourceCompany();
            List<MyMovieData> movieDetailData = new List<MyMovieData>();
            List<MyMovieData> movieFinalList = new List<MyMovieData>();
            List<MyMovieData> objListOrder = new List<MyMovieData>();

            foreach (var movieSource in allSource)
            {
            
                    //MyMovieList movieFilmData = Program.GetDataFromAPI("filmworld").GetAwaiter().GetResult();
                    //MyMovieList movieCinemaData = Program.GetDataFromAPI("cinemaworld").GetAwaiter().GetResult();
                    List<MyMovieData> movieFilmData = Program.GetDataFromAPI(movieSource.SourceCompany).GetAwaiter().GetResult();
                    if (movieFilmData != null)
                        movieFinalList.AddRange(movieFilmData);
            

             }



            if (movieFinalList != null)
            {
                objListOrder = Program.SortListFunc(movieFinalList);             
            }
             
            return new ObjectResult(objListOrder);

        }
    
    }
    }

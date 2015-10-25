using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft;
using HtmlAgilityPack;
using System.Threading.Tasks;
namespace iDunno.Models
{

    public class TargetItem
    {
        public string Url { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public TargetItem()
        {

        }
        public string ImgUrl { get; set; }
        //unstructured_data.images
        public TargetItem(dynamic trio)
        {
            Url = trio.data_page_link;
            Description = trio.alternate_description[0].value;
            Id = trio.identifier[1].id;
            try {
                ImgUrl = trio.image.internal_primary_image_url[0];
            }catch(Exception er)
            {

            }
        }
    }
    public class TargetAPI
    {

        dynamic QueryObject(string itemID)
        {
            WebClient mclient = new WebClient();
            
            string txt = mclient.DownloadString("https://api.target.com/items/v3/" + itemID + "/?id_type=tcin&key=Id8SS1KAXuFd2W7R60XC5AUTTGKbnU2U&fields=descriptions,locations,images,environmental");
            return Newtonsoft.Json.JsonConvert.DeserializeObject(txt);

        }
        public dynamic GetObjectData(string itemID)
        {
            dynamic objdata = QueryObject(itemID).product_composite_response.items;
            objdata = objdata[0];
            return objdata;
        }
        public IEnumerable<string> Search(string query)
        {
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            HtmlDocument doc = web.Load("http://www.target.com/s?searchTerm="+Uri.EscapeDataString(query));
            foreach (var et in doc.DocumentNode.Descendants().Where(m => m.Attributes.Contains("class")).Where(m => m.Attributes["class"].Value.Contains("productClick")))
            {
                string mid = et.Attributes["href"].Value;
                mid = mid.Substring(mid.IndexOf("A-") + 2);
                mid = mid.Substring(0, mid.IndexOf("#"));
                yield return mid;
            }

        }
        Task<dynamic> FetchAsync(string obj)
        {
            return Task<dynamic>.Run(() => {
                return GetObjectData(obj);
            });
        }
        public IEnumerable<dynamic> FastSearch(string query)
        {
            var results = Search(query);
            List<Task<dynamic>> pendingRequests = new List<Task<dynamic>>();

            foreach (var et in results.Take(5))
            {
                pendingRequests.Add(FetchAsync(et));
            }
            
            while(pendingRequests.Any())
            {
                var task = Task.WaitAny(pendingRequests.ToArray());
                var result = pendingRequests[task].Result;
                pendingRequests.RemoveAt(task);
                yield return result;
            }
        }
        public TargetAPI()
        {

        }
    }
}
using AdvertisementService.Models.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using static AdvertisementService.Models.Response;

namespace AdvertisementService.Extensions
{
    public static class APIExtensions
    {
        public static IRestResponse GetAPI(string url, string query = "")
        {
            RestClient client = new RestClient(url + query);
            RestRequest request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            return response.IsSuccessful ? response : throw new HttpListenerException((int)response.StatusCode, response.Content);
        }

        public static IRestResponse DeleteApi(string url)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.DELETE);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.ErrorMessage);
            return response;
        }

        public static dynamic GetInstitutionsIncludedData(List<GetAdvertisementsDto> advertisementsModel, string url)
        {
            List<GetInstitutionsDto> institutions = new List<GetInstitutionsDto>();
            List<int> id = new List<int>(advertisementsModel.Select(x => Obfuscation.Decode(x.InstitutionId)).ToList().Distinct());

            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(id), ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = response.Content;
                var institutionsData = JsonConvert.DeserializeObject<GetResponse<GetInstitutionsDto>>(result);
                institutions.AddRange(institutionsData.Data);
            }
            else
            {
                throw new Exception(response.ErrorException.ToString());
            }

            return JArray.Parse(JsonConvert.SerializeObject(institutions.GroupBy(x => x.InstitutionId).Select(x => x.First()).ToList()));
        }

        public static dynamic GetPromotionsAdvertisement(List<GetAdvertisementsDto> advertisementsModel, string url)
        {
            List<GetPromotionDto> promotions = new List<GetPromotionDto>();
            List<int> id = new List<int>(advertisementsModel.Select(x => Obfuscation.Decode(x.AdvertisementId)).ToList().Distinct());

            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(id), ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = response.Content;
                var promotionsData = JsonConvert.DeserializeObject<GetResponse<GetPromotionDto>>(result);
                promotions.AddRange(promotionsData.Data);
            }
            else
            {
                throw new Exception(response.ErrorException.ToString());
            }

            return JArray.Parse(JsonConvert.SerializeObject(promotions.GroupBy(x => x.PromotionId).Select(x => x.First()).ToList()));
        }
        //public static List<PromotionsGetModel> GetPromotionsContents(List<ContentReadDto> contents, string url)
        //{
        //    List<PromotionsGetModel> promotions = new List<PromotionsGetModel>();
        //    List<int> id = new List<int>(contents.Select(x => Obfuscation.Decode(x.ContentId)).ToList().Distinct());

        //    var client = new RestClient(url);
        //    client.Timeout = -1;
        //    var request = new RestRequest(Method.POST);
        //    request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(id), ParameterType.RequestBody);
        //    IRestResponse response = client.Execute(request);
        //    if (response.StatusCode == HttpStatusCode.OK)
        //    {
        //        var result = response.Content;
        //        var promotionsData = JsonConvert.DeserializeObject<GetResponse<PromotionsGetModel>>(result);
        //        promotions.AddRange(promotionsData.data);
        //    }
        //    else
        //    {
        //        throw new Exception(response.ErrorException.ToString());
        //    }
        //    return promotions;
        //}
        //public static PromotionsGetModel GetPromotionsContentById(ContentReadDto content, string url)
        //{
        //    PromotionsGetModel promotion = new PromotionsGetModel();
        //    List<int> id = new List<int>();
        //    id.Add(Obfuscation.Decode(content.ContentId));
        //    var client = new RestClient(url);
        //    var request = new RestRequest(Method.POST);
        //    request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(id), ParameterType.RequestBody);
        //    IRestResponse response = client.Execute(request);
        //    if (response.StatusCode == HttpStatusCode.OK)
        //    {
        //        var result = response.Content;
        //        var promotionData = JsonConvert.DeserializeObject<GetResponseApi<PromotionsGetModel>>(result);
        //        promotion = promotionData.data[0];
        //    }
        //    return promotion;
        //}
    }
}

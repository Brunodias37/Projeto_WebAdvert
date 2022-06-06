using AdvertApi.Models;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebAdvert.Web.ServiceClients
{
    public class AdvertApiClient : IAdvertApiClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly IMapper  _mapper;

        public AdvertApiClient(IConfiguration configuration, HttpClient client, IMapper mapper)
        {
            _configuration = configuration;
            _client = client;
            _mapper = mapper;

            var createUrl = _configuration.GetSection(key: "AdvertApi").GetValue<string>(key: "CreateUrl");
            _client.BaseAddress = new Uri(createUrl);
            _client.DefaultRequestHeaders.Add(name: "Content-Type", value: "application/json");
        }

        public async Task<bool> Confirm(ConfirmAdvertRequest model)
        {
            var advertmodel = _mapper.Map<ConfirmAdvertModel>(model);
            var jsonModel = JsonConvert.SerializeObject(advertmodel);
            var response = await _client.PutAsync(new Uri(uriString:$"{_client.BaseAddress}/confirm"),new StringContent(jsonModel))
                .ConfigureAwait(continueOnCapturedContext:false);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<AdvertResponse> Create(AdvertModel model)
        {
            var advertApiModel = _mapper.Map<AdvertModel>(model);
            
            var jsonModel = JsonConvert.SerializeObject(advertApiModel);
            var response = await _client.PostAsync(new Uri(uriString: $"{_client.BaseAddress}/create"), new StringContent(jsonModel)).ConfigureAwait(continueOnCapturedContext: false);
            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext:false);
            var createAdvertResponse = JsonConvert.DeserializeObject<CreateAdvertResponse>(responseJson);
            var advertResponse = _mapper.Map<AdvertResponse>(createAdvertResponse);

            return advertResponse;
        }
    }
}

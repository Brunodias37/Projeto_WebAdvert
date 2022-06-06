using AdvertApi.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.ServiceClients;
using WebAdvert.Web.Services;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagementController:Controller
    {
        private readonly IFileUploader _fileUploader;
        private readonly IAdvertApiClient _advertApiClient;
        private readonly IMapper _mapper;

        public AdvertManagementController(IFileUploader fileUploader, IAdvertApiClient advertClientApi, IMapper mapper)
        {
            _fileUploader = fileUploader;
            _advertApiClient = advertClientApi;
            _mapper = mapper;
        }

      
        [HttpPost]
        public async Task<IActionResult> Create(CreateAdvertViewModel model, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                
                var createAdvertModel = _mapper.Map<CreateAdvertModel>(model);
                var apiCallResponse = await _advertApiClient.Create(createAdvertModel).ConfigureAwait(false);
                var id = apiCallResponse.Id;

                var fileName = "";
                if(imageFile != null)
                {
                    fileName = !string.IsNullOrEmpty(imageFile.FileName) ? Path.GetFileName(imageFile.FileName) : imageFile.FileName;
                    var filePath = $"{id}/{fileName}";

                    try
                    {
                        using(var readStream = imageFile.OpenReadStream())
                        {
                            var result = await _fileUploader.UploadFileAsync(filePath, readStream)
                                .ConfigureAwait(continueOnCapturedContext: false);
                            if (!result)
                                throw new Exception(
                                    message:"Could no upload the image to file repository. Please see the logs for details.");
                        }

                        //chamada a API para confirmar o advertisement
                        var confirmModel = new ConfirmAdvertRequest()
                        {
                            Id = id,
                            FilePath = filePath,
                            Status = AdvertStatus.Active
                        };
                        
                        var canConfirm = await _advertApiClient.Confirm(confirmModel);
                        if (!canConfirm)
                        {
                            throw new Exception(message:$"Can not confirm advert of id = {id}");
                        }
                                              
                        return RedirectToAction("Index", controllerName: "Home");
                    }
                    catch (Exception e)
                    {
                        var confirmModel = new ConfirmAdvertRequest()
                        {
                            Id = id,
                            FilePath = filePath,
                            Status = AdvertStatus.Pending
                        };
                                                
                        await _advertApiClient.Confirm(confirmModel).ConfigureAwait(continueOnCapturedContext:false);
                        Console.Write(e);
                    }
                }

            }

            return View();
        }


        

    }
}

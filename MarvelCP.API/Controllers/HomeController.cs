using MarvelCP.API.Data.Entities;
using MarvelCP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MarvelCP.API.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index([FromServices] IConfiguration config)
        {
            Character personagem;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                string ts = "1"; // DateTime.Now.Ticks.ToString();
                string publicKey = config.GetSection("MarvelComicsAPI:PublicKey").Value;
                string hash = GerarHash(ts, publicKey,
                    config.GetSection("MarvelComicsAPI:PrivateKey").Value);

                HttpResponseMessage response = client.GetAsync(
                    config.GetSection("MarvelComicsAPI:BaseURL").Value +
                    $"characters?ts={ts}&apikey={publicKey}&hash={hash}&" +
                    $"name={Uri.EscapeUriString("Captain America")}").Result;

                response.EnsureSuccessStatusCode();
                string conteudo =
                    response.Content.ReadAsStringAsync().Result;

                dynamic resultado = JsonConvert.DeserializeObject(conteudo);

                personagem = new Character();
                personagem.Name = resultado.data.results[0].name;
                personagem.Description = resultado.data.results[0].description;
                personagem.Image = resultado.data.results[0].thumbnail.path + "." +
                    resultado.data.results[0].thumbnail.extension;
                

            }

            return View(personagem);
            //return View();
        }

        private string GerarHash(
            string ts, string publicKey, string privateKey)
        {
            byte[] bytes =
                Encoding.UTF8.GetBytes(ts + privateKey + publicKey);
            var gerador = MD5.Create();
            byte[] bytesHash = gerador.ComputeHash(bytes);
            return BitConverter.ToString(bytesHash)
                .ToLower().Replace("-", String.Empty);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

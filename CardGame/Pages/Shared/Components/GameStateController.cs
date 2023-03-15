using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Web;

namespace CardGame.Pages.Shared.Components
{
    public class GameStateController
    {
        public GameState? game_state;
        public string message = "testas2";
        private static HttpClient HTTPClient = new HttpClient();
        public GameStateController()
        {
            //this.OnGetProcessRequestAsync();
            this.OnGetProcessRequest();
        }
        public async void OnGetProcessRequestAsync()
        {
            string request_url = "http://localhost:3000/game_state";


            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();

            try
            {
                HttpResponseMessage responseMessage = await HTTPClient.GetAsync(request_url);
                message = await responseMessage.Content.ReadAsStringAsync();
                game_state = JsonSerializer.Deserialize<List<GameState>>(message).First();
            }
            catch (HttpRequestException exception)
            {
                message = exception.Message;
            }
        }
        public void OnGetProcessRequest()
        {
            string request_url = "http://localhost:3000/game_state";


            var webRequest = new HttpRequestMessage(HttpMethod.Get, request_url);

            var response = HTTPClient.Send(webRequest);

            using var reader = new StreamReader(response.Content.ReadAsStream());
            message = reader.ReadToEnd();
            game_state = JsonSerializer.Deserialize<List<GameState>>(message).First();
            
        }
    }
}

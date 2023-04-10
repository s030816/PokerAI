using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
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
            this.init_process();
            //this.OnGetProcessRequest();
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
            string request_url = "http://localhost:3000/game_state/" + game_state._id;


            var webRequest = new HttpRequestMessage(HttpMethod.Get, request_url);

            var response = HTTPClient.Send(webRequest);

            using var reader = new StreamReader(response.Content.ReadAsStream());
            message = reader.ReadToEnd();
            game_state = JsonSerializer.Deserialize<GameState>(message);
            
        }
        public void init_process()
        {
            string request_url = "http://localhost:3000/initgame";


            var webRequest = new HttpRequestMessage(HttpMethod.Get, request_url);

            var response = HTTPClient.Send(webRequest);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            message = reader.ReadToEnd();
            game_state = JsonSerializer.Deserialize<GameState>(message);
        }

        public string get_error(int iter, int sample_s, int ns1, int ns2, 
            int iter1, int sample_s1, int ns11, int ns12)
        {
            string request_url = "http://localhost:3000/train";
            var url = 
          $"{request_url}?iter={iter}&sample_s={sample_s}&ns1={ns1}&ns2={ns2}&iter1={iter1}&sample_s1={sample_s1}&ns11={ns11}&ns12={ns12}";

            var webRequest = new HttpRequestMessage(HttpMethod.Get, url);

            var response = HTTPClient.Send(webRequest);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            message = reader.ReadToEnd();
            return message;
        }

        public string OnPutProcess()
        {
            string request_url = "http://localhost:3000/advance/" + game_state._id;
            /*
            var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("id", "6410dc0ebb023e49d13004c4")
            });
            */
            var webRequest = new HttpRequestMessage(HttpMethod.Put, request_url);

            //webRequest.Content = content;

            var response = HTTPClient.Send(webRequest);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            message = reader.ReadToEnd();
            return message;
        }
    }
}

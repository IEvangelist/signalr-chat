using System.Threading.Tasks;

namespace BlazoR.Chat.Client.Pages
{
    public partial class Index
    {
        public bool IsJoined { get; set; }
        public string Username { get; set; }

        protected override async Task OnInitializedAsync()
        {

        }

        public void Join() => IsJoined = Username is not { Length: >= 3 };
    }
}

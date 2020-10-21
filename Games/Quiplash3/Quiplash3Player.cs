using Quipbot.Browsers.WebView2;

namespace Quipbot.Games.Quiplash3
{
    public class Quiplash3Player : PlayerBase<Quiplash3Observer, Quiplash3Behavior, WebView2Container>
    {
        public Quiplash3Player() : base() { }

        public Quiplash3Player(string name) : base(name) { }
    }
}

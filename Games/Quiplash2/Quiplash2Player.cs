using Quipbot.Browsers.WebView2;

namespace Quipbot.Games.Quiplash2
{
    public class Quiplash2Player : PlayerBase<Quiplash2Observer, Quiplash2Behavior, WebView2Container>
    {
        public Quiplash2Player() : base() { }

        public Quiplash2Player(string name) : base(name) { }
    }
}

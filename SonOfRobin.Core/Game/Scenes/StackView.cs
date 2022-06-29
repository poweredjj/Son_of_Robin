using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin

{
    public class StackView : Scene
    {
        private static readonly SpriteFont font = SonOfRobinGame.fontSmall;
        private static readonly int margin = 3;

        private static List<Scene> DisplayedStack
        { get { return sceneStack.OrderByDescending(o => o.priority).ToList(); } }

        private static Dictionary<Scene, string> TextByScene
        {
            get
            {
                var textByScene = new Dictionary<Scene, string> { };

                var drawStack = DrawStack;
                var updateStack = UpdateStack;

                foreach (Scene scene in DisplayedStack)
                {
                    bool updates = updateStack.Contains(scene);
                    bool draws = drawStack.Contains(scene);
                    string sceneText = GetSceneText(scene: scene, updates: updates, draws: draws);

                    textByScene[scene] = sceneText;
                }

                return textByScene;
            }
        }

        private static Vector2 TextBlockSize
        {
            get
            {
                int width = 0;
                int height = 0;

                var textByScene = TextByScene;

                foreach (string sceneTxt in textByScene.Values)
                {
                    Vector2 txtSize = font.MeasureString(sceneTxt);
                    if (txtSize.X > width) width = (int)txtSize.X;

                    height += (int)txtSize.Y + margin;
                }

                return new Vector2(width, height);
            }
        }

        public StackView() : base(inputType: InputTypes.None, priority: 0, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.AddTransition(this.GetTransition(inTrans: true));
        }

        public override void Remove()
        {
            if (this.transition == null)
            {
                this.AddTransition(this.GetTransition(inTrans: false));
                return;
            }

            base.Remove();
        }

        public Transition GetTransition(bool inTrans)
        {
            Transition.TransType transType = inTrans ? Transition.TransType.In : Transition.TransType.Out;
            bool removeScene = !inTrans;
            this.UpdateView();

            return new Transition(type: transType, removeScene: removeScene, duration: 12, scene: this, paramsToChange: new Dictionary<string, float> { { "posX", this.viewParams.posX + this.viewParams.width } });
        }

        public override void Update(GameTime gameTime)
        { this.UpdateView(); }

        private void UpdateView()
        {
            Vector2 textBlockSize = TextBlockSize;
            this.viewParams.width = (int)textBlockSize.X;
            this.viewParams.height = (int)textBlockSize.Y;
            this.viewParams.posX = SonOfRobinGame.VirtualWidth - textBlockSize.X - margin;
            this.viewParams.posY = margin;
        }

        public override void Draw()
        {
            var textByScene = TextByScene;

            int sceneNo = 0;
            for (int i = textByScene.ToList().Count - 1; i >= 0; i--)
            {
                Scene scene = textByScene.Keys.ToList()[i];
                string sceneTxt = textByScene.Values.ToList()[i];

                Vector2 txtSize = font.MeasureString(sceneTxt);
                Vector2 currentPos = new Vector2(0, (sceneNo * (txtSize.Y + margin)));

                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    { SonOfRobinGame.spriteBatch.DrawString(font, sceneTxt, currentPos + new Vector2(x, y), Color.Black); }
                }

                Color color = scene.priority == 0 ? Color.White : Color.LightGreen;
                SonOfRobinGame.spriteBatch.DrawString(font, sceneTxt, currentPos, color);
                sceneNo++;
            }
        }

        private static string GetSceneText(Scene scene, bool updates, bool draws)
        {
            string sceneName;

            if (scene.GetType() == typeof(Menu))
            {
                Menu menu = (Menu)scene;
                sceneName = $"Menu '{menu.templateName}'";
            }
            else if (scene.GetType() == typeof(Map))
            {
                Map map = (Map)scene;
                sceneName = map.fullScreen ? "Map (big)" : "Map (small)";
            }
            else if (scene.GetType() == typeof(Inventory))
            {
                Inventory inventory = (Inventory)scene;
                sceneName = Convert.ToString(inventory.storage.storageType);
            }
            else
            { sceneName = scene.GetType().Name; }

            string inputTxt = scene.inputActive ? " I" : "";
            string updatesTxt = updates ? " U" : "";
            string drawsTxt = draws ? " D" : "";
            string blocksUpdatesTxt = scene.blocksUpdatesBelow ? " blU" : "";
            string blocksDrawsTxt = scene.blocksDrawsBelow ? " blD" : "";

            return $"{scene.priority} {sceneName}{inputTxt}{updatesTxt}{drawsTxt}{blocksUpdatesTxt}{blocksDrawsTxt}";
        }

    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin

{
    public class StackView : Scene
    {
        private static readonly SpriteFont font = SonOfRobinGame.FontPressStart2P5;
        private const int margin = 3;

        private static List<Scene> DisplayedStack
        {
            get
            {
                var displayedStack = sceneStack.OrderByDescending(o => o.priority).ToList();
                displayedStack.AddRange(waitingScenes);
                return displayedStack;
            }
        }

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
                    bool waits = waitingScenes.Contains(scene);
                    string sceneText = GetSceneText(scene: scene, updates: updates, draws: draws, waits: waits);

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

        public StackView() : base(inputType: InputTypes.None, priority: -1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.transManager.AddTransition(this.GetTransition(inTrans: true));
        }

        protected override void AdaptToNewSize()
        {
            this.UpdateView();
        }

        public override void Remove()
        {
            if (!this.transManager.IsEnding) this.transManager.AddTransition(this.GetTransition(inTrans: false));
            else base.Remove();
        }


        public Transition GetTransition(bool inTrans)
        {
            this.UpdateView();

            return new Transition(transManager: this.transManager, outTrans: !inTrans, baseParamName: "PosX", targetVal: this.viewParams.PosX + this.viewParams.Width, duration: 12, endRemoveScene: !inTrans);
        }

        public override void Update(GameTime gameTime)
        {
            this.UpdateView();
        }

        public void UpdateView()
        {
            Vector2 textBlockSize = TextBlockSize;
            this.viewParams.Width = (int)textBlockSize.X;
            this.viewParams.Height = (int)textBlockSize.Y;
            this.viewParams.PosX = SonOfRobinGame.VirtualWidth - textBlockSize.X - margin;
            this.viewParams.PosY = margin;
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
                Vector2 txtPos = new Vector2(0, sceneNo * (txtSize.Y + margin));

                Color color = scene.priority == 0 ? Color.White : Color.LightGreen;
                if (waitingScenes.Contains(scene)) color = Color.Cyan;

                Helpers.DrawTextWithOutline(font: font, text: sceneTxt, pos: txtPos, color: color * this.viewParams.drawOpacity, outlineColor: Color.Black * this.viewParams.drawOpacity, outlineSize: 1);

                sceneNo++;
            }
        }

        private static string GetSceneText(Scene scene, bool updates, bool draws, bool waits)
        {
            string sceneName;

            if (scene.GetType() == typeof(Menu))
            {
                Menu menu = (Menu)scene;
                sceneName = $"Menu '{menu.templateName}'";
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
            string waitsTxt = waits ? " W" : "";
            string blocksUpdatesTxt = scene.blocksUpdatesBelow ? " blU" : "";
            string blocksDrawsTxt = scene.blocksDrawsBelow ? " blD" : "";

            return $"{scene.priority} {sceneName}{inputTxt}{updatesTxt}{drawsTxt}{waitsTxt}{blocksUpdatesTxt}{blocksDrawsTxt}";
        }

    }
}

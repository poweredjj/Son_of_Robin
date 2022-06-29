using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class InputMapper
    {
        public enum Action
        {
            GlobalConfirm,
            GlobalCancelReturnSkip,
            GlobalLeft,
            GlobalRight,
            GlobalUp,
            GlobalDown,

            WorldWalk,
            WorldCameraMove,
            WorldCameraZoomOut,
            WorldRun,
            WorldInteract,
            WorldPickUp,
            WorldUseToolbarPiece,
            WorldInventory,
            WorldEquip,
            WorldFieldCraft,
            WorldMapToggle,
            WorldPauseMenu,

            InvSwitch,
            InvPickOne,
            InvPickStack,
            InvRelease,

            ToolbarPrev,
            ToolbarNext,

            MapSwitch,

            SecretLicenceBypass
        }

        public static readonly int defaultRepeatThreshold = 35;
        public static readonly int defaultRepeatFrames = 6;

        public enum AnalogType { Empty, PadLeft, PadRight, VirtLeft, VirtRight, FromKeys }

        protected readonly static Dictionary<Action, Mapping> detailedMappings = new Dictionary<Action, Mapping>();

        public static readonly InputPackage defaultMappingGamepad = new InputPackage(packageVersion: InputPackage.version, leftStick: AnalogType.PadLeft, rightStick: AnalogType.PadRight, confirm: Buttons.A, cancel: Buttons.B, pauseMenu: Buttons.Start, run: Buttons.B, equip: Buttons.DPadLeft, inventory: Buttons.Y, pickUp: Buttons.X, craft: Buttons.DPadUp, interact: Buttons.A, map: Buttons.DPadRight, useTool: Buttons.RightTrigger, zoomOut: Buttons.LeftTrigger, toolbarPrev: Buttons.LeftShoulder, toolbarNext: Buttons.RightShoulder, invSwitch: Buttons.LeftStick, pickOne: Buttons.Y, pickStack: Buttons.X);

        public static readonly InputPackage defaultMappingKeyboard = new InputPackage(packageVersion: InputPackage.version, leftStick: AnalogType.FromKeys, rightStick: AnalogType.Empty, confirm: Keys.Enter, cancel: Keys.Escape, pauseMenu: Keys.Back, run: Keys.NumPad0, equip: Keys.E, inventory: Keys.Enter, pickUp: Keys.RightControl, craft: Keys.NumPad5, interact: Keys.RightShift, map: Keys.M, useTool: Keys.Space, zoomOut: Keys.NumPad1, toolbarPrev: Keys.OemOpenBrackets, toolbarNext: Keys.OemCloseBrackets, invSwitch: Keys.Tab, pickOne: Keys.RightShift, pickStack: Keys.Space, left: Keys.Left, right: Keys.Right, up: Keys.Up, down: Keys.Down);

        public static InputPackage currentMappingGamepad = defaultMappingGamepad.MakeCopy();
        public static InputPackage currentMappingKeyboard = defaultMappingKeyboard.MakeCopy();

        public static InputPackage newMappingGamepad = currentMappingGamepad.MakeCopy();
        public static InputPackage newMappingKeyboard = currentMappingKeyboard.MakeCopy();

        public static void RebuildMappings()
        {
            detailedMappings.Clear();

            InputPackage padMap = currentMappingGamepad;
            InputPackage keybMap = currentMappingKeyboard;
            var keysToAnalog = new List<Keys> { (Keys)keybMap.left, (Keys)keybMap.right, (Keys)keybMap.up, (Keys)keybMap.down };

            // global
            new Mapping(action: Action.GlobalConfirm, anyInputList: new List<object> { keybMap.confirm, padMap.confirm, VButName.Confirm });
            new Mapping(action: Action.GlobalCancelReturnSkip, anyInputList: new List<object> { keybMap.cancel, padMap.cancel, VButName.Return });
            new Mapping(action: Action.GlobalLeft, anyInputList: new List<object> { keybMap.left, Buttons.DPadLeft }, gamepadAnalogAsDigital: true, repeat: true);
            new Mapping(action: Action.GlobalRight, anyInputList: new List<object> { keybMap.right, Buttons.DPadRight }, gamepadAnalogAsDigital: true, repeat: true);
            new Mapping(action: Action.GlobalUp, anyInputList: new List<object> { keybMap.up, Buttons.DPadUp }, gamepadAnalogAsDigital: true, repeat: true);
            new Mapping(action: Action.GlobalDown, anyInputList: new List<object> { keybMap.down, Buttons.DPadDown }, gamepadAnalogAsDigital: true, repeat: true);

            // world
            new Mapping(action: Action.WorldWalk, anyInputList: new List<object> { AnalogType.VirtLeft, keybMap.leftStick, padMap.leftStick }, keysToAnalog: keysToAnalog);
            new Mapping(action: Action.WorldCameraMove, anyInputList: new List<object> { AnalogType.VirtRight, padMap.rightStick });
            new Mapping(action: Action.WorldPauseMenu, anyInputList: new List<object> { keybMap.pauseMenu, padMap.pauseMenu, VButName.PauseMenu });
            new Mapping(action: Action.WorldRun, anyInputList: new List<object> { keybMap.run, padMap.run, VButName.Run });
            new Mapping(action: Action.WorldEquip, anyInputList: new List<object> { keybMap.equip, padMap.equip, VButName.Equip });
            new Mapping(action: Action.WorldInventory, anyInputList: new List<object> { keybMap.inventory, padMap.inventory, VButName.Inventory });
            new Mapping(action: Action.WorldPickUp, anyInputList: new List<object> { keybMap.pickUp, padMap.pickUp, VButName.PickUp });
            new Mapping(action: Action.WorldInteract, anyInputList: new List<object> { keybMap.interact, padMap.interact, VButName.Interact });
            new Mapping(action: Action.WorldMapToggle, anyInputList: new List<object> { keybMap.map, padMap.map, VButName.Map });
            new Mapping(action: Action.WorldFieldCraft, anyInputList: new List<object> { keybMap.craft, padMap.craft, VButName.FieldCraft });
            new Mapping(action: Action.WorldUseToolbarPiece, anyInputList: new List<object> { keybMap.useTool, padMap.useTool, VButName.UseTool });
            new Mapping(action: Action.WorldCameraZoomOut, anyInputList: new List<object> { keybMap.zoomOut, padMap.zoomOut, VButName.ZoomOut });

            // inventory
            new Mapping(action: Action.InvPickStack, anyInputList: new List<object> { keybMap.pickStack, padMap.pickStack });
            new Mapping(action: Action.InvPickOne, anyInputList: new List<object> { keybMap.pickOne, padMap.pickOne });
            new Mapping(action: Action.InvRelease, anyInputList: new List<object> { keybMap.confirm, padMap.confirm, keybMap.pickStack, padMap.pickStack, keybMap.pickOne, padMap.pickOne });
            new Mapping(action: Action.InvSwitch, anyInputList: new List<object> { keybMap.invSwitch, padMap.invSwitch, keybMap.toolbarPrev, keybMap.toolbarNext, padMap.toolbarPrev, padMap.toolbarNext, });
            new Mapping(action: Action.ToolbarPrev, anyInputList: new List<object> { keybMap.toolbarPrev, padMap.toolbarPrev }, repeat: true);
            new Mapping(action: Action.ToolbarNext, anyInputList: new List<object> { keybMap.toolbarNext, padMap.toolbarNext }, repeat: true);

            // map
            new Mapping(action: Action.MapSwitch, anyInputList: new List<object> { keybMap.cancel, keybMap.map, padMap.cancel, padMap.map, VButName.Return });

            // secret licence bypass
            new Mapping(action: Action.SecretLicenceBypass, anyInputList: new List<object> { Keys.LeftControl, Buttons.Start, VButName.Return });
        }

        public static List<Texture2D> GetTextures(Action action)
        {
            if (!detailedMappings.ContainsKey(action))
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"No mapping found for '{action}'.", color: Color.AliceBlue);
                return new List<Texture2D>();
            }

            return detailedMappings[action].TextureList;
        }

        public static Texture2D GetTexture(Action action)
        {
            if (!detailedMappings.ContainsKey(action))
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"No mapping found for '{action}'.", color: Color.AliceBlue);
                return SonOfRobinGame.whiteRectangle;
            }

            if (detailedMappings.Count == 0) return SonOfRobinGame.whiteRectangle;
            var textureList = detailedMappings[action].TextureList;
            if (textureList.Count == 0) return SonOfRobinGame.whiteRectangle;
            return textureList[0];
        }

        public static bool IsPressed(Action action)
        {
            if (!detailedMappings.ContainsKey(action))
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"No mapping found for '{action}'.", color: Color.AliceBlue);
                return false;
            }

            return detailedMappings[action].IsPressed(triggerAsButton: true);
        }

        public static bool HasBeenPressed(Action action)
        {
            if (!detailedMappings.ContainsKey(action))
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"No mapping found for '{action}'.", color: Color.AliceBlue);
                return false;
            }

            return detailedMappings[action].HasBeenPressed(triggerAsButton: true);
        }

        public static bool HasBeenReleased(Action action)
        {
            if (!detailedMappings.ContainsKey(action))
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"No mapping found for '{action}'.", color: Color.AliceBlue);
                return false;
            }

            return detailedMappings[action].HasBeenReleased(triggerAsButton: true);
        }

        public static Vector2 Analog(Action action)
        {
            if (!detailedMappings.ContainsKey(action))
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"No mapping found for '{action}'.", color: Color.AliceBlue);
                return Vector2.Zero;
            }

            return detailedMappings[action].AnalogState;
        }

        public static float TriggerForce(Action action)
        {
            if (!detailedMappings.ContainsKey(action))
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"No mapping found for '{action}'.", color: Color.AliceBlue);
                return 0f;
            }

            return detailedMappings[action].GetTriggerForce(buttonAsTrigger: true);
        }

        public class Mapping
        {
            private readonly List<Keys> keyboardKeys;
            private readonly List<Buttons> gamepadButtons;
            private readonly List<VButName> virtualButtons;
            private readonly List<AnalogType> analogTypes;
            private readonly List<Keys> keysToAnalog;
            private readonly Dictionary<int, float> triggerStateForFrame;
            private readonly bool gamepadAnalogAsDigital;
            private readonly bool repeat;
            private readonly int repeatThreshold;
            private readonly int repeatFrames;
            private int buttonHeldCounter;
            private int buttonHeldCurrentFrame; // to avoid multiple increments in one frame
            public Mapping(Action action, List<Object> anyInputList, List<Keys> keysToAnalog = null, bool gamepadAnalogAsDigital = false, bool repeat = false, int repeatThreshold = 0, int repeatFrames = 0)
            {
                this.keyboardKeys = new List<Keys>();
                this.gamepadButtons = new List<Buttons>();
                this.virtualButtons = new List<VButName>();
                this.analogTypes = new List<AnalogType>();
                this.keysToAnalog = keysToAnalog == null ? new List<Keys>() : keysToAnalog;

                foreach (Object input in anyInputList)
                {
                    if (input == null) continue; // if obsolete mapping was loaded

                    var inputType = input.GetType();

                    if (inputType == typeof(Keys)) this.keyboardKeys.Add((Keys)input);
                    else if (inputType == typeof(Buttons)) this.gamepadButtons.Add((Buttons)input);
                    else if (inputType == typeof(VButName)) this.virtualButtons.Add((VButName)input);
                    else if (inputType == typeof(AnalogType)) this.analogTypes.Add((AnalogType)input);
                    else throw new DivideByZeroException($"Unsupported inputType - {inputType}.");
                }

                this.gamepadAnalogAsDigital = gamepadAnalogAsDigital;
                this.triggerStateForFrame = new Dictionary<int, float>();
                this.buttonHeldCounter = 0;
                this.buttonHeldCurrentFrame = 0;

                this.repeat = repeat;
                if (this.repeat)
                {
                    if (repeatThreshold == 0) repeatThreshold = defaultRepeatThreshold;
                    if (repeatFrames == 0) repeatFrames = defaultRepeatFrames;
                }
                this.repeatThreshold = repeatThreshold;
                this.repeatFrames = repeatFrames;

                detailedMappings.Add(action, this);
            }

            public bool IsPressed(bool triggerAsButton)
            {
                foreach (Buttons button in gamepadButtons)
                {
                    if (GamePad.IsPressed(playerIndex: PlayerIndex.One, button: button, analogAsDigital: this.gamepadAnalogAsDigital)) return true;
                }

                foreach (Keys key in this.keyboardKeys)
                {
                    if (Keyboard.IsPressed(key)) return true;
                }

                if (triggerAsButton && this.GetTriggerForce(buttonAsTrigger: false) > 0.5f) return true;

                foreach (VButName vbutton in this.virtualButtons)
                { if (VirtButton.IsButtonDown(vbutton)) return true; }

                return false;
            }

            public bool HasBeenPressed(bool triggerAsButton)
            {
                foreach (Buttons button in gamepadButtons)
                {
                    if (GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: button, analogAsDigital: this.gamepadAnalogAsDigital)) return true;
                }

                foreach (Keys key in this.keyboardKeys)
                {
                    if (Keyboard.HasBeenPressed(key)) return true;
                }

                foreach (VButName vbutton in this.virtualButtons)
                { if (VirtButton.HasButtonBeenPressed(vbutton)) return true; }

                if (triggerAsButton && this.TriggerStateLastFrame <= 0.5f && this.GetTriggerForce(buttonAsTrigger: false) > 0.5f) return true;

                if (this.UpdateAndCheckButtonRepeat()) return true;

                return false;
            }

            public bool HasBeenReleased(bool triggerAsButton)
            {
                foreach (Buttons button in gamepadButtons)
                {
                    if (GamePad.HasBeenReleased(playerIndex: PlayerIndex.One, button: button, analogAsDigital: this.gamepadAnalogAsDigital)) return true;
                }

                foreach (Keys key in this.keyboardKeys)
                {
                    if (Keyboard.HasBeenReleased(key)) return true;
                }

                foreach (VButName vbutton in this.virtualButtons)
                { if (VirtButton.HasButtonBeenReleased(vbutton)) return true; }

                if (triggerAsButton && this.TriggerStateLastFrame > 0.5f && (this.GetTriggerForce(buttonAsTrigger: false) <= 0.5f)) return true;

                return false;
            }

            public Vector2 AnalogState
            {
                get
                {
                    Vector2 analogState = Vector2.Zero;
                    bool analogSet = false;

                    foreach (AnalogType analogType in this.analogTypes)
                    {
                        switch (analogType)
                        {
                            case AnalogType.Empty:
                                analogState = Vector2.Zero;
                                break;

                            case AnalogType.PadLeft:
                                analogState = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.Circular).ThumbSticks.Left;
                                analogState = new Vector2(analogState.X, analogState.Y * -1);
                                break;

                            case AnalogType.PadRight:
                                analogState = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.Circular).ThumbSticks.Right;
                                analogState = new Vector2(analogState.X, analogState.Y * -1);
                                break;

                            case AnalogType.VirtLeft:
                                analogState = TouchInput.LeftStick;
                                break;

                            case AnalogType.VirtRight:
                                analogState = TouchInput.RightStick;
                                break;

                            case AnalogType.FromKeys:
                                analogState = ConvertKeysToAnalog(left: this.keysToAnalog[0], right: this.keysToAnalog[1], up: this.keysToAnalog[2], down: this.keysToAnalog[3]);
                                break;


                            default:
                                throw new ArgumentException($"Unsupported analogType - '{analogType}'.");
                        }

                        if (analogState.X != 0 || analogState.Y != 0) analogSet = true;
                        if (analogSet) break;
                    }

                    return analogState;
                }
            }

            public float GetTriggerForce(bool buttonAsTrigger)
            {
                float triggerVal = 0f;
                bool triggerValSet = false;

                foreach (Buttons button in this.gamepadButtons)
                {
                    if (button == Buttons.LeftTrigger) triggerVal = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.IndependentAxes).Triggers.Left;
                    else if (button == Buttons.RightTrigger) triggerVal = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.IndependentAxes).Triggers.Right;

                    if (triggerVal != 0) triggerValSet = true;
                    if (triggerValSet) break;
                }

                if (buttonAsTrigger)
                {
                    if (!triggerValSet && this.IsPressed(triggerAsButton: false))

                    {
                        triggerVal = 1f;
                        triggerValSet = true;
                    }
                }

                if (triggerValSet) this.UpdateTriggerLastState(triggerVal);

                return triggerVal;
            }

            private float TriggerStateLastFrame
            {
                get
                {
                    int lastFrameNo = SonOfRobinGame.currentUpdate - 1;
                    if (!this.triggerStateForFrame.ContainsKey(lastFrameNo)) return 0f;
                    return triggerStateForFrame[lastFrameNo];
                }
            }
            public List<Texture2D> TextureList
            {
                get
                {
                    var textureList = new List<Texture2D>();

                    switch (Input.tipsTypeToShow)
                    {
                        case Input.TipsTypeToShow.Gamepad:
                            foreach (Buttons button in gamepadButtons)
                            { textureList.Add(InputVis.GetTexture(button)); }

                            var padAnalogList = new List<AnalogType> { AnalogType.PadLeft, AnalogType.PadRight };
                            foreach (AnalogType analogType in this.analogTypes)
                            {
                                if (!padAnalogList.Contains(analogType)) continue;
                                textureList.AddRange(InputVis.GetAnalogTextureList(analogType));
                            }

                            break;

                        case Input.TipsTypeToShow.Keyboard:
                            foreach (Keys key in keyboardKeys)
                            { textureList.Add(InputVis.GetTexture(key)); }

                            var keyboardAnalogList = new List<AnalogType> { AnalogType.FromKeys };
                            foreach (AnalogType analogType in this.analogTypes)
                            {
                                if (!keyboardAnalogList.Contains(analogType)) continue;
                                textureList.AddRange(InputVis.GetAnalogTextureList(analogType));
                            }

                            break;

                        default:
                            throw new ArgumentException($"Unsupported LastUsedType - '{Input.tipsTypeToShow}'.");
                    }

                    return textureList;
                }
            }
            private Vector2 ConvertKeysToAnalog(Keys left, Keys right, Keys up, Keys down)
            {
                Vector2 analogState = Vector2.Zero;

                if (Keyboard.IsPressed(left)) analogState.X = -1f;
                if (Keyboard.IsPressed(right)) analogState.X = 1f;
                if (Keyboard.IsPressed(up)) analogState.Y = -1f;
                if (Keyboard.IsPressed(down)) analogState.Y = 1f;

                return analogState;
            }
            private void UpdateTriggerLastState(float triggerVal)
            {
                float triggerStateLastFrame = this.TriggerStateLastFrame;

                this.triggerStateForFrame.Clear();
                this.triggerStateForFrame[SonOfRobinGame.currentUpdate - 1] = triggerStateLastFrame;
                this.triggerStateForFrame[SonOfRobinGame.currentUpdate] = triggerVal;
            }

            private bool UpdateAndCheckButtonRepeat()
            {
                if (!this.repeat) return false;

                if (this.IsPressed(triggerAsButton: true))
                {
                    if (SonOfRobinGame.currentUpdate > this.buttonHeldCurrentFrame)
                    {
                        // there should be no "frame gap" when holding the button
                        if (SonOfRobinGame.currentUpdate == this.buttonHeldCurrentFrame + 1) this.buttonHeldCounter++;
                        else this.buttonHeldCounter = 0;

                        this.buttonHeldCurrentFrame = SonOfRobinGame.currentUpdate;
                    }
                }
                else return false;

                if (this.buttonHeldCounter >= this.repeatThreshold)
                {
                    this.buttonHeldCounter = Math.Max(this.buttonHeldCounter - this.repeatFrames, 0);
                    return true;
                }

                return false;
            }
        }

    }
}

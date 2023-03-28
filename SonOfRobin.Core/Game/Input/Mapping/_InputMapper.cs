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
            GlobalScrollUp,
            GlobalScrollDown,

            WorldWalk,
            WorldCameraMove,
            WorldCameraZoomOut,
            WorldSprintToggle,
            WorldInteract,
            WorldPickUp,
            WorldUseToolbarPiece,
            WorldInventory,
            WorldFieldCraft,
            WorldMapToggle,
            WorldPauseMenu,

            InvSwitch,
            InvSort,
            InvPickOne,
            InvPickStack,
            InvRelease,

            ToolbarPrev,
            ToolbarNext,

            MapToggleMarker,
            MapMove,
            MapCenterPlayer,
            MapZoomIn,
            MapZoomOut,

            MapSwitch,

            SecretLicenceBypass
        }

        public const int defaultRepeatThreshold = 35;
        public const int defaultRepeatFrames = 6;

        public enum AnalogType
        { Empty, PadLeft, PadRight, VirtLeft, VirtRight, FromKeys }

        public enum MouseAction
        { LeftButton, LeftButtonVisOnly, MiddleButton, MiddleButtonVisOnly, RightButton, RightButtonVisOnly, ScrollUp, ScrollDown }

        protected static readonly Dictionary<Action, Mapping> detailedMappings = new Dictionary<Action, Mapping>();

        public static readonly InputPackage defaultMappingGamepad = new InputPackage(packageVersion: InputPackage.version, leftStick: new StoredInput(AnalogType.PadLeft), rightStick: new StoredInput(AnalogType.PadRight), left: StoredInput.empty, right: StoredInput.empty, up: StoredInput.empty, down: StoredInput.empty, confirm: new StoredInput(Buttons.A), cancel: new StoredInput(Buttons.B), pauseMenu: new StoredInput(Buttons.Start), sprint: new StoredInput(Buttons.LeftStick), inventory: new StoredInput(Buttons.Y), pickUp: new StoredInput(Buttons.X), craft: new StoredInput(Buttons.DPadUp), interact: new StoredInput(Buttons.A), map: new StoredInput(Buttons.DPadRight), useTool: new StoredInput(Buttons.RightTrigger), zoomOut: new StoredInput(Buttons.LeftTrigger), toolbarPrev: new StoredInput(Buttons.LeftShoulder), toolbarNext: new StoredInput(Buttons.RightShoulder), invSwitch: new StoredInput(Buttons.LeftStick), invPickOne: new StoredInput(Buttons.Y), invPickStack: new StoredInput(Buttons.X), invSort: new StoredInput(Buttons.RightStick), mapToggleMarker: new StoredInput(Buttons.A), mapCenterPlayer: new StoredInput(Buttons.LeftStick), mapZoomIn: new StoredInput(Buttons.RightTrigger), mapZoomOut: new StoredInput(Buttons.LeftTrigger));

        public static readonly InputPackage defaultMappingKeyboard = new InputPackage(packageVersion: InputPackage.version, leftStick: new StoredInput(AnalogType.FromKeys), rightStick: new StoredInput(AnalogType.Empty), confirm: new StoredInput(Keys.Enter), cancel: new StoredInput(Keys.Escape), pauseMenu: new StoredInput(Keys.Back), sprint: new StoredInput(Keys.NumPad0), inventory: new StoredInput(Keys.Enter), pickUp: new StoredInput(Keys.RightControl), craft: new StoredInput(Keys.NumPad5), interact: new StoredInput(Keys.RightShift), map: new StoredInput(Keys.M), useTool: new StoredInput(Keys.Space), zoomOut: new StoredInput(Keys.NumPad1), toolbarPrev: new StoredInput(Keys.OemOpenBrackets), toolbarNext: new StoredInput(Keys.OemCloseBrackets), invSwitch: new StoredInput(Keys.Tab), invPickOne: new StoredInput(Keys.RightShift), invPickStack: new StoredInput(Keys.Space), invSort: new StoredInput(Keys.LeftShift), mapToggleMarker: new StoredInput(Keys.Space), mapCenterPlayer: new StoredInput(Keys.A), mapZoomIn: new StoredInput(Keys.X), mapZoomOut: new StoredInput(Keys.Z), left: new StoredInput(Keys.Left), right: new StoredInput(Keys.Right), up: new StoredInput(Keys.Up), down: new StoredInput(Keys.Down));

        public static InputPackage currentMappingGamepad = defaultMappingGamepad.MakeCopy();
        public static InputPackage currentMappingKeyboard = defaultMappingKeyboard.MakeCopy();

        public static InputPackage newMappingGamepad = currentMappingGamepad.MakeCopy();
        public static InputPackage newMappingKeyboard = currentMappingKeyboard.MakeCopy();

        public static void RebuildMappings()
        {
            detailedMappings.Clear();

            InputPackage padMap = currentMappingGamepad;
            InputPackage keybMap = currentMappingKeyboard;
            var keysToAnalog = new List<Keys> { keybMap.left.Key, keybMap.right.Key, keybMap.up.Key, keybMap.down.Key };

            // global
            {
                new Mapping(action: Action.GlobalConfirm, anyInputList: new List<object> { keybMap.confirm, MouseAction.LeftButtonVisOnly, padMap.confirm, VButName.Confirm });
                new Mapping(action: Action.GlobalCancelReturnSkip, anyInputList: new List<object> { keybMap.cancel, padMap.cancel, VButName.Return, MouseAction.RightButton });
                new Mapping(action: Action.GlobalLeft, anyInputList: new List<object> { keybMap.left, Buttons.DPadLeft }, gamepadAnalogAsDigital: true, repeat: true);
                new Mapping(action: Action.GlobalRight, anyInputList: new List<object> { keybMap.right, Buttons.DPadRight }, gamepadAnalogAsDigital: true, repeat: true);
                new Mapping(action: Action.GlobalUp, anyInputList: new List<object> { keybMap.up, Buttons.DPadUp }, gamepadAnalogAsDigital: true, repeat: true);
                new Mapping(action: Action.GlobalDown, anyInputList: new List<object> { keybMap.down, Buttons.DPadDown }, gamepadAnalogAsDigital: true, repeat: true);
                new Mapping(action: Action.GlobalScrollUp, anyInputList: new List<object> { MouseAction.ScrollUp });
                new Mapping(action: Action.GlobalScrollDown, anyInputList: new List<object> { MouseAction.ScrollDown });
            }

            // world
            {
                var walkList = new List<object> { AnalogType.VirtLeft, keybMap.leftStick, padMap.leftStick };
                if (Preferences.PointToWalk) walkList.Add(MouseAction.LeftButtonVisOnly);
                new Mapping(action: Action.WorldWalk, anyInputList: walkList, keysToAnalog: keysToAnalog);
                new Mapping(action: Action.WorldCameraMove, anyInputList: new List<object> { AnalogType.VirtRight, padMap.rightStick });
                new Mapping(action: Action.WorldPauseMenu, anyInputList: new List<object> { keybMap.pauseMenu, padMap.pauseMenu, VButName.PauseMenu });
                new Mapping(action: Action.WorldSprintToggle, anyInputList: new List<object> { keybMap.sprint, padMap.sprint, VButName.Sprint });
                new Mapping(action: Action.WorldInventory, anyInputList: new List<object> { keybMap.inventory, padMap.inventory, VButName.Inventory });

                var pickUpList = new List<object> { keybMap.pickUp, padMap.pickUp, VButName.PickUp };
                if (Preferences.PointToWalk && Preferences.PointToInteract) pickUpList.Add(MouseAction.LeftButtonVisOnly);
                new Mapping(action: Action.WorldPickUp, anyInputList: pickUpList);

                var interactList = new List<object> { keybMap.interact, padMap.interact, VButName.Interact };
                if (Preferences.PointToWalk && Preferences.PointToInteract) interactList.Add(MouseAction.LeftButtonVisOnly);
                new Mapping(action: Action.WorldInteract, anyInputList: interactList);

                new Mapping(action: Action.WorldMapToggle, anyInputList: new List<object> { keybMap.map, padMap.map, VButName.Map });
                new Mapping(action: Action.WorldFieldCraft, anyInputList: new List<object> { keybMap.craft, padMap.craft, VButName.FieldCraft });
                new Mapping(action: Action.WorldUseToolbarPiece, anyInputList: new List<object> { keybMap.useTool, MouseAction.RightButton, padMap.useTool, VButName.UseTool, });
                new Mapping(action: Action.WorldCameraZoomOut, anyInputList: new List<object> { keybMap.zoomOut, padMap.zoomOut, VButName.ZoomOut });
            }

            // inventory
            {
                new Mapping(action: Action.InvPickStack, anyInputList: new List<object> { keybMap.invPickStack, padMap.invPickStack });
                new Mapping(action: Action.InvPickOne, anyInputList: new List<object> { keybMap.invPickOne, padMap.invPickOne });
                new Mapping(action: Action.InvRelease, anyInputList: new List<object> { keybMap.confirm, padMap.confirm, keybMap.invPickStack, padMap.invPickStack, keybMap.invPickOne, padMap.invPickOne });
                new Mapping(action: Action.InvSwitch, anyInputList: new List<object> { keybMap.invSwitch, padMap.invSwitch, keybMap.toolbarPrev, keybMap.toolbarNext, padMap.toolbarPrev, padMap.toolbarNext, MouseAction.ScrollUp, MouseAction.ScrollDown });
                new Mapping(action: Action.InvSort, anyInputList: new List<object> { keybMap.invSort, padMap.invSort, VButName.InvSort });
                new Mapping(action: Action.ToolbarPrev, anyInputList: new List<object> { keybMap.toolbarPrev, padMap.toolbarPrev, MouseAction.ScrollUp }, repeat: true);
                new Mapping(action: Action.ToolbarNext, anyInputList: new List<object> { keybMap.toolbarNext, padMap.toolbarNext, MouseAction.ScrollDown }, repeat: true);
            }

            // map
            {
                new Mapping(action: Action.MapToggleMarker, anyInputList: new List<object> { keybMap.mapToggleMarker, padMap.mapToggleMarker, VButName.MapToggleMarker, MouseAction.RightButton });
                new Mapping(action: Action.MapCenterPlayer, anyInputList: new List<object> { keybMap.mapCenterPlayer, padMap.mapCenterPlayer, VButName.MapCenterPlayer });
                new Mapping(action: Action.MapMove, anyInputList: new List<object> { keybMap.leftStick, padMap.leftStick, MouseAction.LeftButtonVisOnly }, keysToAnalog: keysToAnalog); // virtual stick is replaced with direct touch reading
                new Mapping(action: Action.MapSwitch, anyInputList: new List<object> { keybMap.cancel, keybMap.map, padMap.cancel, padMap.map, VButName.Return });
                new Mapping(action: Action.MapZoomIn, anyInputList: new List<object> { keybMap.mapZoomIn, padMap.mapZoomIn, MouseAction.ScrollUp });
                new Mapping(action: Action.MapZoomOut, anyInputList: new List<object> { keybMap.mapZoomOut, padMap.mapZoomOut, MouseAction.ScrollDown });
            }

            // secret licence bypass
            {
                new Mapping(action: Action.SecretLicenceBypass, anyInputList: new List<object> { Keys.LeftControl, Buttons.Start, VButName.Return });
            }
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
                return SonOfRobinGame.WhiteRectangle;
            }

            if (detailedMappings.Count == 0) return SonOfRobinGame.WhiteRectangle;
            var textureList = detailedMappings[action].TextureList;
            if (textureList.Count == 0) return SonOfRobinGame.WhiteRectangle;
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
            private readonly List<MouseAction> mouseActions;
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
                this.mouseActions = new List<MouseAction>();
                this.gamepadButtons = new List<Buttons>();
                this.virtualButtons = new List<VButName>();
                this.analogTypes = new List<AnalogType>();
                this.keysToAnalog = keysToAnalog == null ? new List<Keys>() : keysToAnalog;

                foreach (Object input in anyInputList)
                {
                    if (input == null) continue; // if obsolete mapping was loaded

                    var inputType = input.GetType();

                    if (inputType == typeof(StoredInput))
                    {
                        StoredInput storedInput = (StoredInput)input;
                        switch (storedInput.type)
                        {
                            case StoredInput.Type.Key:
                                this.keyboardKeys.Add(storedInput.Key);
                                break;

                            case StoredInput.Type.Button:
                                this.gamepadButtons.Add(storedInput.Button);
                                break;

                            case StoredInput.Type.VirtButton:
                                this.virtualButtons.Add(storedInput.VirtButton);
                                break;

                            case StoredInput.Type.Analog:
                                this.analogTypes.Add(storedInput.Analog);
                                break;

                            default:
                                throw new ArgumentException($"Unsupported type - '{storedInput.type}'.");
                        }
                    }
                    else if (inputType == typeof(Keys)) this.keyboardKeys.Add((Keys)input);
                    else if (inputType == typeof(Buttons)) this.gamepadButtons.Add((Buttons)input);
                    else if (inputType == typeof(VButName)) this.virtualButtons.Add((VButName)input);
                    else if (inputType == typeof(AnalogType)) this.analogTypes.Add((AnalogType)input);
                    else if (inputType == typeof(MouseAction)) this.mouseActions.Add((MouseAction)input);
                    else throw new ArgumentException($"Unsupported inputType - {inputType}.");
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

                foreach (MouseAction mouseAction in this.mouseActions)
                {
                    switch (mouseAction)
                    {
                        case MouseAction.LeftButton:
                            if (Mouse.LeftIsDown) return true;
                            break;

                        case MouseAction.MiddleButton:
                            if (Mouse.MiddleIsDown) return true;
                            break;

                        case MouseAction.RightButton:
                            if (Mouse.RightIsDown) return true;
                            break;

                        case MouseAction.ScrollUp:
                            if (Mouse.ScrollWheelRolledUp) return true;
                            break;

                        case MouseAction.ScrollDown:
                            if (Mouse.ScrollWheelRolledDown) return true;
                            break;

                        case MouseAction.LeftButtonVisOnly:
                            break;

                        case MouseAction.MiddleButtonVisOnly:
                            break;

                        case MouseAction.RightButtonVisOnly:
                            break;

                        default:
                            throw new ArgumentException($"Unsupported mouseAction - '{mouseAction}'.");
                    }
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

                foreach (MouseAction mouseAction in this.mouseActions)
                {
                    switch (mouseAction)
                    {
                        case MouseAction.LeftButton:
                            if (Mouse.LeftHasBeenPressed) return true;
                            break;

                        case MouseAction.MiddleButton:
                            if (Mouse.MiddleHasBeenPressed) return true;
                            break;

                        case MouseAction.RightButton:
                            if (Mouse.RightHasBeenPressed) return true;
                            break;

                        case MouseAction.ScrollUp:
                            if (Mouse.ScrollWheelRolledUp) return true;
                            break;

                        case MouseAction.ScrollDown:
                            if (Mouse.ScrollWheelRolledDown) return true;
                            break;

                        case MouseAction.LeftButtonVisOnly:
                            break;

                        case MouseAction.MiddleButtonVisOnly:
                            break;

                        case MouseAction.RightButtonVisOnly:
                            break;

                        default:
                            throw new ArgumentException($"Unsupported mouseAction - '{mouseAction}'.");
                    }
                }

                foreach (VButName vbutton in this.virtualButtons)
                { if (VirtButton.HasButtonBeenPressed(vbutton)) return true; }

                if (triggerAsButton && this.TriggerStateLastFrameExists && this.TriggerStateLastFrame <= 0.5f && this.GetTriggerForce(buttonAsTrigger: false) > 0.5f) return true;

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

                foreach (MouseAction mouseAction in this.mouseActions)
                {
                    switch (mouseAction)
                    {
                        case MouseAction.LeftButton:
                            if (Mouse.LeftHasBeenReleased) return true;
                            break;

                        case MouseAction.MiddleButton:
                            if (Mouse.MiddleHasBeenReleased) return true;
                            break;

                        case MouseAction.RightButton:
                            if (Mouse.RightHasBeenReleased) return true;
                            break;

                        case MouseAction.ScrollUp:
                            if (Mouse.ScrollWheelRolledUp) return true;
                            break;

                        case MouseAction.ScrollDown:
                            if (Mouse.ScrollWheelRolledDown) return true;
                            break;

                        case MouseAction.LeftButtonVisOnly:
                            break;

                        case MouseAction.MiddleButtonVisOnly:
                            break;

                        case MouseAction.RightButtonVisOnly:
                            break;

                        default:
                            throw new ArgumentException($"Unsupported mouseAction - '{mouseAction}'.");
                    }
                }

                foreach (VButName vbutton in this.virtualButtons)
                { if (VirtButton.HasButtonBeenReleased(vbutton)) return true; }

                if (triggerAsButton && this.TriggerStateLastFrameExists && this.TriggerStateLastFrame > 0.5f && (this.GetTriggerForce(buttonAsTrigger: false) <= 0.5f)) return true;

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

            private bool TriggerStateLastFrameExists
            { get { return this.triggerStateForFrame.ContainsKey(SonOfRobinGame.CurrentUpdate - 1); } }

            private float TriggerStateLastFrame
            {
                get
                {
                    // TriggerStateLastFrameExists should always be checked first

                    int lastFrameNo = SonOfRobinGame.CurrentUpdate - 1;
                    return triggerStateForFrame[lastFrameNo];
                }
            }

            public List<Texture2D> TextureList
            {
                get
                {
                    var textureList = new List<Texture2D>();

                    switch (Input.currentControlType)
                    {
                        case Input.ControlType.Touch:
                            {
                                break;
                            }

                        case Input.ControlType.Gamepad:
                            foreach (Buttons button in gamepadButtons)
                            { textureList.Add(InputVis.GetTexture(button)); }

                            var padAnalogList = new List<AnalogType> { AnalogType.PadLeft, AnalogType.PadRight };
                            foreach (AnalogType analogType in this.analogTypes)
                            {
                                if (!padAnalogList.Contains(analogType)) continue;
                                textureList.AddRange(InputVis.GetAnalogTextureList(analogType));
                            }

                            break;

                        case Input.ControlType.KeyboardAndMouse:
                            foreach (Keys key in keyboardKeys)
                            { textureList.Add(InputVis.GetTexture(key)); }

                            var keyboardAnalogList = new List<AnalogType> { AnalogType.FromKeys };
                            foreach (AnalogType analogType in this.analogTypes)
                            {
                                if (!keyboardAnalogList.Contains(analogType)) continue;
                                textureList.AddRange(InputVis.GetAnalogTextureList(analogType));
                            }

                            foreach (MouseAction mouseAction in this.mouseActions)
                            {
                                textureList.Add(InputVis.GetTexture(mouseAction));
                            }

                            break;

                        default:
                            throw new ArgumentException($"Unsupported LastUsedType - '{Input.currentControlType}'.");
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
                float triggerStateLastFrame = -1f; // initial value, to be changed below

                bool triggerStateLastFrameExists = this.TriggerStateLastFrameExists;
                if (triggerStateLastFrameExists) triggerStateLastFrame = this.TriggerStateLastFrame;

                this.triggerStateForFrame.Clear();
                if (triggerStateLastFrameExists) this.triggerStateForFrame[SonOfRobinGame.CurrentUpdate - 1] = triggerStateLastFrame;
                this.triggerStateForFrame[SonOfRobinGame.CurrentUpdate] = triggerVal;
            }

            private bool UpdateAndCheckButtonRepeat()
            {
                if (!this.repeat) return false;

                if (this.IsPressed(triggerAsButton: true))
                {
                    if (SonOfRobinGame.CurrentUpdate > this.buttonHeldCurrentFrame)
                    {
                        // there should be no "frame gap" when holding the button
                        if (SonOfRobinGame.CurrentUpdate == this.buttonHeldCurrentFrame + 1) this.buttonHeldCounter++;
                        else this.buttonHeldCounter = 0;

                        this.buttonHeldCurrentFrame = SonOfRobinGame.CurrentUpdate;
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
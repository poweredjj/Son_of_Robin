using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    [Serializable]
    public class InputPackage
    {
        public static readonly float version = 1.02f;

        private static readonly Dictionary<string, string> readablePropertyNames = new Dictionary<string, string>
            {
                {"left", "left"},
                {"right", "right"},
                {"up", "up"},
                {"down", "down"},
                {"leftStick", "movement"},
                {"rightStick", "camera / aim"},
                {"confirm", "confirm"},
                {"cancel", "cancel"},
                {"interact", "interact"},
                {"useTool", "use tool"},
                {"pickUp", "pick up"},
                {"sprint", "sprint"},
                {"zoomOut", "zoom out"},
                {"pauseMenu", "open pause"},
                {"craft", "open craft"},
                {"equip", "open equip"},
                {"inventory", "open inventory"},
                {"invSwitch", "switch active inventory"},
                {"pickOne", "pick item"},
                {"pickStack", "pick stack"},
                {"toolbarPrev", "previous item"},
                {"toolbarNext", "next item"}
            };

        public readonly float packageVersion;

        public InputMapper.AnalogType leftStick;
        public InputMapper.AnalogType rightStick;

        public object left;
        public object right;
        public object up;
        public object down;
        public object confirm;
        public object cancel;
        public object pauseMenu;
        public object sprint;
        public object equip;
        public object inventory;
        public object pickUp;
        public object craft;
        public object interact;
        public object map;
        public object useTool;
        public object zoomOut;
        public object toolbarPrev;
        public object toolbarNext;
        public object invSwitch;
        public object pickOne;
        public object pickStack;

        public bool IsObsolete { get { return this.packageVersion != version; } }
        public InputPackage(float packageVersion, InputMapper.AnalogType leftStick, InputMapper.AnalogType rightStick, object confirm, object cancel, object pauseMenu, object sprint, object equip, object inventory, object pickUp, object craft, object interact, object map, object useTool, object zoomOut, object toolbarPrev, object invSwitch, object toolbarNext, object pickOne, object pickStack, object left = null, object right = null, object up = null, object down = null)
        {
            this.packageVersion = version;

            this.leftStick = leftStick;
            this.rightStick = rightStick;

            this.left = left;
            this.right = right;
            this.up = up;
            this.down = down;
            this.confirm = confirm;
            this.cancel = cancel;
            this.pauseMenu = pauseMenu;
            this.sprint = sprint;
            this.equip = equip;
            this.inventory = inventory;
            this.craft = craft;
            this.pickUp = pickUp;
            this.map = map;
            this.useTool = useTool;
            this.zoomOut = zoomOut;
            this.toolbarPrev = toolbarPrev;
            this.toolbarNext = toolbarNext;
            this.invSwitch = invSwitch;
            this.pickOne = pickOne;
            this.pickStack = pickStack;
            this.interact = interact;
        }

        public string GetReadablePropertyName(string propertyName)
        {
            return readablePropertyNames[propertyName];
        }

        public InputPackage MakeCopy()
        {
            return new InputPackage(
                packageVersion: this.packageVersion,
                leftStick: this.leftStick,
                rightStick: this.rightStick,
                confirm: this.confirm,
                cancel: this.cancel,
                pauseMenu: this.pauseMenu,
                sprint: this.sprint,
                equip: this.equip,
                inventory: this.inventory,
                pickUp: this.pickUp,
                craft: this.craft,
                interact: this.interact,
                map: this.map,
                useTool: this.useTool,
                zoomOut: this.zoomOut,
                toolbarPrev: this.toolbarPrev,
                toolbarNext: this.toolbarNext,
                invSwitch: this.invSwitch,
                pickOne: this.pickOne,
                pickStack: this.pickStack,
                left: this.left,
                right: this.right,
                up: this.up,
                down: this.down);
        }

        public bool Equals(InputPackage inputPackage)
        {
            return
                this.packageVersion == inputPackage.packageVersion &&
                this.leftStick == inputPackage.leftStick &&
                this.rightStick == inputPackage.rightStick &&
                this.confirm == inputPackage.confirm &&
                this.cancel == inputPackage.cancel &&
                this.pauseMenu == inputPackage.pauseMenu &&
                this.sprint == inputPackage.sprint &&
                this.equip == inputPackage.equip &&
                this.inventory == inputPackage.inventory &&
                this.pickUp == inputPackage.pickUp &&
                this.craft == inputPackage.craft &&
                this.interact == inputPackage.interact &&
                this.map == inputPackage.map &&
                this.useTool == inputPackage.useTool &&
                this.zoomOut == inputPackage.zoomOut &&
                this.toolbarPrev == inputPackage.toolbarPrev &&
                this.toolbarNext == inputPackage.toolbarNext &&
                this.invSwitch == inputPackage.invSwitch &&
                this.pickOne == inputPackage.pickOne &&
                this.pickStack == inputPackage.pickStack &&
                this.left == inputPackage.left &&
                this.right == inputPackage.right &&
                this.up == inputPackage.up &&
                this.down == inputPackage.down;
        }

        public bool Validate(bool gamepad)
        {
            // setting lists of properties, that should not be mapped to the same input

            List<List<string>> groupsToCheck = new List<List<string>> {
                { new List<string> { "leftStick", "rightStick" } }, // sticks
                { new List<string> { "confirm", "cancel", "left", "right", "up", "down", "pauseMenu"} }, // general
                { new List<string> { "interact", "pickUp", "sprint", "useTool", "zoomOut", "toolbarPrev", "toolbarNext", "pauseMenu", "equip", "inventory", "craft", "map" } }, // field
                { new List<string> { "invSwitch", "pickOne", "pickStack", "confirm", "cancel", "left", "right", "up", "down" } }, // inventory
            };

            // searching for duplicates and making a dictionary of found duplicates (texture: property list)

            var duplicatedNamesByTextures = new Dictionary<Texture2D, List<string>>();

            foreach (List<string> propertyNameList in groupsToCheck)
            {
                Dictionary<string, object> valuesByNames = propertyNameList.ToDictionary(propertyName => propertyName, propertyName => Helpers.GetProperty(targetObj: this, propertyName: propertyName));

                var duplicatesByValues = this.FindDuplicatesByValues(valuesByNames);
                if (duplicatesByValues.Keys.Count > 0)
                {
                    foreach (var kvp in duplicatesByValues)
                    {
                        Texture2D image = InputVis.GetTexture(kvp.Key);

                        if (!duplicatedNamesByTextures.ContainsKey(image)) duplicatedNamesByTextures[image] = new List<string>();
                        foreach (string name in kvp.Value)
                        {
                            if (!duplicatedNamesByTextures[image].Contains(name)) duplicatedNamesByTextures[image].Add(name);
                        }
                    }
                }
            }

            // converting duplicates dictionary to image list and error list

            List<string> errorList = new List<string>();
            List<Texture2D> imageList = new List<Texture2D>();

            foreach (var kvp in duplicatedNamesByTextures)
            {
                imageList.Add(kvp.Key);
                string error = "| " + String.Join(", ", kvp.Value.ToArray());
                errorList.Add(error);
            }

            // showing message, if duplicates were found

            bool isValid = errorList.Count == 0;
            if (!isValid)
            {
                string type = gamepad ? "Gamepad" : "Keyboard";
                string report = String.Join("\n", errorList.ToArray());

                new TextWindow(text: $"{type} controls cannot be saved.\n\nFound duplicates:\n{report}", imageList: imageList, textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false);
            }

            return isValid;
        }

        private Dictionary<object, List<string>> FindDuplicatesByValues(Dictionary<string, object> valuesByNames)
        {
            var values = new List<object>();
            var duplicatesByValues = new Dictionary<object, List<string>>();

            foreach (var kvp in valuesByNames)
            {
                if (kvp.Value == null) continue;
                if (!values.Contains(kvp.Value)) values.Add(kvp.Value);
                else duplicatesByValues[kvp.Value] = new List<string>();
            }

            var duplicatedValues = duplicatesByValues.Keys.ToList();

            foreach (var kvp in valuesByNames)
            {
                if (duplicatedValues.Contains(kvp.Value)) duplicatesByValues[kvp.Value].Add(this.GetReadablePropertyName(kvp.Key));
            }

            return duplicatesByValues;
        }

    }
}

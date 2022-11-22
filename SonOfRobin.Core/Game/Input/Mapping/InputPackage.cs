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
        public const float version = 1.07f;

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
                {"inventory", "open inventory"},
                {"invSwitch", "switch active inventory"},
                {"invPickOne", "pick item"},
                {"invPickStack", "pick stack"},
                {"invSort", "sort"},
                {"toolbarPrev", "previous item"},
                {"toolbarNext", "next item"},
                {"mapToggleMarker", "toggle marker"},
                {"mapCenterPlayer", "center on player"},
                {"mapZoomIn", "zoom in"},
                {"mapZoomOut", "zoom out"},
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
        public object invPickOne;
        public object invPickStack;
        public object invSort;
        public object mapToggleMarker;
        public object mapCenterPlayer;
        public object mapZoomIn;
        public object mapZoomOut;

        public bool IsObsolete { get { return this.packageVersion != version; } }
        public InputPackage(float packageVersion, InputMapper.AnalogType leftStick, InputMapper.AnalogType rightStick, object confirm, object cancel, object pauseMenu, object sprint, object inventory, object pickUp, object craft, object interact, object map, object useTool, object zoomOut, object toolbarPrev, object invSwitch, object invSort, object toolbarNext, object invPickOne, object invPickStack, object mapToggleMarker, object mapCenterPlayer, object mapZoomIn, object mapZoomOut, object left = null, object right = null, object up = null, object down = null)
        {
            this.packageVersion = packageVersion;

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
            this.inventory = inventory;
            this.craft = craft;
            this.pickUp = pickUp;
            this.map = map;
            this.useTool = useTool;
            this.zoomOut = zoomOut;
            this.toolbarPrev = toolbarPrev;
            this.toolbarNext = toolbarNext;
            this.invSort = invSort;
            this.invSwitch = invSwitch;
            this.invPickOne = invPickOne;
            this.invPickStack = invPickStack;
            this.interact = interact;
            this.mapToggleMarker = mapToggleMarker;
            this.mapCenterPlayer = mapCenterPlayer;
            this.mapZoomIn = mapZoomIn;
            this.mapZoomOut = mapZoomOut;
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
                invPickOne: this.invPickOne,
                invPickStack: this.invPickStack,
                invSort: this.invSort,
                mapToggleMarker: this.mapToggleMarker,
                mapCenterPlayer: this.mapCenterPlayer,
                mapZoomIn: this.mapZoomIn,
                mapZoomOut: this.mapZoomOut,
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
                this.invPickOne == inputPackage.invPickOne &&
                this.invPickStack == inputPackage.invPickStack &&
                this.invSort == inputPackage.invSort &&
                this.mapToggleMarker == inputPackage.mapToggleMarker &&
                this.mapCenterPlayer == inputPackage.mapCenterPlayer &&
                this.mapZoomIn == inputPackage.mapZoomIn &&
                this.mapZoomOut == inputPackage.mapZoomOut &&
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
                { new List<string> { "interact", "pickUp", "sprint", "useTool", "zoomOut", "toolbarPrev", "toolbarNext", "pauseMenu", "inventory", "craft", "map" } }, // field
                { new List<string> { "invSwitch", "invPickOne", "invPickStack", "invSort", "confirm", "cancel", "left", "right", "up", "down" } }, // inventory
                { new List<string> { "cancel", "mapToggleMarker", "mapCenterPlayer", "mapZoomIn", "mapZoomOut" } }, // map
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    [Serializable]
    public class MappingPackage
    {
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
                {"run", "run"},
                {"zoomOut", "zoom out"},
                {"pauseMenu", "open pause"},
                {"craft", "open craft"},
                {"equip", "open equip"},
                {"inventory", "open inventory"},
                {"pickOne", "pick item"},
                {"pickStack", "pick stack"},
                {"toolbarPrev", "previous item"},
                {"toolbarNext", "next item"}
            };

        public InputMapper.AnalogType leftStick;
        public InputMapper.AnalogType rightStick;

        public object left; // used only for validation
        public object right; // used only for validation
        public object up; // used only for validation
        public object down; // used only for validation
        public object confirm;
        public object cancel;
        public object pauseMenu;
        public object run;
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
        public object pickOne;
        public object pickStack;
        public MappingPackage(InputMapper.AnalogType leftStick, InputMapper.AnalogType rightStick, object confirm, object cancel, object pauseMenu, object run, object equip, object inventory, object pickUp, object craft, object interact, object map, object useTool, object zoomOut, object toolbarPrev, object toolbarNext, object pickOne, object pickStack)
        {
            this.leftStick = leftStick;
            this.rightStick = rightStick;

            var directionKeys = InputMapper.SplitKeySet(this.leftStick);
            if (directionKeys.Count > 0)
            {
                this.left = directionKeys[0];
                this.right = directionKeys[1];
                this.up = directionKeys[2];
                this.down = directionKeys[3];
            }
            this.confirm = confirm;
            this.cancel = cancel;
            this.pauseMenu = pauseMenu;
            this.run = run;
            this.equip = equip;
            this.inventory = inventory;
            this.craft = craft;
            this.pickUp = pickUp;
            this.map = map;
            this.useTool = useTool;
            this.zoomOut = zoomOut;
            this.toolbarPrev = toolbarPrev;
            this.toolbarNext = toolbarNext;
            this.pickOne = pickOne;
            this.pickStack = pickStack;
            this.interact = interact;
        }

        public string GetReadablePropertyName(string propertyName)
        {
            return readablePropertyNames[propertyName];
        }

        public MappingPackage MakeCopy()
        {
            return new MappingPackage(leftStick: this.leftStick, rightStick: this.rightStick, confirm: this.confirm, cancel: this.cancel, pauseMenu: this.pauseMenu, run: this.run, equip: this.equip, inventory: this.inventory, pickUp: this.pickUp, craft: this.craft, interact: this.interact, map: this.map, useTool: this.useTool, zoomOut: this.zoomOut, toolbarPrev: this.toolbarPrev, toolbarNext: this.toolbarNext, pickOne: this.pickOne, pickStack: this.pickStack);
        }

        public bool Validate(bool gamepad)
        {
            // setting lists of properties, that should not be mapped to the same input

            List<List<string>> groupsToCheck = new List<List<string>> {
                { new List<string> { "leftStick", "rightStick" } }, // sticks
                { new List<string> { "confirm", "cancel", "left", "right", "up", "down", "pauseMenu"} }, // general
                { new List<string> { "interact", "pickUp", "run", "useTool", "zoomOut", "toolbarPrev", "toolbarNext", "pauseMenu", "equip", "inventory", "craft", "map" } }, // field
                { new List<string> { "pickOne", "pickStack", "confirm", "cancel", "left", "right", "up", "down" } }, // inventory
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

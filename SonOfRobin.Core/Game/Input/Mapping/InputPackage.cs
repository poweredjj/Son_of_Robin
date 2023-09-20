using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class InputPackage
    {
        public const float version = 1.142f;

        private static readonly Dictionary<string, string> readablePropertyNames = new()
        {
                {"left", "left"},
                {"right", "right"},
                {"up", "up"},
                {"down", "down"},
                {"analogMovement", "movement"},
                {"analogCamera", "camera / aim"},
                {"confirm", "confirm"},
                {"cancel", "cancel"},
                {"interact", "interact"},
                {"useTool", "use tool"},
                {"pickUp", "pick up"},
                {"highlightPickups", "highlight pickups"},
                {"sprint", "sprint"},
                {"zoomOut", "zoom out"},
                {"pauseMenu", "open pause"},
                {"craft", "field craft"},
                {"inventory", "open inventory"},
                {"invSwitch", "switch active inventory"},
                {"invPickOne", "pick item"},
                {"invPickStack", "pick stack"},
                {"invSort", "sort"},
                {"toolbarPrev", "previous item"},
                {"toolbarNext", "next item"},
                {"mapToggleMarker", "toggle marker"},
                {"mapDeleteMarkers", "delete all markers"},
                {"mapToggleLocations", "toggle locations"},
                {"mapCenterPlayer", "center on player"},
                {"mapZoomIn", "zoom in"},
                {"mapZoomOut", "zoom out"},
            };

        public readonly float packageVersion;

        public StoredInput analogMovement;
        public StoredInput analogCamera;
        public StoredInput left;
        public StoredInput right;
        public StoredInput up;
        public StoredInput down;
        public StoredInput confirm;
        public StoredInput cancel;
        public StoredInput pauseMenu;
        public StoredInput sprint;
        public StoredInput inventory;
        public StoredInput pickUp;
        public StoredInput highlightPickups;
        public StoredInput craft;
        public StoredInput interact;
        public StoredInput map;
        public StoredInput useTool;
        public StoredInput zoomOut;
        public StoredInput toolbarPrev;
        public StoredInput toolbarNext;
        public StoredInput invSwitch;
        public StoredInput invPickOne;
        public StoredInput invPickStack;
        public StoredInput invSort;
        public StoredInput mapToggleMarker;
        public StoredInput mapDeleteMarkers;
        public StoredInput mapToggleLocations;
        public StoredInput mapCenterPlayer;
        public StoredInput mapZoomIn;
        public StoredInput mapZoomOut;

        public bool IsObsolete
        { get { return this.packageVersion != version; } }

        public InputPackage(float packageVersion, StoredInput analogMovement, StoredInput analogCamera, StoredInput confirm, StoredInput cancel, StoredInput pauseMenu, StoredInput sprint, StoredInput inventory, StoredInput pickUp, StoredInput highlightPickups, StoredInput craft, StoredInput interact, StoredInput map, StoredInput useTool, StoredInput zoomOut, StoredInput toolbarPrev, StoredInput invSwitch, StoredInput invSort, StoredInput toolbarNext, StoredInput invPickOne, StoredInput invPickStack, StoredInput mapToggleMarker, StoredInput mapDeleteMarkers, StoredInput mapToggleLocations, StoredInput mapCenterPlayer, StoredInput mapZoomIn, StoredInput mapZoomOut, StoredInput left = null, StoredInput right = null, StoredInput up = null, StoredInput down = null)
        {
            this.packageVersion = packageVersion;

            this.analogMovement = analogMovement;
            this.analogCamera = analogCamera;
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
            this.highlightPickups = highlightPickups;
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
            this.mapDeleteMarkers = mapDeleteMarkers;
            this.mapToggleLocations = mapToggleLocations;
            this.mapCenterPlayer = mapCenterPlayer;
            this.mapZoomIn = mapZoomIn;
            this.mapZoomOut = mapZoomOut;
        }

        public static string GetReadablePropertyName(string propertyName)
        {
            return readablePropertyNames[propertyName];
        }

        public InputPackage MakeCopy()
        {
            return new InputPackage(
                packageVersion: this.packageVersion,
                analogMovement: this.analogMovement,
                analogCamera: this.analogCamera,
                confirm: this.confirm,
                cancel: this.cancel,
                pauseMenu: this.pauseMenu,
                sprint: this.sprint,
                inventory: this.inventory,
                pickUp: this.pickUp,
                highlightPickups: this.highlightPickups,
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
                mapDeleteMarkers: this.mapDeleteMarkers,
                mapToggleLocations: this.mapToggleLocations,
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
                this.analogMovement == inputPackage.analogMovement &&
                this.analogCamera == inputPackage.analogCamera &&
                this.confirm == inputPackage.confirm &&
                this.cancel == inputPackage.cancel &&
                this.pauseMenu == inputPackage.pauseMenu &&
                this.sprint == inputPackage.sprint &&
                this.inventory == inputPackage.inventory &&
                this.pickUp == inputPackage.pickUp &&
                this.highlightPickups == inputPackage.highlightPickups &&
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
                this.mapDeleteMarkers == inputPackage.mapDeleteMarkers &&
                this.mapToggleLocations == inputPackage.mapToggleLocations &&
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

            List<List<string>> groupsToCheck = new()
            {
                { new List<string> { "analogMovement", "analogCamera" } }, // sticks
                { new List<string> { "confirm", "cancel", "left", "right", "up", "down", "pauseMenu"} }, // general
                { new List<string> { "interact", "pickUp", "highlightPickups", "sprint", "useTool", "zoomOut", "toolbarPrev", "toolbarNext", "pauseMenu", "inventory", "craft", "map" } }, // field
                { new List<string> { "invSwitch", "invPickOne", "invPickStack", "invSort", "confirm", "cancel", "left", "right", "up", "down" } }, // inventory
                { new List<string> { "cancel", "mapToggleMarker", "mapDeleteMarkers", "mapCenterPlayer", "mapZoomIn", "mapZoomOut", "mapToggleLocations" } }, // map
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

            List<string> errorList = new();
            List<Texture2D> imageList = new();

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

            var duplicatedValues = duplicatesByValues.Keys;

            foreach (var kvp in valuesByNames)
            {
                if (kvp.Value == null) continue;
                if (duplicatedValues.Contains(kvp.Value)) duplicatesByValues[kvp.Value].Add(GetReadablePropertyName(kvp.Key));
            }

            return duplicatesByValues;
        }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> packageData = new();

            packageData["version"] = version;

            packageData["leftStick"] = analogMovement.Serialize();
            packageData["rightStick"] = analogCamera.Serialize();
            packageData["left"] = left?.Serialize();
            packageData["right"] = right?.Serialize();
            packageData["up"] = up?.Serialize();
            packageData["down"] = down?.Serialize();
            packageData["confirm"] = confirm.Serialize();
            packageData["cancel"] = cancel.Serialize();
            packageData["pauseMenu"] = pauseMenu.Serialize();
            packageData["sprint"] = sprint.Serialize();
            packageData["inventory"] = inventory.Serialize();
            packageData["pickUp"] = pickUp.Serialize();
            packageData["highlightPickups"] = highlightPickups.Serialize();
            packageData["craft"] = craft.Serialize();
            packageData["interact"] = interact.Serialize();
            packageData["map"] = map.Serialize();
            packageData["useTool"] = useTool.Serialize();
            packageData["zoomOut"] = zoomOut.Serialize();
            packageData["toolbarPrev"] = toolbarPrev.Serialize();
            packageData["toolbarNext"] = toolbarNext.Serialize();
            packageData["invSwitch"] = invSwitch.Serialize();
            packageData["invPickOne"] = invPickOne.Serialize();
            packageData["invPickStack"] = invPickStack.Serialize();
            packageData["invSort"] = invSort.Serialize();
            packageData["mapToggleMarker"] = mapToggleMarker.Serialize();
            packageData["mapDeleteMarkers"] = mapDeleteMarkers.Serialize();
            packageData["mapToggleLocations"] = mapToggleLocations.Serialize();
            packageData["mapCenterPlayer"] = mapCenterPlayer.Serialize();
            packageData["mapZoomIn"] = mapZoomIn.Serialize();
            packageData["mapZoomOut"] = mapZoomOut.Serialize();

            return packageData;
        }

        public static InputPackage Deserialize(Object inputData)
        {
            var inputDict = (Dictionary<string, Object>)inputData;

            float version = (float)(double)inputDict["version"];

            StoredInput analogMovement = StoredInput.Deserialize(inputDict["leftStick"]);
            StoredInput analogCamera = StoredInput.Deserialize(inputDict["rightStick"]);
            StoredInput left = StoredInput.Deserialize(inputDict["left"]);
            StoredInput right = StoredInput.Deserialize(inputDict["right"]);
            StoredInput up = StoredInput.Deserialize(inputDict["up"]);
            StoredInput down = StoredInput.Deserialize(inputDict["down"]);
            StoredInput confirm = StoredInput.Deserialize(inputDict["confirm"]);
            StoredInput cancel = StoredInput.Deserialize(inputDict["cancel"]);
            StoredInput pauseMenu = StoredInput.Deserialize(inputDict["pauseMenu"]);
            StoredInput sprint = StoredInput.Deserialize(inputDict["sprint"]);
            StoredInput inventory = StoredInput.Deserialize(inputDict["inventory"]);
            StoredInput pickUp = StoredInput.Deserialize(inputDict["pickUp"]);
            StoredInput highlightPickups = StoredInput.Deserialize(inputDict["highlightPickups"]);
            StoredInput craft = StoredInput.Deserialize(inputDict["craft"]);
            StoredInput interact = StoredInput.Deserialize(inputDict["interact"]);
            StoredInput map = StoredInput.Deserialize(inputDict["map"]);
            StoredInput useTool = StoredInput.Deserialize(inputDict["useTool"]);
            StoredInput zoomOut = StoredInput.Deserialize(inputDict["zoomOut"]);
            StoredInput toolbarPrev = StoredInput.Deserialize(inputDict["toolbarPrev"]);
            StoredInput toolbarNext = StoredInput.Deserialize(inputDict["toolbarNext"]);
            StoredInput invSwitch = StoredInput.Deserialize(inputDict["invSwitch"]);
            StoredInput invPickOne = StoredInput.Deserialize(inputDict["invPickOne"]);
            StoredInput invPickStack = StoredInput.Deserialize(inputDict["invPickStack"]);
            StoredInput invSort = StoredInput.Deserialize(inputDict["invSort"]);
            StoredInput mapToggleMarker = StoredInput.Deserialize(inputDict["mapToggleMarker"]);
            StoredInput mapDeleteMarkers = StoredInput.Deserialize(inputDict["mapDeleteMarkers"]);
            StoredInput mapToggleLocations = StoredInput.Deserialize(inputDict["mapToggleLocations"]);
            StoredInput mapCenterPlayer = StoredInput.Deserialize(inputDict["mapCenterPlayer"]);
            StoredInput mapZoomIn = StoredInput.Deserialize(inputDict["mapZoomIn"]);
            StoredInput mapZoomOut = StoredInput.Deserialize(inputDict["mapZoomOut"]);

            return new InputPackage(packageVersion: version, analogMovement: analogMovement, analogCamera: analogCamera, left: left, right: right, up: up, down: down, confirm: confirm, cancel: cancel, pauseMenu: pauseMenu, sprint: sprint, inventory: inventory, pickUp: pickUp, highlightPickups: highlightPickups, craft: craft, interact: interact, map: map, useTool: useTool, zoomOut: zoomOut, toolbarPrev: toolbarPrev, toolbarNext: toolbarNext, invSwitch: invSwitch, invPickOne: invPickOne, invPickStack: invPickStack, invSort: invSort, mapDeleteMarkers: mapDeleteMarkers, mapToggleMarker: mapToggleMarker, mapToggleLocations: mapToggleLocations, mapCenterPlayer: mapCenterPlayer, mapZoomIn: mapZoomIn, mapZoomOut: mapZoomOut);
        }
    }
}
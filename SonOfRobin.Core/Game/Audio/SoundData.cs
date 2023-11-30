﻿using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class SoundData
    {
        private static readonly Dictionary<Name, SoundEffect> soundsDict = new();
        public static readonly Name[] allNames = (Name[])Enum.GetValues(typeof(Name));

        public static void LoadMostSounds() // not needed anymore
        {
            if (soundsDict.Count > 0) throw new ArgumentException("Sounds has already been loaded.");

            foreach (Name soundName in soundFilenamesDict.Keys)
            {
                LoadOneSound(soundName: soundName);
            }
        }

        public static SoundEffect GetSound(Name soundName)
        {
            if (!soundsDict.ContainsKey(soundName)) LoadOneSound(soundName);
            return soundsDict[soundName];
        }

        public static void LoadOneSound(Name soundName)
        {
            if (!soundsDict.ContainsKey(soundName)) soundsDict[soundName] = SonOfRobinGame.ContentMgr.Load<SoundEffect>($"sound/{soundFilenamesDict[soundName]}");
        }

        public enum Name : byte
        {
            Empty = 0,

            Navigation = 1,
            Tick = 2,
            Beep = 3,
            Select = 4,
            Invoke = 5,
            Notification1 = 6,
            Notification2 = 7,
            Notification3 = 8,
            Notification4 = 9,
            DunDunDun = 10,
            Ding1 = 11,
            Ding2 = 12,
            Ding3 = 13,
            Ding4 = 14,
            Chime = 15,
            Error = 16,

            NewGameStart = 17,

            InventoryOpen = 18,
            ChestOpen = 19,
            ChestClose = 20,

            PickUpItem = 21,

            DestroyWood = 22,
            DestroyMetal = 23,
            DestroyCrystal = 24,
            DestroyBox = 25,
            DestroyTree = 26,
            DestroyRock1 = 27,
            DestroyRock2 = 28,
            DestroyStump = 29,
            DestroyFlesh1 = 30,
            DestroyFlesh2 = 31,
            DestroyCeramic1 = 32,
            DestroyCeramic2 = 33,
            DestroyCeramic3 = 34,
            DestroySmallPlant1 = 35,
            DestroySmallPlant2 = 36,
            DestroySmallPlant3 = 37,
            DestroySmallPlant4 = 38,
            DestroySmallPlant5 = 39,
            DestroySmallPlant6 = 40,

            HitSmallPlant1 = 41,
            HitSmallPlant2 = 42,
            HitSmallPlant3 = 43,
            HitRock1 = 44,
            HitRock2 = 45,
            HitRock3 = 46,
            HitRock4 = 47,
            HitRock5 = 48,
            HitRock6 = 49,
            HitFlesh1 = 50,
            HitFlesh2 = 51,
            HitCeramic = 52,
            HitCrystal = 53,
            HitWood = 54,
            HitMetal = 55,

            DropGeneric = 56,
            DropPlant = 57,
            DropMud = 58,
            DropStick = 59,
            DropSand = 60,
            DropIronRod = 61,
            DropIronBar = 62,
            DropIronPlate = 63,

            DropMeat1 = 64,
            DropMeat2 = 65,
            DropMeat3 = 66,

            Bonfire = 67,
            Torch = 68,
            FryingPan = 69,
            Cooking = 70,

            PotLid = 71,
            StoneMove1 = 72,
            StoneMove2 = 73,
            FireBurst = 74,
            HammerHits = 75,
            BoilingPotion = 76,
            ToolsMove = 77,
            PaperMove1 = 78,
            PaperMove2 = 79,
            TurnPage = 80,

            GameOver = 81,

            CrySmallAnimal1 = 82,
            CrySmallAnimal2 = 83,
            CrySmallAnimal3 = 84,
            CrySmallAnimal4 = 85,

            EatHerbivore1 = 86,
            EatHerbivore2 = 87,
            EatHerbivore3 = 88,
            EatHerbivore4 = 89,
            EatHerbivore5 = 90,

            EatPredator1 = 91,
            EatPredator2 = 92,
            EatPredator3 = 93,
            EatPredator4 = 94,

            DropCrystal = 95,
            DropGlass = 96,
            DropArrow = 97,
            DropRope = 98,

            BearRoar = 99,

            EatPlayer1 = 100,
            EatPlayer2 = 101,
            EatPlayer3 = 102,
            EatPlayer4 = 103,

            Sprint = 104,
            Sawing = 105,
            Hammering = 106,
            Planting = 107,
            MovingPlant = 108,
            StartFireSmall = 109,
            StartFireBig = 110,
            EndFire = 111,
            WaterSplash = 112,
            Drink = 113,
            Pulse = 114,
            Cough = 115,
            BowDraw = 116,
            BowRelease = 117,
            ArrowFly = 118,
            ArrowHit = 119,
            ClothRustle1 = 120,
            ClothRustle2 = 121,
            KnifeSharpen = 122,
            LeatherMove = 123,
            SplashMud = 124,

            SnoringFemale = 125,
            SnoringMale = 126,

            YawnFemale = 127,
            YawnMale = 128,

            StomachGrowl = 129,

            CryPlayerMale1 = 130,
            CryPlayerMale2 = 131,
            CryPlayerMale3 = 132,
            CryPlayerMale4 = 133,

            CryPlayerFemale1 = 134,
            CryPlayerFemale2 = 135,
            CryPlayerFemale3 = 136,
            CryPlayerFemale4 = 137,

            DeathPlayerMale = 138,
            DeathPlayerFemale = 139,

            PantMale = 140,
            PantFemale = 141,

            CryFrog1 = 142,
            CryFrog2 = 143,
            CryFrog3 = 144,
            CryFrog4 = 145,

            Dig1 = 146,
            Dig2 = 147,
            Dig3 = 148,
            Dig4 = 149,

            StepGrass1 = 150,
            StepGrass2 = 151,
            StepGrass3 = 152,
            StepGrass4 = 153,
            StepGrass5 = 154,
            StepGrass6 = 155,

            StepWater = 156,

            SwimShallow = 157,
            SwimDeep1 = 158,
            SwimDeep2 = 159,
            SwimDeep3 = 160,
            SwimDeep4 = 161,
            SwimDeep5 = 162,

            StepSand1 = 163,
            StepSand2 = 164,
            StepSand3 = 165,
            StepSand4 = 166,
            StepLava = 167,

            StepRock1 = 168,
            StepRock2 = 169,
            StepRock3 = 170,

            StepMud1 = 171,
            StepMud2 = 172,
            StepMud3 = 173,
            StepMud4 = 174,
            StepMud5 = 175,
            StepMud6 = 176,
            StepMud7 = 177,

            StepGhost = 178,

            AngelChorus = 179,

            SeaWave1 = 180,
            SeaWave2 = 181,
            SeaWave3 = 182,
            SeaWave4 = 183,
            SeaWave5 = 184,
            SeaWave6 = 185,
            SeaWave7 = 186,
            SeaWave8 = 187,
            SeaWave9 = 188,
            SeaWave10 = 189,
            SeaWave11 = 190,
            SeaWave12 = 191,
            SeaWave13 = 192,

            LakeWave1 = 193,
            LakeWave2 = 194,
            LakeWave3 = 195,
            LakeWave4 = 196,
            LakeWave5 = 197,
            LakeWave6 = 198,
            LakeWave7 = 199,

            Cicadas1 = 200,
            Cicadas2 = 201,
            Cicadas3 = 202,

            Lava = 203,
            SeaWind = 204,
            WeatherWind = 205,
            Rain = 206,
            NightCrickets = 207,

            Thunder1 = 208,
            Thunder2 = 209,
            Thunder3 = 210,
            Thunder4 = 211,
            ElectricShock = 212,
            MetalicClank = 213,
            FireBurnShort = 214,
            ShootFire = 215,
            AnvilHit = 216,
            BoilingPotionLoop = 217,
            DropLeather = 218,
            HitLeather = 219,
            DestroyLeather = 220,
            Jump = 221,
            SteamHit = 222,
            DropPlastic = 223,
            TrumpetChime = 224,
            CreepyCave = 225,
            Rattle = 226,
            BonusItem = 227,
            Whoosh = 228,
            Glug = 229,
            WaterDrop = 230,
            StonesFalling = 231,
            WoodCreak = 232,
            OpenSeaStormLoop = 233,
            OpenSeaStormCrash = 234,
            BoatHorn = 235,
            MusicBoxEnding = 236,
        }

        public static readonly Dictionary<Name, string> soundFilenamesDict = new()
        {
            { Name.Navigation, "628638__el-boss__menu-select-tick" },
            { Name.Tick, "268108__nenadsimic__button-tick" },
            { Name.Beep, "616821__trp__cell-phone-beep-2012" },
            { Name.Select, "575542__tissman__menu-move7" },
            { Name.Invoke, "531852__tissman__menu-click-1" },
            { Name.NewGameStart, "567204__sergequadrado__announcement-sound-4" },
            { Name.InventoryOpen, "568992__fission9__inventory-open" },
            { Name.ChestOpen, "391947__ssierra1202__chest-openning-crack" },
            { Name.ChestClose, "411790__inspectorj__door-wooden-close-a-h4n" },
            { Name.DestroyBox, "563175__gronnie__wooden-box-breaking-sound-effects" },
            { Name.HitWood, "257929__kane53126__bat-hit-against-wall" },
            { Name.Dig1, "367010__tanapistorius__digging_1" },
            { Name.Dig2, "367010__tanapistorius__digging_2" },
            { Name.Dig3, "367010__tanapistorius__digging_3" },
            { Name.Dig4, "79560__robinhood76__01259-wet-sand-digging-1" },
            { Name.DestroyTree, "139952__ecfike__a-tree-falling-down" },
            { Name.DestroyStump, "183450__utsuru__wood-crack-5" },
            { Name.GameOver, "187809__sonsdebarcelona__low-piano-note" },
            { Name.StepGrass1, "521587__fission9__hiking-boot-footsteps-on-grass-1" },
            { Name.StepGrass2, "521587__fission9__hiking-boot-footsteps-on-grass-2" },
            { Name.StepGrass3, "521587__fission9__hiking-boot-footsteps-on-grass-3" },
            { Name.StepGrass4, "521587__fission9__hiking-boot-footsteps-on-grass-4" },
            { Name.StepGrass5, "521587__fission9__hiking-boot-footsteps-on-grass-5" },
            { Name.StepGrass6, "521587__fission9__hiking-boot-footsteps-on-grass-6" },
            { Name.HitRock1, "587904__somehumbleonion__rock-hits-with-hammer-1" },
            { Name.HitRock2, "587904__somehumbleonion__rock-hits-with-hammer-2" },
            { Name.HitRock3, "587904__somehumbleonion__rock-hits-with-hammer-3" },
            { Name.HitRock4, "587904__somehumbleonion__rock-hits-with-hammer-4" },
            { Name.HitRock5, "587904__somehumbleonion__rock-hits-with-hammer-5" },
            { Name.HitRock6, "587904__somehumbleonion__rock-hits-with-hammer-6" },
            { Name.DestroyRock1, "524312__bertsz__rock-destroy" },
            { Name.DestroyRock2, "131554__shaynecantly__price-rock-crash" },
            { Name.DestroyFlesh1, "627927__pandartb3d__smashed-egg" },
            { Name.DestroyFlesh2, "87572__huluvu42__platzender-kopf-nachschlag" },
            { Name.CrySmallAnimal1, "164882__timsc__squirrel-call-1" },
            { Name.CrySmallAnimal2, "164882__timsc__squirrel-call-2" },
            { Name.CrySmallAnimal3, "164882__timsc__squirrel-call-3" },
            { Name.CrySmallAnimal4, "164882__timsc__squirrel-call-4" },
            { Name.CryFrog1, "484763__inspectorj__florida-treefrog-close-in-air-vent-01-1" },
            { Name.CryFrog2, "484763__inspectorj__florida-treefrog-close-in-air-vent-01-2" },
            { Name.CryFrog3, "484763__inspectorj__florida-treefrog-close-in-air-vent-01-3" },
            { Name.CryFrog4, "484763__inspectorj__florida-treefrog-close-in-air-vent-01-4" },
            { Name.HitFlesh1, "522091__magnuswaker__pound-of-flesh-1" },
            { Name.HitFlesh2, "620354__marb7e__whoosh-sword-hit-flesh" },
            { Name.BearRoar, "614013__aarontheonly__roar9" },
            { Name.PickUpItem, "467610__triqystudio__pickupitem" },
            { Name.DestroyCeramic1, "214336__zerolagtime__breaking-a-vase-remix" },
            { Name.DestroyCeramic2, "194683__kingsrow__breakingvase03" },
            { Name.DestroyCeramic3, "583310__fission9__tile-breaking" },
            { Name.Bonfire, "195586__scott-snailham__bonfire-fx-with-whoosh-basic-with-some-crackle-cd-quality" },
            { Name.Torch, "363092__fractalstudios__fire-crackle-and-flames-002" },
            { Name.HitCeramic, "615308__fractanimal__ceramic-cup-hit" },
            { Name.Ding1, "320655__rhodesmas__level-up-01" },
            { Name.Ding2, "406243__stubb__typewriter-ding-near-mono" },
            { Name.Ding3, "13934__adcbicycle__6" },
            { Name.Ding4, "614981__simonegianni__beepc6" },
            { Name.Chime, "321263__rhodesmas__coins-purchase-01" },
            { Name.DestroyWood, "66778__kevinkace__crate-break-2" },
            { Name.SwimShallow, "317067__robinhood76__05913-swimming-loop-shallow_1" },
            { Name.SwimDeep1, "317067__robinhood76__05913-swimming-loop-1" },
            { Name.SwimDeep2, "317067__robinhood76__05913-swimming-loop-2" },
            { Name.SwimDeep3, "317067__robinhood76__05913-swimming-loop-3" },
            { Name.SwimDeep4, "317067__robinhood76__05913-swimming-loop-4" },
            { Name.SwimDeep5, "317067__robinhood76__05913-swimming-loop-5" },
            { Name.StepWater, "145242__nathanaelj83__water-step" },
            { Name.StepSand1, "540728__lukeo135__sand-step-1" },
            { Name.StepSand2, "540728__lukeo135__sand-step-2" },
            { Name.StepSand3, "540728__lukeo135__sand-step-3" },
            { Name.StepSand4, "540728__lukeo135__sand-step-4" },
            { Name.StepRock1, "434895__julius-galla__steps-on-loose-stones-actions-1" },
            { Name.StepRock2, "434895__julius-galla__steps-on-loose-stones-actions-2" },
            { Name.StepRock3, "434895__julius-galla__steps-on-loose-stones-actions-3" },
            { Name.StepMud1, "488068__bendrain__footsteps-mud-01_1" },
            { Name.StepMud2, "488068__bendrain__footsteps-mud-01_2" },
            { Name.StepMud3, "488068__bendrain__footsteps-mud-01_3" },
            { Name.StepMud4, "488068__bendrain__footsteps-mud-01_4" },
            { Name.StepMud5, "488068__bendrain__footsteps-mud-01_5" },
            { Name.StepMud6, "488068__bendrain__footsteps-mud-01_6" },
            { Name.StepMud7, "488068__bendrain__footsteps-mud-01_7" },
            { Name.HitCrystal, "562199__gristi__snd-small-object-hit" },
            { Name.DestroyCrystal, "542560__matthewholdensound__owi-crystalshatter-2" },
            { Name.EatPredator1, "233168__jarredgibb__pig-chewing-2-96khz-1" },
            { Name.EatPredator2, "233168__jarredgibb__pig-chewing-2-96khz-2" },
            { Name.EatPredator3, "233168__jarredgibb__pig-chewing-2-96khz-3" },
            { Name.EatPredator4, "233168__jarredgibb__pig-chewing-2-96khz-4" },
            { Name.AngelChorus, "211659__taira-komori__angel-chorus2" },
            { Name.DropGlass, "114201__edgardedition__glassthud9" },
            { Name.Cough, "252231__reitanna__rough-grunt" },
            { Name.StepGhost, "608911__qwerty8339__pulse-noise-looping-9sec" },
            { Name.Pulse, "485076__inspectorj__heartbeat-regular-single-01-01-loop" },
            { Name.CryPlayerMale1, "413176__micahlg__male-hurt1" },
            { Name.CryPlayerMale2, "413178__micahlg__male-hurt2" },
            { Name.CryPlayerMale3, "413182__micahlg__male-hurt4" },
            { Name.CryPlayerMale4, "413183__micahlg__male-hurt7" },
            { Name.CryPlayerFemale1, "344004__reitanna__heavy-grunt" },
            { Name.CryPlayerFemale2, "252232__reitanna__weird-grunt" },
            { Name.CryPlayerFemale3, "242623__reitanna__grunt" },
            { Name.CryPlayerFemale4, "252235__reitanna__soft-grunt" },
            { Name.DeathPlayerMale, "554443__highpixel__male-death-sound" },
            { Name.DeathPlayerFemale, "343914__reitanna__seriously-exasperated" },
            { Name.EatPlayer1, "546322__micahlg__munchin-1" },
            { Name.EatPlayer2, "546322__micahlg__munchin-2" },
            { Name.EatPlayer3, "546322__micahlg__munchin-3" },
            { Name.EatPlayer4, "546322__micahlg__munchin-4" },
            { Name.EatHerbivore1, "365138__gibarroule__horse-eating-1" },
            { Name.EatHerbivore2, "365138__gibarroule__horse-eating-2" },
            { Name.EatHerbivore3, "365138__gibarroule__horse-eating-3" },
            { Name.EatHerbivore4, "365138__gibarroule__horse-eating-4" },
            { Name.EatHerbivore5, "365138__gibarroule__horse-eating-5" },
            { Name.Drink, "41529__jamius__potiondrinklong" },
            { Name.DestroySmallPlant1, "400641__lampeight__bush-weed-grass-cut-prune-move-1" },
            { Name.DestroySmallPlant2, "400641__lampeight__bush-weed-grass-cut-prune-move-2" },
            { Name.DestroySmallPlant3, "400641__lampeight__bush-weed-grass-cut-prune-move-3" },
            { Name.DestroySmallPlant4, "400641__lampeight__bush-weed-grass-cut-prune-move-4" },
            { Name.DestroySmallPlant5, "400641__lampeight__bush-weed-grass-cut-prune-move-5" },
            { Name.DestroySmallPlant6, "400641__lampeight__bush-weed-grass-cut-prune-move-6" },
            { Name.HitSmallPlant1, "488378__wobesound__harvest-sounds-1" },
            { Name.HitSmallPlant2, "488378__wobesound__harvest-sounds-2" },
            { Name.HitSmallPlant3, "488378__wobesound__harvest-sounds-3" },
            { Name.BowDraw, "490556__paveroux__bow-drawn" },
            { Name.BowRelease, "209399__samulis__bow-release" },
            { Name.ArrowHit, "178872__hanbaal__bow" },
            { Name.ArrowFly, "511490__lydmakeren__fx-bow-arrow" },
            { Name.DropArrow, "418530__14fpanskabubik-lukas__darts" },
            { Name.StartFireBig, "51450__robinhood76__00120-zapalka-1" },
            { Name.WaterSplash, "25819__freqman__splash-1" },
            { Name.EndFire, "544634__eminyildirim__fire-sizzle" },
            { Name.StartFireSmall, "524306__bertsz__match-strike" },
            { Name.Sawing, "566051__pagey1969__saw-wood-1" },
            { Name.Hammering, "406048__inspectorj__hammering-nails-close-a" },
            { Name.Planting, "384362__ali-6868__digging-a-hole-and-shovelling-soil-dirt-2" },
            { Name.MovingPlant, "185368__eelke__moving-plant" },
            { Name.Notification1, "342749__rhodesmas__notification-01" },
            { Name.Notification2, "352650__foolboymedia__piano-notification-4" },
            { Name.Notification3, "542035__rob-marion__gasp-ui-notification-4" },
            { Name.Notification4, "352651__foolboymedia__piano-notification-3" },
            { Name.DunDunDun, "45654__simon-lacelle__dun-dun-dun" },
            { Name.FryingPan, "571670__nachtmahrtv__frying-in-a-pan" },
            { Name.Cooking, "212610__taira-komori__boiling-opening" },
            { Name.HitMetal, "525167__sophia-c__metal-impact-4-soft" },
            { Name.DestroyMetal, "587442__samsterbirdies__dropping-sheet-metal-1" },
            { Name.DropIronPlate, "587442__samsterbirdies__dropping-sheet-metal-2" },
            { Name.DropMeat1, "481090__greenfiresound__meat-drop-1" },
            { Name.DropMeat2, "481090__greenfiresound__meat-drop-2" },
            { Name.DropMeat3, "481090__greenfiresound__meat-drop-3" },
            { Name.DropGeneric, "434782__stephenbist__luggage-drop-2" },
            { Name.DropStick, "414026__fractanimal__drop-stick-dropping-a-stick-on-concrete" },
            { Name.DropIronRod, "244990__vitaminl__metal-rods-drop" },
            { Name.DropIronBar, "213124__dkudos__iron-bar-drop-onto-wood-straw-floor" },
            { Name.DropPlant, "488393__wobesound__planting-sounds" },
            { Name.DropMud, "445109__breviceps__mud-splat" },
            { Name.DropSand, "547585__f-m-audio__grit-spray" },
            { Name.DropCrystal, "491677__bucketmandave__glass-breaking-due-to-impact-hard-sound-recorded-on-rode-shotgun-mini-processed-in-reaper" },
            { Name.SnoringFemale, "360423__blouhond__female-snoring" },
            { Name.SnoringMale, "538868__adrianoanjos__male-snore" },
            { Name.YawnFemale, "404383__eskimoneil__yawn-female" },
            { Name.YawnMale, "564231__arrowheadproductions__yawn-yawning" },
            { Name.StomachGrowl, "536870__nuncaconoci__stomach-growl" },
            { Name.Sprint, "607409__colorscrimsontears__upgrade" },
            { Name.StepLava, "237406__squareal__match-sizzle-03" },
            { Name.SeaWave1, "632517__thedutchmancreative__waves-1" },
            { Name.SeaWave2, "632517__thedutchmancreative__waves-2" },
            { Name.SeaWave3, "632517__thedutchmancreative__waves-3" },
            { Name.SeaWave4, "632517__thedutchmancreative__waves-4" },
            { Name.SeaWave5, "632517__thedutchmancreative__waves-5" },
            { Name.SeaWave6, "632517__thedutchmancreative__waves-6" },
            { Name.SeaWave7, "632517__thedutchmancreative__waves-7" },
            { Name.SeaWave8, "632517__thedutchmancreative__waves-8" },
            { Name.SeaWave9, "632517__thedutchmancreative__waves-9" },
            { Name.SeaWave10, "632517__thedutchmancreative__waves-10" },
            { Name.SeaWave11, "632517__thedutchmancreative__waves-11" },
            { Name.SeaWave12, "632517__thedutchmancreative__waves-12" },
            { Name.SeaWave13, "632517__thedutchmancreative__waves-13" },
            { Name.LakeWave1, "530699__szegvari__water-wave-beach-field-recording-200815-0036-1" },
            { Name.LakeWave2, "530699__szegvari__water-wave-beach-field-recording-200815-0036-2" },
            { Name.LakeWave3, "530699__szegvari__water-wave-beach-field-recording-200815-0036-3" },
            { Name.LakeWave4, "530699__szegvari__water-wave-beach-field-recording-200815-0036-4" },
            { Name.LakeWave5, "530699__szegvari__water-wave-beach-field-recording-200815-0036-5" },
            { Name.LakeWave6, "530699__szegvari__water-wave-beach-field-recording-200815-0036-6" },
            { Name.LakeWave7, "530699__szegvari__water-wave-beach-field-recording-200815-0036-7" },
            { Name.SeaWind, "465739__arnaud-coutancier__wind-waves-splashing" },
            { Name.NightCrickets, "60968__ifartinurgeneraldirection__night-sounds" },
            { Name.WeatherWind, "135193__felix-blume__wind-blowing-in-the-bush-on-the-top-of-kitt-peak-mountain" },
            { Name.PantFemale, "95567__kuroseishin__breathing-female" },
            { Name.PantMale, "489917__falcospizaetus__gasps02" },
            { Name.Cicadas1, "400096__harrietniamh__cicadas" },
            { Name.Cicadas2, "486628__nanisia__japanese-summer-cicadas" },
            { Name.Cicadas3, "321591__macdaddyno1__japanese-cicada" },
            { Name.PotLid, "210100__bowlingballout__pot-lid" },
            { Name.StoneMove1, "95007__j1987__stonegrind1" },
            { Name.StoneMove2, "572754__f-m-audio__scraping-and-clunking-large-stone-1" },
            { Name.FireBurst, "614724__newlocknew__fast-fire-flare-whoosh-1-6lrs" },
            { Name.HammerHits, "388062__arnaud-coutancier__quick-hammer-blows-on-anvil" },
            { Name.BoilingPotion, "62282__robinhood76__00517-alchemist-laboratory-arrangement" },
            { Name.ToolsMove, "517430__corlia__looking-through-metal-toolbox" },
            { Name.PaperMove1, "517732__zovipoland__book-page-change-1-1" },
            { Name.PaperMove2, "517732__zovipoland__book-page-change-1-2" },
            { Name.ClothRustle1, "427865__leonelmail__clothing-rustle-nylon_1" },
            { Name.ClothRustle2, "427865__leonelmail__clothing-rustle-nylon_2" },
            { Name.KnifeSharpen, "69480__aboe__knifesha4" },
            { Name.LeatherMove, "536087__eminyildirim__leather-movement" },
            { Name.DropRope, "459440__coral-island-studios__13-cardboard-box-stacking" },
            { Name.Error, "141334__lluiset7__error-2" },
            { Name.Lava, "174500__unfa__boiling-towel-loop" },
            { Name.TurnPage, "521114__d-jones__turn-the-page" },
            { Name.SplashMud, "447928__breviceps__muddy-boots" },
            { Name.Rain, "678435__borgory__soft-rain-in-forest-raindrops-fall-on-leaves" },
            { Name.Thunder1, "195439__littlebrojay__strike-3-sec" },
            { Name.Thunder2, "475094__josh74000mc__thunder3" },
            { Name.Thunder3, "334047__drukalo__badweatherrec" },
            { Name.Thunder4, "345920__dragisharambo21__thunder" },
            { Name.ElectricShock, "512471__michael_grinnell__electric-zap" },
            { Name.MetalicClank, "581592__samsterbirdies__saw-metal-bounce-1" },
            { Name.FireBurnShort, "195586__scott-snailham__bonfire-fx-with-whoosh-basic-with-some-crackle-cd-quality_short" },
            { Name.ShootFire, "234083__211redman112__lasgun-fire" },
            { Name.AnvilHit, "386124__ldezem__anvil-lokomo-a-100-kg-combination-of-hot-and-cold-hits" },
            { Name.BoilingPotionLoop, "62282__robinhood76__00517-alchemist-laboratory-arrangement_loop" },
            { Name.DropLeather, "203086__mafon2__two-thumps" },
            { Name.HitLeather, "144266__dwoboyle__impact-leatherbelt-001" },
            { Name.DestroyLeather, "471745__juanfg__break" },
            { Name.Jump, "380471__acebrian__jump" },
            { Name.SteamHit, "90143__pengo_au__steam_burst" },
            { Name.DropPlastic, "211171__sdlx__plastic-lid-snap" },
            { Name.TrumpetChime, "AudioGen_trumpet_chime" },
            { Name.CreepyCave, "442956__qubodup__creepy-cave" },
            { Name.Rattle, "475827__zeffon__rattle-01" },
            { Name.BonusItem, "625174__gabfitzgerald__ui-sound-approval-high-pitched-bell-synth" },
            { Name.Whoosh, "351256__kinoton__deep-whoosh-1" },
            { Name.Glug, "221488__lloydevans09__glug" },
            { Name.WaterDrop, "349873__matrixxx__water-drop-02" },
            { Name.StonesFalling, "567251__iwanplays__stones-falling" },
            { Name.WoodCreak, "610886__jakobgille__doorcreak-openclose_tur-holzschrank-10_ioka_none_hofefeld" },
            { Name.OpenSeaStormLoop, "441008__girlwithsoundrecorder__rough-baltic-sea_loop" },
            { Name.OpenSeaStormCrash, "441008__girlwithsoundrecorder__rough-baltic-sea_crash" },
            { Name.BoatHorn, "441223__devy32__boat-horn" },
            { Name.MusicBoxEnding, "72650__lg__music-box-01" },
            };
    }
}
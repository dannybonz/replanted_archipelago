GEN_VERSION = "1.4" #Version to match mod

LEVEL_TYPE_NAMES = {"adventure": "Adventure", "minigame": "Mini-game", "puzzle": "Puzzle", "survival": "Survival", "bonus": "Bonus Levels", "cloudy": "Cloudy Day"}

LEVEL_LOCATIONS = {
    "Day":  {"at_night": False, "has_pool": False, "on_roof": False},
    "Night": {"at_night": True,  "has_pool": False, "on_roof": False},
    "Pool": {"at_night": False, "has_pool": True,  "on_roof": False},
    "Fog":  {"at_night": True,  "has_pool": True,  "on_roof": False},
    "Roof": {"at_night": False, "has_pool": False, "on_roof": True},
}

LEVELS = {
    "1-1": {
        "name": "Day: Level 1-1",
        "flags": 0,
        "location": "Day",
        "zombies": [
            "Normal",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1000
    },
    "1-2": {
        "name": "Day: Level 1-2",
        "flags": 1,
        "location": "Day",
        "zombies": [
            "Normal",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1001
    },
    "1-3": {
        "name": "Day: Level 1-3",
        "flags": 1,
        "location": "Day",
        "zombies": [
            "Normal",
            "Conehead",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1002
    },
    "1-4": {
        "name": "Day: Level 1-4",
        "flags": 1,
        "location": "Day",
        "zombies": [
            "Normal",
            "Conehead",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1003
    },
    "1-5": {
        "name": "Day: Level 1-5",
        "flags": 1,
        "location": "Day",
        "zombies": [
            "Normal",
            "Conehead",
            "Flag"
        ],
        "choose": False,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1004
    },
    "1-6": {
        "name": "Day: Level 1-6",
        "flags": 1,
        "location": "Day",
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1005
    },
    "1-7": {
        "name": "Day: Level 1-7",
        "flags": 2,
        "location": "Day",
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2000
        ],
        "clear_location_id": 1006
    },
    "1-8": {
        "name": "Day: Level 1-8",
        "flags": 1,
        "location": "Day",
        "zombies": [
            "Normal",
            "Conehead",
            "Buckethead",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1007
    },
    "1-9": {
        "name": "Day: Level 1-9",
        "flags": 2,
        "location": "Day",
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2001
        ],
        "clear_location_id": 1008
    },
    "1-10": {
        "name": "Day: Level 1-10",
        "flags": 2,
        "location": "Day",
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Flag"
        ],
        "choose": False,
        "type": "adventure",
        "flag_location_ids": [
            2002
        ],
        "clear_location_id": 1009
    },
    "2-1": {
        "name": "Night: Level 2-1",
        "flags": 1,
        "location": "Night",
        "zombies": [
            "Normal",
            "Newspaper",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1010
    },
    "2-2": {
        "name": "Night: Level 2-2",
        "flags": 2,
        "location": "Night",
        "zombies": [
            "Normal",
            "Conehead",
            "Buckethead",
            "Newspaper",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2003
        ],
        "clear_location_id": 1011
    },
    "2-3": {
        "name": "Night: Level 2-3",
        "flags": 1,
        "location": "Night",
        "zombies": [
            "Normal",
            "Conehead",
            "ScreenDoor",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1012
    },
    "2-4": {
        "name": "Night: Level 2-4",
        "flags": 2,
        "location": "Night",
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "ScreenDoor",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2004
        ],
        "clear_location_id": 1013
    },
    "2-5": {
        "name": "Night: Level 2-5",
        "flags": 0,
        "location": "Night",
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "Flag"
        ],
        "choose": False,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1014
    },
    "2-6": {
        "name": "Night: Level 2-6",
        "flags": 1,
        "location": "Night",
        "zombies": [
            "Normal",
            "Conehead",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1015
    },
    "2-7": {
        "name": "Night: Level 2-7",
        "flags": 2,
        "location": "Night",
        "zombies": [
            "Normal",
            "Conehead",
            "ScreenDoor",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2005
        ],
        "clear_location_id": 1016
    },
    "2-8": {
        "name": "Night: Level 2-8",
        "flags": 1,
        "location": "Night",
        "zombies": [
            "Normal",
            "Conehead",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1017
    },
    "2-9": {
        "name": "Night: Level 2-9",
        "flags": 2,
        "location": "Night",
        "zombies": [
            "Normal",
            "Conehead",
            "ScreenDoor",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2006
        ],
        "clear_location_id": 1018
    },
    "2-10": {
        "name": "Night: Level 2-10",
        "flags": 2,
        "location": "Night",
        "zombies": [
            "Normal",
            "Conehead",
            "ScreenDoor",
            "Flag"
        ],
        "choose": False,
        "type": "adventure",
        "flag_location_ids": [
            2007
        ],
        "clear_location_id": 1019
    },
    "3-1": {
        "name": "Pool: Level 3-1",
        "flags": 1,
        "location": "Pool",
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1020
    },
    "3-2": {
        "name": "Pool: Level 3-2",
        "flags": 2,
        "location": "Pool",
        "zombies": [
            "Normal",
            "Conehead",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2008
        ],
        "clear_location_id": 1021
    },
    "3-3": {
        "name": "Pool: Level 3-3",
        "flags": 2,
        "location": "Pool",
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "Snorkel",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2009
        ],
        "clear_location_id": 1022
    },
    "3-4": {
        "name": "Pool: Level 3-4",
        "flags": 3,
        "location": "Pool",
        "zombies": [
            "Normal",
            "Conehead",
            "Snorkel",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2010,
            2011
        ],
        "clear_location_id": 1023
    },
    "3-5": {
        "name": "Pool: Level 3-5",
        "flags": 2,
        "location": "Pool",
        "zombies": [
            "Normal",
            "Conehead",
            "Snorkel",
            "Flag"
        ],
        "choose": False,
        "type": "adventure",
        "flag_location_ids": [
            2012
        ],
        "clear_location_id": 1024
    },
    "3-6": {
        "name": "Pool: Level 3-6",
        "flags": 2,
        "location": "Pool",
        "zombies": [
            "Normal",
            "Conehead",
            "Football",
            "Zomboni",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2013
        ],
        "clear_location_id": 1025
    },
    "3-7": {
        "name": "Pool: Level 3-7",
        "flags": 3,
        "location": "Pool",
        "zombies": [
            "Normal",
            "Conehead",
            "Buckethead",
            "Football",
            "Snorkel",
            "Zomboni",
            "DolphinRider",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2014,
            2015
        ],
        "clear_location_id": 1026
    },
    "3-8": {
        "name": "Pool: Level 3-8",
        "flags": 2,
        "location": "Pool",
        "zombies": [
            "Normal",
            "Conehead",
            "Dancer",
            "DolphinRider",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2016
        ],
        "clear_location_id": 1027
    },
    "3-9": {
        "name": "Pool: Level 3-9",
        "flags": 3,
        "location": "Pool",
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Dancer",
            "Zomboni",
            "DolphinRider",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2017,
            2018
        ],
        "clear_location_id": 1028
    },
    "3-10": {
        "name": "Pool: Level 3-10",
        "flags": 3,
        "location": "Pool",
        "zombies": [
            "Normal",
            "Conehead",
            "Buckethead",
            "Football",
            "Dancer",
            "Snorkel",
            "Zomboni",
            "Flag"
        ],
        "choose": False,
        "type": "adventure",
        "flag_location_ids": [
            2019,
            2020
        ],
        "clear_location_id": 1029
    },
    "4-1": {
        "name": "Fog: Level 4-1",
        "flags": 1,
        "location": "Fog",
        "zombies": [
            "Normal",
            "Conehead",
            "Football",
            "JackInTheBox",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1030
    },
    "4-2": {
        "name": "Fog: Level 4-2",
        "flags": 2,
        "location": "Fog",
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Football",
            "JackInTheBox",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2021
        ],
        "clear_location_id": 1031
    },
    "4-3": {
        "name": "Fog: Level 4-3",
        "flags": 1,
        "location": "Fog",
        "zombies": [
            "Normal",
            "Conehead",
            "Football",
            "Balloon",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1032
    },
    "4-4": {
        "name": "Fog: Level 4-4",
        "flags": 2,
        "location": "Fog",
        "zombies": [
            "Normal",
            "Conehead",
            "DolphinRider",
            "Balloon",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2022
        ],
        "clear_location_id": 1033
    },
    "4-5": {
        "name": "Fog: Level 4-5",
        "flags": 0,
        "location": "Fog",
        "zombies": [
            "Normal",
            "Conehead",
            "Flag"
        ],
        "choose": False,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1034
    },
    "4-6": {
        "name": "Fog: Level 4-6",
        "flags": 1,
        "location": "Fog",
        "zombies": [
            "Normal",
            "Conehead",
            "Digger",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1035
    },
    "4-7": {
        "name": "Fog: Level 4-7",
        "flags": 2,
        "location": "Fog",
        "zombies": [
            "Normal",
            "Conehead",
            "Buckethead",
            "JackInTheBox",
            "Digger",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2023
        ],
        "clear_location_id": 1036
    },
    "4-8": {
        "name": "Fog: Level 4-8",
        "flags": 1,
        "location": "Fog",
        "zombies": [
            "Normal",
            "Conehead",
            "Pogo",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1037
    },
    "4-9": {
        "name": "Fog: Level 4-9",
        "flags": 2,
        "location": "Fog",
        "zombies": [
            "Normal",
            "Conehead",
            "Buckethead",
            "Balloon",
            "Pogo",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2024
        ],
        "clear_location_id": 1038
    },
    "4-10": {
        "name": "Fog: Level 4-10",
        "flags": 2,
        "location": "Fog",
        "zombies": [
            "Normal",
            "Conehead",
            "Buckethead",
            "JackInTheBox",
            "Balloon",
            "Digger",
            "Pogo",
            "Flag"
        ],
        "choose": False,
        "type": "adventure",
        "flag_location_ids": [
            2025
        ],
        "clear_location_id": 1039
    },
    "5-1": {
        "name": "Roof: Level 5-1",
        "flags": 1,
        "location": "Roof",
        "zombies": [
            "Normal",
            "Conehead",
            "Football",
            "Bungee",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1040
    },
    "5-2": {
        "name": "Roof: Level 5-2",
        "flags": 2,
        "location": "Roof",
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Bungee",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2026
        ],
        "clear_location_id": 1041
    },
    "5-3": {
        "name": "Roof: Level 5-3",
        "flags": 2,
        "location": "Roof",
        "zombies": [
            "Normal",
            "Conehead",
            "Ladder",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2027
        ],
        "clear_location_id": 1042
    },
    "5-4": {
        "name": "Roof: Level 5-4",
        "flags": 3,
        "location": "Roof",
        "zombies": [
            "Normal",
            "Conehead",
            "Pogo",
            "Ladder",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2028,
            2029
        ],
        "clear_location_id": 1043
    },
    "5-5": {
        "name": "Roof: Level 5-5",
        "flags": 2,
        "location": "Roof",
        "zombies": [
            "Normal",
            "Conehead",
            "Buckethead",
            "Ladder",
            "Flag"
        ],
        "choose": False,
        "type": "adventure",
        "flag_location_ids": [
            2030
        ],
        "clear_location_id": 1044
    },
    "5-6": {
        "name": "Roof: Level 5-6",
        "flags": 2,
        "location": "Roof",
        "zombies": [
            "Normal",
            "Conehead",
            "Catapult",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2031
        ],
        "clear_location_id": 1045
    },
    "5-7": {
        "name": "Roof: Level 5-7",
        "flags": 3,
        "location": "Roof",
        "zombies": [
            "Normal",
            "Conehead",
            "Bungee",
            "Ladder",
            "Catapult",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2032,
            2033
        ],
        "clear_location_id": 1046
    },
    "5-8": {
        "name": "Roof: Level 5-8",
        "flags": 2,
        "location": "Roof",
        "zombies": [
            "Normal",
            "Conehead",
            "Gargantuar",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2034
        ],
        "clear_location_id": 1047
    },
    "5-9": {
        "name": "Roof: Level 5-9",
        "flags": 3,
        "location": "Roof",
        "zombies": [
            "Normal",
            "Conehead",
            "Buckethead",
            "JackInTheBox",
            "Bungee",
            "Ladder",
            "Catapult",
            "Gargantuar",
            "Flag"
        ],
        "choose": True,
        "type": "adventure",
        "flag_location_ids": [
            2035,
            2036
        ],
        "clear_location_id": 1048
    },
    "5-10": {
        "name": "Roof: Dr. Zomboss",
        "flags": 0,
        "location": "Roof",
        "zombies": [
            "Normal",
            "Conehead",
            "Buckethead",
            "JackInTheBox",
            "Bungee",
            "Ladder",
            "Catapult",
            "Gargantuar"
        ],
        "choose": False,
        "type": "adventure",
        "flag_location_ids": [],
        "clear_location_id": 1049
    },
    "ChallengeWarAndPeas": {
        "zombies": [
            "PeaHead",
            "WallnutHead"
        ],
        "name": "Mini-games: ZomBotany",
        "location": "Day",
        "choose": True,
        "flags": 2,
        "type": "minigame",
        "flag_location_ids": [
            2037
        ],
        "clear_location_id": 1050
    },
    "ChallengeWallnutBowling": {
        "zombies": [
            "Newspaper",
            "Normal",
            "Buckethead",
            "Polevaulter",
            "Conehead"
        ],
        "name": "Mini-games: Wall-nut Bowling",
        "location": "Day",
        "choose": False,
        "flags": 2,
        "type": "minigame",
        "flag_location_ids": [
            2038
        ],
        "clear_location_id": 1051
    },
    "ChallengeSlotMachine": {
        "zombies": [
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Mini-games: Slot Machine",
        "location": "Day",
        "choose": False,
        "flags": 0,
        "type": "minigame",
        "flag_location_ids": [],
        "clear_location_id": 1052
    },
    "ChallengeRainingSeeds": {
        "zombies": [
            "Bungee",
            "ScreenDoor",
            "Football",
            "JackInTheBox",
            "Newspaper",
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Mini-games: It's Raining Seeds",
        "location": "Fog",
        "choose": False,
        "flags": 4,
        "type": "minigame",
        "flag_location_ids": [
            2039,
            2040,
            2041
        ],
        "clear_location_id": 1053
    },
    "ChallengeBeghouled": {
        "zombies": [
            "ScreenDoor",
            "Football",
            "Newspaper",
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Mini-games: Beghouled",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "minigame",
        "flag_location_ids": [],
        "clear_location_id": 1054
    },
    "ChallengeInvisighoul": {
        "zombies": [
            "DolphinRider",
            "JackInTheBox",
            "Normal",
            "Buckethead",
            "Conehead",
            "Zomboni"
        ],
        "name": "Mini-games: Invisi-ghoul",
        "location": "Fog",
        "choose": False,
        "flags": 2,
        "type": "minigame",
        "flag_location_ids": [
            2042
        ],
        "clear_location_id": 1055
    },
    "ChallengeSeeingStars": {
        "zombies": [
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Mini-games: Seeing Stars",
        "location": "Day",
        "choose": True,
        "flags": 0,
        "type": "minigame",
        "flag_location_ids": [],
        "clear_location_id": 1056
    },
    "ChallengeZombiquarium": {
        "zombies": [],
        "name": "Mini-games: Zombiquarium",
        "location": "Day",
        "choose": False,
        "flags": 0,
        "type": "minigame",
        "flag_location_ids": [],
        "clear_location_id": 1057
    },
    "ChallengeBeghouledTwist": {
        "zombies": [
            "ScreenDoor",
            "Football",
            "Newspaper",
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Mini-games: Beghouled Twist",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "minigame",
        "flag_location_ids": [],
        "clear_location_id": 1058
    },
    "ChallengeLittleTrouble": {
        "zombies": [
            "Football",
            "Normal",
            "Snorkel",
            "Conehead"
        ],
        "name": "Mini-games: Big Trouble Little Zombie",
        "location": "Pool",
        "choose": False,
        "flags": 3,
        "type": "minigame",
        "flag_location_ids": [
            2043,
            2044
        ],
        "clear_location_id": 1059
    },
    "ChallengePortalCombat": {
        "zombies": [
            "Balloon",
            "Football",
            "Normal",
            "Buckethead"
        ],
        "name": "Mini-games: Portal Combat",
        "location": "Night",
        "choose": False,
        "flags": 2,
        "type": "minigame",
        "flag_location_ids": [
            2045
        ],
        "clear_location_id": 1060
    },
    "ChallengeColumn": {
        "zombies": [
            "Football",
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Mini-games: Column Like You See 'Em",
        "location": "Roof",
        "choose": False,
        "flags": 3,
        "type": "minigame",
        "flag_location_ids": [
            2046,
            2047
        ],
        "clear_location_id": 1061
    },
    "ChallengeBobsledBonanza": {
        "zombies": [
            "Zomboni"
        ],
        "name": "Mini-games: Bobsled Bonanza",
        "location": "Pool",
        "choose": True,
        "flags": 4,
        "type": "minigame",
        "flag_location_ids": [
            2048,
            2049,
            2050
        ],
        "clear_location_id": 1062
    },
    "ChallengeSpeed": {
        "zombies": [
            "DolphinRider",
            "Normal",
            "Polevaulter",
            "Conehead"
        ],
        "name": "Mini-games: Zombie Nimble Zombie Quick",
        "location": "Pool",
        "choose": True,
        "flags": 4,
        "type": "minigame",
        "flag_location_ids": [
            2051,
            2052,
            2053
        ],
        "clear_location_id": 1063
    },
    "ChallengeWhackAZombie": {
        "zombies": [
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Mini-games: Whack a Zombie",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "minigame",
        "flag_location_ids": [],
        "clear_location_id": 1064
    },
    "ChallengeLastStand": {
        "zombies": [
            "DolphinRider",
            "Football",
            "JackInTheBox",
            "Ladder",
            "Newspaper",
            "Normal",
            "Buckethead",
            "Polevaulter",
            "Conehead"
        ],
        "name": "Mini-games: Last Stand",
        "location": "Pool",
        "choose": True,
        "flags": 5,
        "type": "minigame",
        "flag_location_ids": [
            2054,
            2055,
            2056,
            2057
        ],
        "clear_location_id": 1065
    },
    "ChallengeWarAndPeas2": {
        "zombies": [
            "GatlingHead",
            "JalapenoHead",
            "PeaHead",
            "SquashHead",
            "TallnutHead",
            "WallnutHead"
        ],
        "name": "Mini-games: ZomBotany 2",
        "location": "Pool",
        "choose": True,
        "flags": 3,
        "type": "minigame",
        "flag_location_ids": [
            2058,
            2059
        ],
        "clear_location_id": 1066
    },
    "ChallengeWallnutBowling2": {
        "zombies": [
            "Dancer",
            "ScreenDoor",
            "Newspaper",
            "Normal",
            "Buckethead",
            "Polevaulter",
            "Conehead"
        ],
        "name": "Mini-games: Wall-nut Bowling 2",
        "location": "Day",
        "choose": False,
        "flags": 3,
        "type": "minigame",
        "flag_location_ids": [
            2060,
            2061
        ],
        "clear_location_id": 1067
    },
    "ChallengePogoParty": {
        "zombies": [
            "Pogo"
        ],
        "name": "Mini-games: Pogo Party",
        "location": "Roof",
        "choose": True,
        "flags": 3,
        "type": "minigame",
        "flag_location_ids": [
            2062,
            2063
        ],
        "clear_location_id": 1068
    },
    "ChallengeFinalBoss": {
        "zombies": [],
        "name": "Mini-games: Dr. Zomboss's Revenge",
        "location": "Roof",
        "choose": False,
        "flags": 0,
        "type": "minigame",
        "flag_location_ids": [],
        "clear_location_id": 1069
    },
    "ScaryPotter1": {
        "zombies": [],
        "name": "Puzzle: Vasebreaker",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1070
    },
    "ScaryPotter2": {
        "zombies": [],
        "name": "Puzzle: To The Left",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1071
    },
    "ScaryPotter3": {
        "zombies": [],
        "name": "Puzzle: Third Vase",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1072
    },
    "ScaryPotter4": {
        "zombies": [],
        "name": "Puzzle: Chain Reaction",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1073
    },
    "ScaryPotter5": {
        "zombies": [],
        "name": "Puzzle: M is for Metal",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1074
    },
    "ScaryPotter6": {
        "zombies": [],
        "name": "Puzzle: Scary Potter",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1075
    },
    "ScaryPotter7": {
        "zombies": [],
        "name": "Puzzle: Hokey Pokey",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1076
    },
    "ScaryPotter8": {
        "zombies": [],
        "name": "Puzzle: Another Chain Reaction",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1077
    },
    "ScaryPotter9": {
        "zombies": [],
        "name": "Puzzle: Ace of Vase",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1078
    },
    "PuzzleIZombie1": {
        "zombies": [],
        "name": "Puzzle: I, Zombie",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1079
    },
    "PuzzleIZombie2": {
        "zombies": [],
        "name": "Puzzle: I, Zombie Too",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1080
    },
    "PuzzleIZombie3": {
        "zombies": [],
        "name": "Puzzle: Can You Dig It?",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1081
    },
    "PuzzleIZombie4": {
        "zombies": [],
        "name": "Puzzle: Totally Nuts",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1082
    },
    "PuzzleIZombie5": {
        "zombies": [],
        "name": "Puzzle: Dead Zeppelin",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1083
    },
    "PuzzleIZombie6": {
        "zombies": [],
        "name": "Puzzle: Me Smash!",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1084
    },
    "PuzzleIZombie7": {
        "zombies": [],
        "name": "Puzzle: ZomBoogie",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1085
    },
    "PuzzleIZombie8": {
        "zombies": [],
        "name": "Puzzle: Three Hit Wonder",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1086
    },
    "PuzzleIZombie9": {
        "zombies": [],
        "name": "Puzzle: All your brainz r belong to us",
        "location": "Night",
        "choose": False,
        "flags": 0,
        "type": "puzzle",
        "flag_location_ids": [],
        "clear_location_id": 1087
    },
    "SurvivalNormalStage1": {
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "ScreenDoor",
            "Football",
            "Dancer"
        ],
        "name": "Survival: Day",
        "location": "Day",
        "choose": True,
        "flags": 5,
        "type": "survival",
        "flag_location_ids": [
            2064,
            2065,
            2066,
            2067
        ],
        "clear_location_id": 1088
    },
    "SurvivalNormalStage2": {
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "ScreenDoor",
            "Football",
            "Dancer"
        ],
        "name": "Survival: Night",
        "location": "Night",
        "choose": True,
        "flags": 5,
        "type": "survival",
        "flag_location_ids": [
            2068,
            2069,
            2070,
            2071
        ],
        "clear_location_id": 1089
    },
    "SurvivalNormalStage3": {
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "ScreenDoor",
            "Football",
            "Dancer",
            "Snorkel"
        ],
        "name": "Survival: Pool",
        "location": "Pool",
        "choose": True,
        "flags": 5,
        "type": "survival",
        "flag_location_ids": [
            2072,
            2073,
            2074,
            2075
        ],
        "clear_location_id": 1090
    },
    "SurvivalNormalStage4": {
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "ScreenDoor",
            "Football",
            "Dancer",
            "Snorkel",
            "Zomboni",
            "DolphinRider",
            "JackInTheBox",
            "Balloon",
            "Digger",
            "Pogo",
            "Ladder",
            "Catapult"
        ],
        "name": "Survival: Fog",
        "location": "Fog",
        "choose": True,
        "flags": 5,
        "type": "survival",
        "flag_location_ids": [
            2076,
            2077,
            2078,
            2079
        ],
        "clear_location_id": 1091
    },
    "SurvivalNormalStage5": {
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "ScreenDoor",
            "Football",
            "Zomboni",
            "JackInTheBox",
            "Balloon",
            "Pogo",
            "Bungee",
            "Ladder",
            "Catapult"
        ],
        "name": "Survival: Roof",
        "location": "Roof",
        "choose": True,
        "flags": 5,
        "type": "survival",
        "flag_location_ids": [
            2080,
            2081,
            2082,
            2083
        ],
        "clear_location_id": 1092
    },
    "SurvivalHardStage1": {
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "ScreenDoor",
            "Football",
            "Dancer",
            "Zomboni",
            "JackInTheBox",
            "Balloon",
            "Digger",
            "Pogo",
            "Ladder",
            "Catapult",
            "Gargantuar"
        ],
        "name": "Survival: Day (Hard)",
        "location": "Day",
        "choose": True,
        "flags": 10,
        "type": "survival",
        "flag_location_ids": [
            2084,
            2085,
            2086,
            2087,
            2088,
            2089,
            2090,
            2091,
            2092
        ],
        "clear_location_id": 1093
    },
    "SurvivalHardStage2": {
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "ScreenDoor",
            "Football",
            "Dancer",
            "JackInTheBox",
            "Balloon",
            "Digger",
            "Pogo",
            "Ladder",
            "Catapult",
            "Gargantuar"
        ],
        "name": "Survival: Night (Hard)",
        "location": "Night",
        "choose": True,
        "flags": 10,
        "type": "survival",
        "flag_location_ids": [
            2093,
            2094,
            2095,
            2096,
            2097,
            2098,
            2099,
            2100,
            2101
        ],
        "clear_location_id": 1094
    },
    "SurvivalHardStage3": {
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "ScreenDoor",
            "Football",
            "Dancer",
            "Snorkel",
            "Zomboni",
            "DolphinRider",
            "JackInTheBox",
            "Balloon",
            "Digger",
            "Pogo",
            "Ladder",
            "Catapult",
            "Gargantuar"
        ],
        "name": "Survival: Pool (Hard)",
        "location": "Pool",
        "choose": True,
        "flags": 10,
        "type": "survival",
        "flag_location_ids": [
            2102,
            2103,
            2104,
            2105,
            2106,
            2107,
            2108,
            2109,
            2110
        ],
        "clear_location_id": 1095
    },
    "SurvivalHardStage4": {
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "ScreenDoor",
            "Football",
            "Dancer",
            "Snorkel",
            "Zomboni",
            "DolphinRider",
            "JackInTheBox",
            "Balloon",
            "Digger",
            "Pogo",
            "Ladder",
            "Catapult",
            "Gargantuar"
        ],
        "name": "Survival: Fog (Hard)",
        "location": "Fog",
        "choose": True,
        "flags": 10,
        "type": "survival",
        "flag_location_ids": [
            2111,
            2112,
            2113,
            2114,
            2115,
            2116,
            2117,
            2118,
            2119
        ],
        "clear_location_id": 1096
    },
    "SurvivalHardStage5": {
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
            "Buckethead",
            "Newspaper",
            "ScreenDoor",
            "Football",
            "Zomboni",
            "JackInTheBox",
            "Balloon",
            "Pogo",
            "Bungee",
            "Ladder",
            "Catapult",
            "Gargantuar"
        ],
        "name": "Survival: Roof (Hard)",
        "location": "Roof",
        "choose": True,
        "flags": 10,
        "type": "survival",
        "flag_location_ids": [
            2120,
            2121,
            2122,
            2123,
            2124,
            2125,
            2126,
            2127,
            2128
        ],
        "clear_location_id": 1097
    },
    "ChallengeArtChallenge1": {
        "zombies": [
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Bonus Levels: Art Challenge Wall-nut",
        "location": "Day",
        "choose": True,
        "flags": 0,
        "type": "bonus",
        "flag_location_ids": [],
        "clear_location_id": 1098
    },
    "ChallengeSunnyDay": {
        "zombies": [
            "Football",
            "JackInTheBox",
            "Normal",
            "Buckethead",
            "Polevaulter",
            "Conehead"
        ],
        "name": "Bonus Levels: Sunny Day",
        "location": "Day",
        "choose": True,
        "flags": 4,
        "type": "bonus",
        "flag_location_ids": [
            2129,
            2130,
            2131
        ],
        "clear_location_id": 1099
    },
    "ChallengeResodded": {
        "zombies": [
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Bonus Levels: Unsodded",
        "location": "Day",
        "choose": True,
        "flags": 4,
        "type": "bonus",
        "flag_location_ids": [
            2132,
            2133,
            2134
        ],
        "clear_location_id": 1100
    },
    "ChallengeBigTime": {
        "zombies": [
            "ScreenDoor",
            "Football",
            "JackInTheBox",
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Bonus Levels: Big Time",
        "location": "Day",
        "choose": True,
        "flags": 4,
        "type": "bonus",
        "flag_location_ids": [
            2135,
            2136,
            2137
        ],
        "clear_location_id": 1101
    },
    "ChallengeArtChallenge2": {
        "zombies": [
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Bonus Levels: Art Challenge Sunflower",
        "location": "Day",
        "choose": True,
        "flags": 0,
        "type": "bonus",
        "flag_location_ids": [],
        "clear_location_id": 1102
    },
    "ChallengeAirRaid": {
        "zombies": [
            "Balloon"
        ],
        "name": "Bonus Levels: Air Raid",
        "location": "Fog",
        "choose": True,
        "flags": 2,
        "type": "bonus",
        "flag_location_ids": [
            2138
        ],
        "clear_location_id": 1103
    },
    "ChallengeHighGravity": {
        "zombies": [
            "Balloon",
            "ScreenDoor",
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Bonus Levels: High Gravity",
        "location": "Roof",
        "choose": True,
        "flags": 2,
        "type": "bonus",
        "flag_location_ids": [
            2139
        ],
        "clear_location_id": 1104
    },
    "ChallengeGraveDanger": {
        "zombies": [
            "Normal",
            "Buckethead",
            "Conehead"
        ],
        "name": "Bonus Levels: Grave Danger",
        "location": "Night",
        "choose": True,
        "flags": 2,
        "type": "bonus",
        "flag_location_ids": [
            2140
        ],
        "clear_location_id": 1105
    },
    "ChallengeShovel": {
        "zombies": [
            "Normal",
            "Conehead"
        ],
        "name": "Bonus Levels: Can You Dig It?",
        "location": "Day",
        "choose": False,
        "flags": 3,
        "type": "bonus",
        "flag_location_ids": [
            2141,
            2142
        ],
        "clear_location_id": 1106
    },
    "ChallengeStormyNight": {
        "zombies": [
            "Balloon",
            "DolphinRider",
            "Normal",
            "Conehead"
        ],
        "name": "Bonus Levels: Dark Stormy Night",
        "location": "Fog",
        "choose": False,
        "flags": 3,
        "type": "bonus",
        "flag_location_ids": [
            2143,
            2144
        ],
        "clear_location_id": 1107
    },
    "CloudyDay1": {
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
        ],
        "name": "Cloudy Day: Level 1",
        "location": "Day",
        "choose": True,
        "flags": 1,
        "type": "cloudy",
        "flag_location_ids": [],
        "clear_location_id": 1108
    },
    "CloudyDay2": {
        "zombies": [
            "Normal",
            "Conehead",
            "Polevaulter",
        ],
        "name": "Cloudy Day: Level 2",
        "location": "Day",
        "choose": True,
        "flags": 2,
        "type": "cloudy",
        "flag_location_ids": [
            2145
        ],
        "clear_location_id": 1109
    },
    "CloudyDay3": {
        "zombies": [
            "Normal",
            "Conehead",
            "Buckethead",
        ],
        "name": "Cloudy Day: Level 3",
        "location": "Day",
        "choose": True,
        "flags": 1,
        "type": "cloudy",
        "flag_location_ids": [],
        "clear_location_id": 1110
    },
    "CloudyDay4": {
        "zombies": [
            "Normal",
            "Conehead",
            "Buckethead",
            "Polevaulter"
        ],
        "name": "Cloudy Day: Level 4",
        "location": "Day",
        "choose": True,
        "flags": 3,
        "type": "cloudy",
        "flag_location_ids": [
            2146,
            2147
        ],
        "clear_location_id": 1111
    },
    "CloudyDay5": {
        "zombies": [
            "Normal",
            "Conehead",
            "Zomboni"
        ],
        "name": "Cloudy Day: Level 5",
        "location": "Pool",
        "choose": True,
        "flags": 2,
        "type": "cloudy",
        "flag_location_ids": [
            2148
        ],
        "clear_location_id": 1112
    },
    "CloudyDay6": {
        "zombies": [
            "Normal",
            "Conehead",
            "Zomboni",
            "Buckethead",
            "Snorkel"
        ],
        "name": "Cloudy Day: Level 6",
        "location": "Pool",
        "choose": True,
        "flags": 3,
        "type": "cloudy",
        "flag_location_ids": [
            2149,
            2150
        ],
        "clear_location_id": 1113
    },
    "CloudyDay7": {
        "zombies": [
            "Normal",
            "Conehead",
            "DolphinRider",
        ],
        "name": "Cloudy Day: Level 7",
        "location": "Pool",
        "choose": True,
        "flags": 2,
        "type": "cloudy",
        "flag_location_ids": [
            2151
        ],
        "clear_location_id": 1114
    },    
    "CloudyDay8": {
        "zombies": [
            "Normal",
            "Conehead",
            "DolphinRider",
            "Zomboni",
            "Polevaulter",
            "Buckethead"
        ],
        "name": "Cloudy Day: Level 8",
        "location": "Pool",
        "choose": True,
        "flags": 3,
        "type": "cloudy",
        "flag_location_ids": [
            2152,
            2153
        ],
        "clear_location_id": 1115
    },    
    "CloudyDay9": {
        "zombies": [
            "Normal",
            "Conehead",
            "Catapult"
        ],
        "name": "Cloudy Day: Level 9",
        "location": "Roof",
        "choose": True,
        "flags": 2,
        "type": "cloudy",
        "flag_location_ids": [
            2154
        ],
        "clear_location_id": 1116
    },   
    "CloudyDay10": {
        "zombies": [
            "Normal",
            "Conehead",
            "Catapult",
            "Bungee",
            "Ladder"
        ],
        "name": "Cloudy Day: Level 10",
        "location": "Roof",
        "choose": True,
        "flags": 3,
        "type": "cloudy",
        "flag_location_ids": [
            2155,
            2156
        ],
        "clear_location_id": 1117
    },  
    "CloudyDay11": {
        "zombies": [
            "Normal",
            "Conehead",
            "Gargantuar"
        ],
        "name": "Cloudy Day: Level 11",
        "location": "Roof",
        "choose": True,
        "flags": 2,
        "type": "cloudy",
        "flag_location_ids": [
            2157
        ],
        "clear_location_id": 1118
    }, 
    "CloudyDay12": {
        "zombies": [
            "Normal",
            "Conehead",
            "Gargantuar",
            "Bungee",
            "Catapult",
            "Ladder",
            "JackInTheBox",
            "Buckethead"
        ],
        "name": "Cloudy Day: Level 12",
        "location": "Roof",
        "choose": True,
        "flags": 3,
        "type": "cloudy",
        "flag_location_ids": [
            2158,
            2159
        ],
        "clear_location_id": 1119
    }, 
}

level_id = 1 
for level in LEVELS:
    if LEVELS[level]["type"] == "adventure":
        type_name = LEVELS[level]["location"]
    else:
        type_name = LEVEL_TYPE_NAMES[LEVELS[level]["type"]]
    LEVELS[level]["unlock_item_name"] = f"{type_name} Unlock: {(LEVELS[level]["name"].split(": ")[-1])}"
    LEVELS[level]["id"] = level_id
    level_id += 1

ZOMBIE_TYPES = [
    "Normal",
    "Flag",
    "Conehead",
    "Polevaulter",
    "Buckethead",
    "Newspaper",
    "ScreenDoor",
    "Football",
    "Dancer",
    "BackupDancer",
    "DuckyTube",
    "Snorkel",
    "Zomboni",
    "Bobsled",
    "DolphinRider",
    "JackInTheBox",
    "Balloon",
    "Digger",
    "Pogo",
    "Yeti",
    "Bungee",
    "Ladder",
    "Catapult",
    "Gargantuar",
    "Imp",
    "Boss",
    "PeaHead",
    "WallnutHead",
    "JalapenoHead",
    "GatlingHead",
    "SquashHead",
    "TallnutHead",
    "GigaGargantuar",
    "Zombatar",
    "Target",
    "TrashCan"
]

NO_RANDO_ZOMBIES = ["Normal", "Flag", "DuckyTube", "Yeti", "Target", "Zombatar", "Imp", "Boss", "Bobsled", "BackupDancer"]
POOL_ONLY_ZOMBIES = ["Snorkel", "DolphinRider"]

ALL_PLANTS = [
    "Peashooter",
    "Sunflower",
    "Cherry Bomb",
    "Wall-nut",
    "Potato Mine",
    "Snow Pea",
    "Chomper",
    "Repeater",
    "Puff-shroom",
    "Sun-shroom",
    "Fume-shroom",
    "Grave Buster",
    "Hypno-shroom",
    "Scaredy-shroom",
    "Ice-shroom",
    "Doom-shroom",
    "Lily Pad",
    "Squash",
    "Threepeater",
    "Tangle Kelp",
    "Jalapeno",
    "Spikeweed",
    "Torchwood",
    "Tall-nut",
    "Sea-shroom",
    "Plantern",
    "Cactus",
    "Blover",
    "Split Pea",
    "Starfruit",
    "Pumpkin",
    "Magnet-shroom",
    "Cabbage-pult",
    "Flower Pot",
    "Kernel-pult",
    "Coffee Bean",
    "Garlic",
    "Umbrella Leaf",
    "Marigold",
    "Melon-pult",
    "Gatling Pea",
    "Twin Sunflower",
    "Gloom-shroom",
    "Cattail",
    "Winter Melon",
    "Gold Magnet",
    "Spikerock",
    "Cob Cannon",
    "Imitater"
]

ATTACKING_PLANTS = ["Peashooter", "Chomper", "Snow Pea", "Repeater", "Split Pea", "Cactus", "Cabbage-pult", "Kernel-pult", "Starfruit"]
PROGRESSION_PLANTS = ["Lily Pad", "Flower Pot", "Spikeweed", "Peashooter", "Chomper", "Snow Pea", "Repeater", "Split Pea", "Cactus", "Cabbage-pult", "Kernel-pult", "Starfruit", "Melon-pult", "Winter Melon", "Sunflower", "Sun-shroom", "Wall-nut", "Tall-nut", "Pumpkin", "Torchwood", "Threepeater", "Fume-shroom", "Coffee Bean", "Puff-shroom", "Scaredy-shroom", "Blover", "Cattail", "Magnet-shroom", "Cherry Bomb", "Squash", "Jalapeno", "Grave Buster"]

PLANT_STATS = {
    "Peashooter": {"cost": 100, "refresh": 750, "rate": 150, "health": 300, "projectiles": ["Pea"]},
    "Sunflower": {"cost": 50, "refresh": 750, "rate": 2500, "health": 300},
    "Cherry Bomb": {"cost": 150, "refresh": 5000},
    "Wall-nut": {"cost": 50, "refresh": 3000, "health": 4000},
    "Potato Mine": {"cost": 25, "refresh": 3000, "health": 300},
    "Snow Pea": {"cost": 175, "refresh": 750, "rate": 150, "health": 300, "projectiles": ["Frozen Pea"]},
    "Chomper": {"cost": 150, "refresh": 750, "health": 300},
    "Repeater": {"cost": 200, "refresh": 750, "rate": 150, "health": 300, "projectiles": ["Pea"]},
    "Puff-shroom": {"cost": 0, "refresh": 750, "rate": 150, "health": 300, "projectiles": ["Spore"]},
    "Sun-shroom": {"cost": 25, "refresh": 750, "rate": 2500, "health": 300},
    "Fume-shroom": {"cost": 75, "refresh": 750, "rate": 150, "health": 300},
    "Grave Buster": {"cost": 75, "refresh": 750, "health": 300},
    "Hypno-shroom": {"cost": 75, "refresh": 3000, "health": 300},
    "Scaredy-shroom": {"cost": 25, "refresh": 750, "rate": 150, "health": 300, "projectiles": ["Spore"]},
    "Ice-shroom": {"cost": 75, "refresh": 5000, "health": 300},
    "Doom-shroom": {"cost": 125, "refresh": 5000, "health": 300},
    "Lily Pad": {"cost": 25, "refresh": 750, "health": 300},
    "Squash": {"cost": 50, "refresh": 3000},
    "Threepeater": {"cost": 325, "refresh": 750, "rate": 150, "health": 300, "projectiles": ["Pea"]},
    "Tangle Kelp": {"cost": 25, "refresh": 3000},
    "Jalapeno": {"cost": 125, "refresh": 5000},
    "Spikeweed": {"cost": 100, "refresh": 750, "health": 300},
    "Torchwood": {"cost": 175, "refresh": 750, "health": 300},
    "Tall-nut": {"cost": 125, "refresh": 3000, "health": 8000},
    "Sea-shroom": {"cost": 0, "refresh": 3000, "rate": 150, "health": 300, "projectiles": ["Spore"]},
    "Plantern": {"cost": 25, "refresh": 3000, "health": 300},
    "Cactus": {"cost": 125, "refresh": 750, "rate": 150, "health": 300, "projectiles": ["Spike"]},
    "Blover": {"cost": 100, "refresh": 750},
    "Split Pea": {"cost": 125, "refresh": 750, "rate": 150, "health": 300, "projectiles": ["Pea"]},
    "Starfruit": {"cost": 125, "refresh": 750, "rate": 150, "health": 300, "projectiles": ["Star"]},
    "Pumpkin": {"cost": 125, "refresh": 3000, "health": 4000},
    "Magnet-shroom": {"cost": 100, "refresh": 750, "health": 300},
    "Cabbage-pult": {"cost": 100, "refresh": 750, "rate": 300, "health": 300, "projectiles": ["Cabbage"]},
    "Flower Pot": {"cost": 25, "refresh": 750, "health": 300},
    "Kernel-pult": {"cost": 100, "refresh": 750, "rate": 300, "health": 300, "projectiles": ["Kernel", "Butter"]},
    "Coffee Bean": {"cost": 75, "refresh": 750},
    "Garlic": {"cost": 50, "refresh": 750, "health": 400},
    "Umbrella Leaf": {"cost": 100, "refresh": 750, "health": 300},
    "Marigold": {"cost": 50, "refresh": 3000, "rate": 2500, "health": 300},
    "Melon-pult": {"cost": 300, "refresh": 750, "rate": 300, "health": 300, "projectiles": ["Melon"]},
    "Gatling Pea": {"cost": 250, "refresh": 5000, "rate": 150, "health": 300, "projectiles": ["Pea"], "easy_upgrade_cost": 450},
    "Twin Sunflower": {"cost": 150, "refresh": 5000, "rate": 2500, "health": 300, "easy_upgrade_cost": 200},
    "Gloom-shroom": {"cost": 150, "refresh": 5000, "rate": 200, "health": 300, "easy_upgrade_cost": 225},
    "Cattail": {"cost": 225, "refresh": 5000, "rate": 150, "health": 300, "projectiles": ["Spike"], "easy_upgrade_cost": 250},
    "Winter Melon": {"cost": 200, "refresh": 5000, "rate": 300, "health": 300, "projectiles": ["Frozen Melon"], "easy_upgrade_cost": 500},
    "Gold Magnet": {"cost": 50, "refresh": 5000, "health": 300, "easy_upgrade_cost": 150},
    "Spikerock": {"cost": 125, "refresh": 5000, "health": 450, "easy_upgrade_cost": 450},
    "Cob Cannon": {"cost": 500, "refresh": 5000, "rate": 600, "health": 300, "easy_upgrade_cost": 700}
}

PROJECTILE_STATS = {
    "Pea": {"damage": 20},
    "Frozen Pea": {"damage": 20},
    "Cabbage": {"damage": 40},
    "Melon": {"damage": 80},
    "Spore": {"damage": 20},
    "Frozen Melon": {"damage": 80},
    "Star": {"damage": 20},
    "Spike": {"damage": 20},
    "Kernel": {"damage": 20},
    "Butter": {"damage": 40}
}
ALL_PROJECTILES = list(PROJECTILE_STATS.keys())
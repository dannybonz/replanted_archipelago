from dataclasses import dataclass
import copy

@dataclass
class Zombie:
    name: str
    zombie_id: int
    weight: int = 4000
    value: int = 1
    aquatic: bool = False

    def __post_init__(self):
        self.unmodified = copy.deepcopy(self)

    def randomise(self, world):
        self.weight = world.random.randint(1, 6000)

def create_zombies():
    return {
        "Normal": Zombie(name = "Normal", zombie_id = 0),
        "Flag": Zombie(name = "Flag", zombie_id = 1, weight = 0),
        "Conehead": Zombie(name = "Conehead", zombie_id = 2, value = 2),
        "Polevaulter": Zombie(name = "Polevaulter", zombie_id = 3, weight = 2000, value = 2),
        "Buckethead": Zombie(name = "Buckethead", zombie_id = 4, weight = 3000, value = 4),
        "Newspaper": Zombie(name = "Newspaper", zombie_id = 5, weight = 1000, value = 2),
        "ScreenDoor": Zombie(name = "ScreenDoor", zombie_id = 6, weight = 3500, value = 4),
        "Football": Zombie(name = "Football", zombie_id = 7, weight = 2000, value = 7),
        "Dancer": Zombie(name = "Dancer", zombie_id = 8, weight = 1000, value = 5),
        "BackupDancer": Zombie(name = "BackupDancer", zombie_id = 9, weight = 0),
        "DuckyTube": Zombie(name = "DuckyTube", zombie_id = 10, aquatic = True, weight = 0),
        "Snorkel": Zombie(name = "Snorkel", zombie_id = 11, aquatic = True, weight = 2000, value = 3),
        "Zomboni": Zombie(name = "Zomboni", zombie_id = 12, weight = 2000, value = 7),
        "Bobsled": Zombie(name = "Bobsled", zombie_id = 13, weight = 2000, value = 3),
        "DolphinRider": Zombie(name = "DolphinRider", zombie_id = 14, aquatic = True, weight = 1500, value = 3),
        "JackInTheBox": Zombie(name = "JackInTheBox", zombie_id = 15, weight = 1000, value = 3),
        "Balloon": Zombie(name = "Balloon", zombie_id = 16, weight = 2000, value = 2),
        "Digger": Zombie(name = "Digger", zombie_id = 17, weight = 1000, value = 4),
        "Pogo": Zombie(name = "Pogo", zombie_id = 18, weight = 1000, value = 4),
        "Yeti": Zombie(name = "Yeti", zombie_id = 19, weight = 1, value = 4),
        "Bungee": Zombie(name = "Bungee", zombie_id = 20, weight = 1000, value = 3),
        "Ladder": Zombie(name = "Ladder", zombie_id = 21, weight = 1000, value = 4),
        "Catapult": Zombie(name = "Catapult", zombie_id = 22, weight = 1500, value = 5),
        "Gargantuar": Zombie(name = "Gargantuar", zombie_id = 23, weight = 1500, value = 10),
        "Imp": Zombie(name = "Imp", zombie_id = 24, weight = 0),
        "Boss": Zombie(name = "Boss", zombie_id = 25, weight = 0),
        "PeaHead": Zombie(name = "PeaHead", zombie_id = 26, weight = 4000, value = 1),
        "WallnutHead": Zombie(name = "WallnutHead", zombie_id = 27, weight = 3000, value = 4),
        "JalapenoHead": Zombie(name = "JalapenoHead", zombie_id = 28, weight = 1000, value = 3),
        "GatlingHead": Zombie(name = "GatlingHead", zombie_id = 29, weight = 2000, value = 3),
        "SquashHead": Zombie(name = "SquashHead", zombie_id = 30, weight = 2000, value = 3),
        "TallnutHead": Zombie(name = "TallnutHead", zombie_id = 31, weight = 2000, value = 4),
        "GigaGargantuar": Zombie(name = "GigaGargantuar", zombie_id = 32, weight = 6000, value = 10),
        "Zombatar": Zombie(name = "Zombatar", zombie_id = 33, weight = 0),
        "Target": Zombie(name = "Target", zombie_id = 34, weight = 0),
        "TrashCan": Zombie(name = "TrashCan", zombie_id = 35, weight = 4000, value = 1),
    }
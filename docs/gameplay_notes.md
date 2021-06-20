This is not the game manual.

This is something like a single player game mode design document.

## The Game Goal

In short, player mission is simple: collect N artifacts (N may depend on the game difficulty) before
the in-game day X (X depends on the game difficulty). Then player needs to defeat the main boss.

The problem is, the player starts with a weak fleet and with one star base. A galaxy is quite big
and there are a lot of dangers out there. One can't simply fly through it and find all artifacts
without any troubles.

Losing conditions:

* All Earthling bases are destroyed
* Flagship is destroyed
* N artifacts are not collected before the day X
* The final hostile armada is not defeated

## Gameplay variety

Global map (strategy/tactics):

* Research new technologies
* Build star bases in the right spots
* Keep some garrison inside the frontline bases
* Place mining drones wisely
* Distribute the resources among the bases
* Plan your flagship movement carefully

Random events:

* Simple 1-screen encounters
* (todo) Multi-screen text quests

Diplomacy:

* (todo) Some of your actions affect the faction relation modifier
* (todo) You can communicate with other races

Arcade battles:

* Control the flagship
* (todo) Choose allied vessels behavior mode
* (todo) Choose which vessels will participate in battle
* (todo) Fight boss battles

Exploration:

* Find and recover the artifacts along the world map
* Gain experience and learn new skills

## The Game Phases

* Opening: 0-5% of the mission time
* Early-game: 5-55% of the mission time
* Mid-game: 55-85% of the mission time
* Late-game: 85-100% of the mission time

Depending on a current phase, the game plays a little bit differently.

Obviously, it becomes harder with every phase.

## Flying Through The Galaxy

Traversing from one **star system** to another consumes **fuel**.

There are few ways to recover your fuel:

* Burn **energy resources**: expensive, but can save you some time
* Buy it on a **star base**: relatively inexpensive, but you need to visit a base
* Perform an **idle** action in any system: free, but very slow

![image](https://user-images.githubusercontent.com/6286655/112731247-1b653680-8f47-11eb-91d4-e6d391480e4d.png)

A player can't get stuck in **interstellar space**: you can only peek a destination that is reachable.
It's impossible to wait in interstellar space, neither it is possible to fly somewhere in a middle of nowhere (only systems can be targeted).

## Player Fleet

![image](https://user-images.githubusercontent.com/6286655/112732731-c3c9c980-8f4c-11eb-89e7-41f93e4e5bc8.png)

A fleet is a group of vessels that fly together.

Player can have a fleet of size 6. One flagship (player-controlled) and 5 escort vessels.

Not every vessel has to participate in combat, but flagship is obligated to.

> TODO: or should it be OK to have a bots-vs-bots battle without a player?

The player can choose what vessels are included into the fleet. It can consist of all military crafts
or it can have some freighters to make it easy to carry more resources.

## Planetary Resources

There are 3 basic types of **planetary resources**:

* Minerals. The most common resource.
* Organics. Valuable resource with a good price.
* Energy. Even more valuable, but can also be converted to **fuel**.

Every resource is needed to help a **star base** growth.

As the name suggests, planetary resources can be mined on planets. To do that, **drones** are needed.
Drones can be bought on a star base.

Every star system has 1-3 planets with resourcs. Such planets may have different resources available.

Some notes to keep in mind:

* Planet resources are infinite
* Drone collects some resources each day
* The amount of resources gathered every day vary from planet to planet
* Collected resources must be loaded to a vessel
* There is a resource storage cap that will make the drone stop collecting resources if it's reached

For example, there could be a planet with these properties: minerals-1, organics-3, energy-0.
It means that every day drone will collect 1 mineral and 3 units of organics.

It's up to the player to deploy droids effectively.

## Earthling Star Bases

![image](https://user-images.githubusercontent.com/6286655/112732527-8d3f7f00-8f4b-11eb-81b6-007e1f006d53.png)

Player-controlled bases can be entered.

Every star base has a level.

A star base level affects what you can do on a base and how effectively it operates:

* Max level of equipment it can produce (given that it's researched)
* Max level of ships it can produce

Every base gets some development points every day. When it reaches some threshold,
a new level is granted. This process is slow and player can sped it up by offloading
the resources required by a base.

A base accepts all kinds of resources, but some of them are needed more than others.
Selling the resource that has higher demand will result in a faster base growth.

Base offers these services:

* Equipment store
* Refuelling
* Vessel repairs
* Drone store
* Resources market
* Research
* Vessel production

| Star Base Level | Max Weapon Level |
|---|---|
| 1 | Level 1 normal and special weapons |
| 2 | Level 2 normal weapons |
| 3 | Level 2 normal and special weapons |
| 4 | Level 3 normal weapons |
| 5 | Level 3 normal and special weapons |

| Star Base Level | Max Shield Level |
|---|---|
| 1 | Level 1 |
| 2 | Level 1 |
| 3 | Level 2 |
| 4 | Level 2 |
| 5 | Level 3 |

| Star Base Level | Max Vessel Level 
|---|---|
| 1 | Level 3 (e.g. Earthling Fighter) |
| 2 | Level 4 (e.g. Earthling Interceptor) |
| 3 | Level 5 (e.g. Wertu Guardian) |
| 4 | Level 6 (e.g. Krigia Horns) |
| 5 | Level 7 (e.g. Wertu Dominator) |

> Note: level 8, 9 and 10 vessels can't be produced on a star base.

## Equipment Shop

Theoretically, an equipment shop can contain all game items.

But there are requirements for every item that must be satisfied in order for that item to be available for sale.

Every item has these potential requirements:

* Required research
* Minimal star base level

For example, Krigia `Scythe` weapon requires these researches to be completed:

* `Krigia Weapons I`
* `Scythe`

After that, it will be possible to produce that weapon at any allied star base.

Unique weapons that can be found in space usually require a dedicated research. For example,
when you'll find a `Stormbringer`, there will be a choise: you can either sell it or give it to the
research lab. Researching that item will makes that item instance impossible to use, but
it will be possible to produce them in the future.

## Enemy Star Bases

![image](https://user-images.githubusercontent.com/6286655/112732471-3043c900-8f4b-11eb-96d7-f8e013a053ba.png)

Being in a system that contains a hostile star base is dangerous.

Every day there is a chance that some of the star base fleet ships will attack the player.
The number of attackers depends on the garrison size. If a player uses **attack** action,
that chance is equal to 100%; there will be a battle every day.

If entire garrison is defeated, the base can be destroyed. In order to do that,
player should use **attack** action. That will inflict damage to the base, until it's destroyed.

Note that a base will produce new starships while it's active.

## Researching

Every base can perform a research.

You can order different kinds of researches:

* A study on alien equipment
* Artifact research

Studying alien equipment requires specific vessel debris. When you destroy a vessel of X race, you get some X-specific debris.
You can choose what should be focused: alien weapons or vessel designs.

> Maybe there will be a way to get some technology through a diplomacy, like in Civilization-ish games.

To research an artifact you first need... an artifact. When artifact is researched, it can be used by any vessel. Artifacts can
be found in star systems and in some random encounters.

> TODO: how does one make artifact available for production?

Research tree can be found in a [dedicated document](/docs/research_tree.md).

## Building Star Bases

The player can build new star bases.

Requirements:

* The system should be unoccupied (i.e. it has no other star bases)
* Fleet should have at least 1 **Ark** vessel

If these requirements are satisfied, "build star base" action will be available.

Since Ark is essentially a ship that transforms into a base, it takes no extra time to build it.

After the base is created, Ark ship leaves the fleet (in other words, it's consumed).

A new base starts with a 1st level and no garrison.

## Wandering Units

Player is not the only force in the galaxy that moves from one star system to another.

There are different fleets that may roam the space:

* Unique groups (related to random encounters)
* Star base patrols
* Task forces
* Free roam group (scavengers, for example)
* And some other kinds (described in separate sections)

## Krigia Patrol Units

These units dispatched from a Krigia star base from time to time.

They visit a selected neighbor system, stay there for a while and then go back to the base.

Notes:

* They are the only fair way of Krigia to discover human bases (to launch an assault later)
* Just like scavengers, they leave a system immediately if it has a star base
* Patrol unit stays in the system for a time span comparable with a scavengers raid unit
* They will destroy mining droids
* They may attempt an attack on the player unit if it's in the area

Patrols are usually weak and consist of the light vessels.

A screenshot below illustrates a star base influence area.

![image](https://user-images.githubusercontent.com/6286655/114607251-f7268b00-9ca4-11eb-8027-4e1441988167.png)


## Krigia Task Force Units

If Krigia knows about an enemy star base location, there is a chance that it will launch a fleet to attack it.

Task forces are usually powerfull and consist of the elite vessels.

## Krigia Reinforcement Units

There are two main cases in which one base may send reinforcements to another:

* That base is being attacked
* It plans to launch an attack, but it lacks vessels

In all cases, the base that sends help should be relatively close.

## Random Events

There are 2 kinds of random events:

* One-off events
* Periodic events

One-off events are determined during the new game generation. They are unique, so the same event won't happen twice in a game.

Periodic events are less unique and the same kind of event can happen several times during a game. They're usually less drastic.

> TODO: figure out which kinds of random events we have and how they're resolved.

## Computer-controlled Pilot Ranks

Every computer-controlled vessel has associated bot rank.

Bot rank affects the battle performance.

Higher rank means:

* Better chances to do a snipe shot (with velocity calculations)
* May avoid asteroids more successfully
* Can perform more advanced movement tricks
* More careful energy management
* Less deliberate mistakes (bots make such mistakes sometimes; experienced bots do it very rarely)
* Higher self-preservation (may retreat more when low on health)
* More skillfull shield usage (experienced bot does not use a shield if projectile would miss anyway)

In a single player mode, all Earthling pilots start as rookies and then gain experience through battles.
This makes it more desirable to keep allied pilots alive, so they can improve.

## Earning Credits

Ways to get credits:

* Sell planetary resources (collected by drones)
* Sell vessel debris (collected by fighting)
* Sell equipment
* Some random events can give you credits

> Note: selling equipment only gives credit gains; it doesn't count as giving an equipment for the research.

## Getting Equipment

Ways to get equipment:

* Buy on a friendly star base: limited by a base technological limitations
* Get from a random event

Note that you don't normally get equipment from battles; all you get is sellable (and researchable) debris.

## Upgrades System

This is terribly outdated! See the [sources](https://github.com/quasilyte/quasisolar-mission/blob/master/scripts/nodeless/Skill.cs) to get a better picture.

| Skill | Expr | Effect |
|---|---|---|
| Mechanic I | 150 | When winning in battle, all ships recover 10% of the damage taken during that battle. |
| Mechanic II | 200 | When winning in battle, all ships recover 20% of the damage taken during that battle. |
| Recycling | 250 | When in idle mode, fuel gain is doubled (2 per day). |
| Escape Tactics | 100 | Retreating fuel cost is halved. |
| Siege Mastery I | 50 | When in attack mode, do double damage against bases. |
| Siege Mastery II | 100 | When in attack mode, do 1 point of damage even if the garrison is not empty. |
| Navigation I | 100 | Map travelling is 10% faster. |
| Navigation II | 100 | Map travelling is 15% faster. |
| Navigation III | 125 | Map travelling is 20% faster. |
| Diplomacy | 75 | Better deals in negotiations. |
| Luck | 50 | Improves a chance of getting better random event. Also affects some rewards. |
| Fighter | 50 | 25% more experience from battles. |
| Mining I | 75 | Drones max resource limit increased by 25. |
| Mining II | 100 | Drones max resource limit increased by 50. |

## Earning Experience

Ways to get experience points:

* Winning in battles
* Some random events can give you experience

## Scavengers

Scavengers are the local "space pirates". They attack weak bases and fleets in attempt to scavenge their resources.

They don't have own star bases.

> TODO: explain where and when do they spawn.

## Scavenger Bases

* Keeps 10 garrison ships for defense
* Not attacked by any other race (but player can destroy them)

Scavenger base activity:

* Creates scavenger ships using its resources
* Spawns a **raid unit** when it has enough vessels

Scavenger bases get resources over time and by receiving resources from the raid units.

## Scavenger Raid Unit

A small fleet of Scavenger ships that flies in a chaotic manner through the map.

They leave systems with star bases immediately while they can stay for several days on neutral systems.

If they find a system with player-controlled mines, they'll grab as many resources as they can
and fly to the nearest scavenger base. It's possible to intercept them and get some resources back.

When player fleet meets the raid unit inside one system, a special "Scavengers" event appears.
Depending on the player fleet power, they may be inclined to attack or not.
Player can always choose to engage them in combat even if they don't want to attack.

## Diplomacy

Initial reputation scores:

| Faction | Starting Value |
|---|---|
| Krigia | -25 |
| Wertu | 10 |
| Zyth | -10 |
| Vespion | 0 |

There are several ways to improve the relationships with a faction:

* Provide resources
* Assist in battles
* Complete text quests
* Choose appropriate actions in random events

A player can form an alliance with these races:

* Wertu
* Vespion

An alliance can be formed when reputation reaches 25.

## Bosses

Although we cover **battles** in a different document, bosses will be described here.

### The purple system visitor

Difficulty: normal.

Notable rewards: **crystal cannon** weapon blueprint.

A battle station that can only move by using its warp device.

Tactics: although it has artifacts that make **ion cannons** less effective, a combined fire from several
**ion cannons** with one or more **disruptors** can make it suffer from the energy hunger.

Since it can't move around normally, it's an easy target for most weapons.

## Battles

![image](https://user-images.githubusercontent.com/6286655/112731615-5c5e4a80-8f49-11eb-8c1f-ca38a423ff3c.png)

Battles are a big part of a game and they shoud be covered in a separate document.

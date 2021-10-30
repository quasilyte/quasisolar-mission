This is not the game manual.

This is something like a single player game mode design document.

## The Game Goal

In short, the player mission is simple: survive until the final enemy comes and defeat that enemy.

The problem is, the player starts with a weak fleet and with one star base. A galaxy is quite big
and there are a lot of dangers out there.

Losing conditions:

* All Earthling bases are destroyed
* The fleet controlled by the player is destroyed

## Gameplay variety

* Research new technologies
* Build star bases in the right spots
* Keep some garrison inside the frontline bases
* Build base modules wisely
* Distribute the resources among the bases
* Plan your fleet movements carefully
* Lots of random events and encounters
* Communicate with other factions
* Your actions can affect other factions reputation modifier
* Do quests for other factions
* Build alliances
* Fight in the arcade fleet battles
* Fight in boss battles
* Find and recover the artifacts along the world map
* Gain experience and learn new skills

## Flying Through The Galaxy

Traversing from one **star system** to another consumes **fuel**.

There are few ways to recover your fuel:

* Burn **energy resources**: expensive, but can save you some time
* Buy it on a **star base**: relatively inexpensive, but you need to visit a base
* Perform an **idle** action in any system: free, but very slow
* Sometimes you can get some fuel out of random events
* Sometimes you can get some fuel from the battle encounters

![image](https://user-images.githubusercontent.com/6286655/112731247-1b653680-8f47-11eb-91d4-e6d391480e4d.png)

A player can't get stuck in **interstellar space**: you can only peek a destination that is reachable.
It's impossible to wait in interstellar space, neither it is possible to fly somewhere in a middle of nowhere (only systems can be targeted).

## Player Fleet

![image](https://user-images.githubusercontent.com/6286655/112732731-c3c9c980-8f4c-11eb-89e7-41f93e4e5bc8.png)

A fleet is a group of vessels that fly together.

Player can have a fleet of size 4. One flagship (player-controlled) and 3 escort vessels.

Not every vessel has to participate in combat, but flagship is obligated to.

> TODO: or should it be OK to have a bots-vs-bots battle without a player?

The player can choose what vessels are included into the fleet. It can consist of all military crafts
or it can have some freighters to make it easy to carry more resources.

## Planetary Resources

There are 3 basic types of **planetary resources**:

* Minerals. The most common resource.
* Organics. Valuable resource with a good price.
* Energy. Even more valuable; can also be converted to **fuel**.

Every resource is needed to help a **star base** growth.

Each star system has 1-3 planets with resourcs. Such planets may have different resources available.

A planet needs to be **explored** before it can be **mined**.

In order to explore the planet, **exploration drone** needs to be used. They can be
bought on a friendly star base.

After the planet is explored, its resources can be mined by a star base with an
appropriate **collector** module. For every resource type, there is a collector module.

A star base can earn money by converting the excessive resources into **RU** (universal **resource units**).
An associated **refinery** module should be used for that.

So, a collector+refinery combination can be used to both mine and utilize a resource.

Some notes to keep in mind:

* Planet resources are infinite
* The amount of resources gathered every day vary from planet to planet

For example, there could be a planet with these properties: minerals-1, organics-3, energy-0.
It means that evert day a collector module can gather 1 mineral and 3 units of organics.

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

Base offers these services:

* Equipment store
* Refuelling
* Vessel repairs
* Exploration drones store
* Resources storage
* Vessel production
* Vessel updates installation
* Base modules construction

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
* If an equipment is unique, it needs to be discovered first

For example, the Krigia `Scythe` weapon requires these researches to be completed:

* `Krigia Weapons I`
* `Scythe`

After that, it will be possible to produce that weapon at any allied star base.

When you find an artifact, you can't use it straight away. You need to research
it first. After the research you'll get the artifact itself available for use
as well as an ability to produce them on your star base.

## Vessel modules (mods)

How to acquire mods:

* In a random events
* Get as a quest reward
* Buy from a mod trader

## Enemy Star Bases

![image](https://user-images.githubusercontent.com/6286655/112732471-3043c900-8f4b-11eb-96d7-f8e013a053ba.png)

Being in a system that contains a hostile star base is dangerous.

Every day there is a chance that some of the star base fleet ships will attack the player.

If a player uses **attack** action, that chance is equal to 100%; there will be a battle every day.

If entire garrison is defeated, the base can be destroyed. In order to do that,
player should use **attack** action. That will inflict damage to the base, until it's destroyed.

Note that a base will produce new starships while it's active.

## Researching

You can order different kinds of researches:

* A study on alien equipment
* Artifact research
* Some fundamental or Earthling-based technologies

Studying alien technologies requires their **research material**.

Ways to get that X faction material:

* Destroy X faction vessels in battle
* Trade that material from X faction (no war required)
* Get some during the random events (unreliable in general)

## Building Star Bases

The player can build new star bases.

Requirements:

* The system should be unoccupied (i.e. it has no other star bases)
* Fleet should have at least 1 **Ark** vessel

If these requirements are satisfied, "build star base" action will be available.

Since Ark is essentially a ship that transforms into a base, it takes no extra time to build it.

After the base is created, Ark ship leaves the fleet (in other words, it's consumed).

A new base starts with a 1st level, no garrison and no modules.

## Krigia Patrol Units

These units dispatched from a Krigia star base from time to time.

They visit a selected neighbor system, stay there for a while and then go back to the base.

Notes:

* They are the only fair way of Krigia to discover human bases (to launch an assault later)
* Just like Draklids, they leave a system immediately if it has a star base
* Patrol unit stays in the system for a time span comparable with a Draklid raid unit
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

* Complete quests
* Build the refinery base modules
* Sell vessel debris (collected by fighting)
* Sell equipment
* Some random events can give you credits
* Explore planets with drones

## Getting Equipment

Ways to get equipment:

* Buy on a friendly star base: limited by a base technological limitations
* Get from a random event

Note that you don't normally get equipment from battles; all you get is sellable (and researchable) debris.

## Bosses

Although we cover **battles** in a different document, bosses will be described here.

### The purple system visitor

Difficulty: normal.

Notable rewards: **crystal cannon** weapon blueprint.

A battle station that can only move by using its warp device.

Tactics: although it has artifacts that make **ion cannons** less effective, a combined fire from several
**ion cannons** with one or more **disruptors** can make it suffer from the energy hunger.

Since it can't move around normally, it's an easy target for most weapons.

### The Spectre

TODO

### Krigia Flagship

TODO

## Battles

![image](https://user-images.githubusercontent.com/6286655/112731615-5c5e4a80-8f49-11eb-8c1f-ca38a423ff3c.png)

Battles are a big part of a game and they shoud be covered in a separate document.
